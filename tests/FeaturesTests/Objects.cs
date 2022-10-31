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

        [And(@"the response should have a property (id|value) with value (.+)")]
        public void CheckResponseIdProperty(string propName, string propValue)
        {
            var propInfo = _actual.GetType().GetProperty(propName);
            Assert.NotNull(propInfo);
            Assert.Equal(propValue, propInfo.GetValue(_actual).ToString());
        }

        [When(@"calling the method (\w+) with (?:object|array|parameters) (.+)")]
        public async Task CallMethodWithParameters(string methodName, string jsonTextObject)
        {
            var inlineObj = _testHelper.JsonToTypeInstance("InlineObject1", jsonTextObject);

            await CallMethod(methodName, new object[] { inlineObj });
        }

        [And("the request should have a body with value (.+)")]
        public async Task CheckRequestBody(string propValue)
        {
            Assert.NotNull(_request);
            var body = await _request.Content.ReadAsStringAsync();
            Assert.NotNull(body);
            Assert.Equal(propValue, body);
        }
    }
}
