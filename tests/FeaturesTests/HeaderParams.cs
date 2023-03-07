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
