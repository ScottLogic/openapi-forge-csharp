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

        private int? _serverIndex;

        private readonly Dictionary<string, string> _responses = new Dictionary<string, string>();

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

        [When(@"calling the method (\w+) and the server responds with")]
        public async Task CallWithResponse(string methodName, DocString response)
        {
            _responses[methodName.Replace("Response", string.Empty)] = response.Content;

            _mockHttp.When("*").Respond((HttpRequestMessage request) =>
            {
                return Task.FromResult<HttpResponseMessage>(
                    new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(_responses[request.RequestUri.LocalPath.Split("/").Last()])
                    });
            });

            var request = _mockHttp.When("*").Respond("application/json", response.Content);
            var apiClient = _testHelper.CreateApiClient(_mockHttp.ToHttpClient());

            var methodInfo = apiClient.GetType().GetMethod(methodName);

            dynamic awaitable = methodInfo.Invoke(apiClient, null);
            await awaitable;
            _actual = awaitable.GetAwaiter().GetResult();
        }

        [Then(@"the response should be of type (\w+)")]
        public void CheckResponseType(string type)
        {
            Assert.EndsWith(type, _actual.Data.GetType().Name);
        }

        [When(@"calling the method (\w+) without params")]
        public async Task CallWithoutParameters(string methodName)
        {
            await CallMethod(methodName, null, null, _serverIndex);
        }

        [When(@"calling the method (\w+) with object (.+)")]
        public async Task CallMethodWithStringObject(string methodName, string parametersString)
        {
            var parameters = new object[] { _testHelper.JsonToTypeInstance("InlineObject1", parametersString) };

            await CallMethod(methodName, parameters);
        }

        [When(@"calling the method (\w+) with (object|array|parameters) ""(.+)""")]
        public async Task CallMethodWithStringParameters(string methodName, string paramType, string parametersString)
        {
            var parameters = paramType switch
            {
                "object" => new object[] { _testHelper.JsonToTypeInstance("InlineObject1", parametersString) },
                "array" => new object[] { parametersString.Split(",") },
                _ => parametersString.Split(",")
            };

            await CallMethod(methodName, parameters);
        }

        [When(@"selecting the server at index (\d)")]
        public void SelectedServerIndexIsOne(string serverIndex)
        {
            _serverIndex = new Nullable<int>(int.Parse(serverIndex));
        }


        [Then(@"the response should have a property (.+) with value (.+)")]
        [And(@"the response should have a property (.+) with value (.+)")]
        public void CheckResponseIdProperty(string propName, string propValue)
        {
            var propInfo = _actual.Data.GetType().GetProperty(propName);
            Assert.NotNull(propInfo);
            Assert.Equal(propValue, propInfo.GetValue(_actual.Data).ToString());
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