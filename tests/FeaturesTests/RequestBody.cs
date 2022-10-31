using System.Diagnostics;
using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(RequestBody) + Constants.FeatureFileExtension)]
    public sealed class RequestBody : FeatureBase
    {
        public RequestBody(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        [When(@"calling the method (\w+) with (?:object|array|parameters) (.+)")]
        public async Task CallMethodWithStringParameters(string methodName, string parametersString)
        {
            var inlineObj = _testHelper.JsonToTypeInstance("ObjectResponse", parametersString);

            await CallMethod(methodName, new object[] { inlineObj });
        }

        [And(@"the request should have a body with value (.+)")]
        public async Task CheckRequestBody(string propValue)
        {
            Assert.NotNull(_request);
            var body = await _request.Content.ReadAsStringAsync();
            Assert.NotNull(body);
            Assert.Equal(propValue, body);
        }
    }
}
