using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    public abstract class FeatureBase : Xunit.Gherkin.Quick.Feature
    {
        protected readonly ITestOutputHelper _testOutputHelper;

        protected readonly TestHelper _testHelper;

        protected readonly MockHttpMessageHandler _mockHttp;

        protected readonly string _testId;

        protected dynamic _actual;

        protected string _docStringContent;

        protected HttpRequestMessage _request;

        public FeatureBase(ITestOutputHelper testOutputHelper)
        {
            _testId = System.Guid.NewGuid().ToString().Substring(0, 8);
            _testOutputHelper = testOutputHelper;
            _testHelper = new TestHelper(_testId);
            _mockHttp = new MockHttpMessageHandler();
        }

        [Given(@"an API with the following specification")]
        public virtual void Generate(DocString schema)
        {
            Assert.False(string.IsNullOrWhiteSpace(schema.Content),
                $"Parameter '{nameof(schema)}' must not be null or whitespace");
            _docStringContent = schema.Content;
            _testHelper.GenerateApi(schema.Content);
        }

        [Then(@"the requested URL should be (.+)")]
        public void CheckRequestUri(string url)
        {
            var expected = new Uri(url);
            var actual = new Uri(_actual.Data.ToString());
            Assert.Equal(expected.Host, actual.Host);
            Assert.Equal(expected.AbsolutePath, actual.AbsolutePath);
            var separators = new[] { '?', '&' };
            Assert.Equal(expected.Query.Split(separators).OrderBy(s => s),
                actual.Query.Split(separators).OrderBy(s => s));
        }

        protected async Task CallMethod(string methodName, object[] parameters = null, string response = null,
            int? serverIndex = 0)
        {
            _mockHttp.When("*").Respond((HttpRequestMessage request) =>
            {
                InterceptRequest(request);

                return Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(response ?? request.RequestUri.AbsoluteUri.ToString()),
                        RequestMessage = request
                    });
            });

            var apiClient = _testHelper.CreateApiClient(_mockHttp.ToHttpClient(), serverIndex);

            var methodInfo = apiClient.GetType().GetMethod(methodName);

            var methodParameters = methodInfo.GetParameters();

            dynamic awaitable = methodInfo.Invoke(apiClient, GetMethodArgs(methodParameters, parameters));
            await awaitable;
            if (methodInfo.ReturnType.GenericTypeArguments.Length > 0)
            {
                _actual = awaitable.GetAwaiter().GetResult();
            }
        }

        private static object[] GetMethodArgs(ParameterInfo[] paramInfos, object[] arguments)
        {
            if (paramInfos.Length == 0)
            {
                return null;
            }

            if (arguments == null)
            {
                arguments = new object[paramInfos.Length];
            }
            else if (paramInfos.Length > arguments.Length)
            {
                Array.Resize(ref arguments, paramInfos.Length);
            }

            return paramInfos
                .Zip(arguments, (paramInfo, arg) => arg != null
                    ? TryParsePrimitiveArgs(arg, paramInfo.ParameterType)
                    : (paramInfo.HasDefaultValue ? paramInfo.DefaultValue : null))
                .ToArray();
        }

        private void InterceptRequest(HttpRequestMessage request)
        {
            _request = request;
        }

        private static object TryParsePrimitiveArgs(object arg, Type paramType)
        {
            switch (Type.GetTypeCode(paramType))
            {
                case TypeCode.Int32:
                    return int.Parse(arg.ToString());
                case TypeCode.Int64:
                    return long.Parse(arg.ToString());
                case TypeCode.Single:
                    return float.Parse(arg.ToString());
                case TypeCode.Double:
                    return double.Parse(arg.ToString());
                case TypeCode.Boolean:
                    return bool.Parse(arg.ToString());
                case TypeCode.DateTime:
                    return DateTime.Parse(arg.ToString());
                default:
                    return arg;
            }

            ;
        }
    }
}