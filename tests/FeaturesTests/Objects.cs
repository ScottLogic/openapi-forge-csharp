using System.Diagnostics;
using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(Objects) + Constants.FeatureFileExtension)]
    public sealed class Objects : FeatureBase
    {

        public Objects(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [When(@"calling the method (\w+) and the server responds with")]
        public async Task CallWithResponse(string methodName, DocString response)
        {
            await CallMethod(methodName, null, response.Content);
        }

        [Then(@"the response should be of type (\w+)")]
        public void CheckResponseType(string type)
        {
            Assert.EndsWith(type, _actual.GetType().Name);
        }

        [And(@"the response should have a property id with value (\d+)")]
        public void CheckResponseIdProperty(string propValue)
        {
            var propInfo = _actual.GetType().GetProperty("id");
            Assert.NotNull(propInfo);
            Assert.Equal(int.Parse(propValue), propInfo.GetValue(_actual));
        }

        [And(@"the response should have a property value with value (\w+)")]
        public void CheckResponseValueProperty(string propValue)
        {
            var propInfo = _actual.GetType().GetProperty("value");
            Assert.NotNull(propInfo);
            Assert.Equal(propValue, propInfo.GetValue(_actual));
        }

        [When(@"calling the method (\w+) with parameters ""(.+)""")]
        public async Task CallMethodWithParameters(string methodName, string jsonTextObect)
        {
            var inlineObj = _testHelper.JsonToTypeInstance("InlineObject1", jsonTextObect);

            await CallMethod(methodName, new object[] { inlineObj });
        }

        [And("the request should have a body with value \"(.+)\"")]
        public async Task CheckRequestBody(string propValue)
        {
            Assert.NotNull(_request);
            var body = await _request.Content.ReadAsStringAsync();
            Assert.NotNull(body);
            body = body.Replace("\"", "'");
            Assert.Equal(propValue, body);
        }
    }
}