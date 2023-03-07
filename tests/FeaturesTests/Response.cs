using System;
using System.Text.RegularExpressions;
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

        protected dynamic _extractedResponse;

        public Response(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public void Dispose()
        {
            _extractedResponse = null;
        }

        [Then(@"the response should be of type (\w+)")]
        public void CheckResponseType(string expected)
        {
            var type = _extractedResponse != null ? _extractedResponse.GetType() : _actual.Data.GetType();
            Assert.EndsWith(expected, type.Name);
        }

        [And(@"the response should have a property (id|value) with value (.+)")]
        public void CheckResponseIdProperty(string propName, string propValue)
        {
            var property = _extractedResponse != null ? _extractedResponse : _actual.Data;
            var propInfo = property.GetType().GetProperty(propName);
            Assert.NotNull(propInfo);
            Assert.Equal(propValue, propInfo.GetValue(property).ToString());
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
            var propInfo = _actual.Data.GetType().GetProperty(propertyName);
            Assert.NotNull(propInfo);
            Assert.Equal(DateTime.Parse(expectedPropValue).ToUniversalTime(), propInfo.GetValue(_actual.Data));
        }

        [And(@"the response should be equal to (""[\w\s]+"")")]
        public void CheckStringResponseValue(string expectedResponse)
        {
            Assert.Equal(expectedResponse, _actual.Data);
        }

        [Then(@"the response should be an array")]
        public void CheckArrayResponseType()
        {
            Assert.True(_actual.Data.GetType().IsArray);
        }

        [When(@"calling the method (\w+) and the server provides an empty response")]
        public async Task WhenEmptyResponse(string methodName)
        {
            await CallMethod(methodName);
        }

        [When(@"calling the method (\w+) and the server responds with headers")]
        public async Task WhenEmptyResponse(string methodName, DocString headers)
        {
            _responses[methodName.Replace("Response", string.Empty)] = "";

            // we'll assume a single header for now
            var header = headers.Content.Split(":");
            var headerName = Regex.Match(header[0], "\"(.*)\"").Groups[1].Value;
            var headerValue = Regex.Match(header[1], "\"(.*)\"").Groups[1].Value;

            _mockHttp.When("*").Respond((HttpRequestMessage request) =>
            {
                var message = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(_responses[request.RequestUri.LocalPath.Split("/").Last()]),
                };
                message.Headers.Add(headerName, headerValue);
                return Task.FromResult<HttpResponseMessage>(message);
            });

            var request = _mockHttp.When("*").Respond("application/json", "");
            var apiClient = _testHelper.CreateApiClient(_mockHttp.ToHttpClient());

            var methodInfo = apiClient.GetType().GetMethod(methodName);

            dynamic awaitable = methodInfo.Invoke(apiClient, null);
            await awaitable;
            _actual = awaitable.GetAwaiter().GetResult();
        }

        [Then(@"the response should be null")]
        public void ThenResponseTypeIsTask()
        {
            Assert.Null(_actual.Data);
        }

        [When(@"extracting the object at index (\d)")]
        public void CheckArrayResponseType(string index)
        {
            _extractedResponse = ((object[])_actual.Data)[int.Parse(index)];
        }

        [Then(@"the response should have a property (cats) with value (\d+)")]
        [And(@"the response should have a property (dogs) with value (\d+)")]
        public void CheckResponseIntDictionaryProperties(string propertyName, string expectedPropValue)
        {
            var actual = _actual.Data as Dictionary<string, int>;
            Assert.NotNull(actual);
            Assert.Equal(int.Parse(expectedPropValue), actual[propertyName]);
        }

        [Then(@"the response should have a header (.+) with value (.+)")]
        [And(@"the response should have a header (.+) with value (.+)")]
        public void CheckResponseHeaderProperty(string propertyName, string expectedPropValue)
        {
            Assert.Equal(expectedPropValue, _actual.Headers.GetValues(propertyName)[0]);
        }

        [Then(@"the response should have a property (dateOne) with value ([\d-:.TZ]+)")]
        [And(@"the response should have a property (dateTwo) with value ([\d-:.TZ]+)")]
        public void CheckResponseDateDictionaryProperties(string propertyName, string expectedPropValue)
        {
            var actual = _actual.Data as Dictionary<string, DateTime>;
            Assert.NotNull(actual);
            Assert.Equal(DateTime.Parse(expectedPropValue).ToUniversalTime(), actual[propertyName]);
        }
    }
}