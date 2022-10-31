using System;
using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(Response) + Constants.FeatureFileExtension)]
    public sealed class Response : FeatureBase
    {
        private readonly Dictionary<string, string> _responses = new Dictionary<string, string>();

        public Response(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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
            Assert.EndsWith(type, _actual.GetType().Name);
        }

        [And(@"the response should have a property (id|value) with value (.+)")]
        public void CheckResponseIdProperty(string propName, string propValue)
        {
            var propInfo = _actual.GetType().GetProperty(propName);
            Assert.NotNull(propInfo);
            Assert.Equal(propValue, propInfo.GetValue(_actual).ToString());
        }

        [When(@"calling the method (\w+) with parameters (.+)")]
        public async Task CallMethodWithParameters(string methodName, string rawParameters)
        {
            var paramStringValues = rawParameters.Split(",");
            var parameters = new object[]
            {
                paramStringValues[0], int.TryParse(paramStringValues[1], out var parsed)
                    ? new Nullable<int>(parsed)
                    : null
            };

            await CallMethod(methodName, parameters);
        }

        [And(@"the response should have a property (date|dateTime) with value ([\d-:.TZ]+)")]
        public void CheckDateValueProperty(string propertyName, string expectedPropValue)
        {
            var propInfo = _actual.GetType().GetProperty(propertyName);
            Assert.NotNull(propInfo);
            Assert.Equal(DateTime.Parse(expectedPropValue).ToUniversalTime(), propInfo.GetValue(_actual));
        }

        [And(@"the response should be equal to (""[\w\s]+"")")]
        public void CheckStringResponseValue(string expectedResponse)
        {
            Assert.Equal(expectedResponse, _actual);
        }

        [Then(@"the response should be an array")]
        public void CheckArrayResponseType()
        {
            Assert.True(_actual.GetType().IsArray);
        }

        [When(@"calling the method (\w+) and the server provides an empty response")]
        public async Task WhenEmptyResponse(string methodName)
        {
            await CallMethod(methodName);
        }

        [Then(@"the response should be null")]
        public void ThenResponseTypeIsTask()
        {
            Assert.Null(_actual);
        }

        [When(@"extracting the object at index (\d)")]
        public void CheckArrayResponseType(string index)
        {
            _actual = ((object[])_actual)[int.Parse(index)];
        }

        [Then(@"the response should have a property (cats) with value (\d+)")]
        [And(@"the response should have a property (dogs) with value (\d+)")]
        public void CheckResponseIntDictionaryProperties(string propertyName, string expectedPropValue)
        {
            var actual = _actual as Dictionary<string, int>;
            Assert.NotNull(actual);
            Assert.Equal(int.Parse(expectedPropValue), actual[propertyName]);
        }

        [Then(@"the response should have a property (dateOne) with value ([\d-:.TZ]+)")]
        [And(@"the response should have a property (dateTwo) with value ([\d-:.TZ]+)")]
        public void CheckResponseDateDictionaryProperties(string propertyName, string expectedPropValue)
        {
            var actual = _actual as Dictionary<string, DateTime>;
            Assert.NotNull(actual);
            Assert.Equal(DateTime.Parse(expectedPropValue).ToUniversalTime(), actual[propertyName]);
        }
    }
}