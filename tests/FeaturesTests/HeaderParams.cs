using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(HeaderParams) + Constants.FeatureFileExtension)]
    public sealed class HeaderParams : FeatureBase
    {
        public HeaderParams(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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

        [Then(@"the request should have a header property with value (.+)")]
        public void CheckHeaderPropertyAsync(string headerProperty)
        {
            Assert.NotNull(_request);
            IEnumerable<string> keys = null;
            if (!_request.Headers.TryGetValues("test", out keys))
                Assert.True(false, "No test parameter found");
            Assert.Equal(headerProperty, keys.First());
        }
    }
}
