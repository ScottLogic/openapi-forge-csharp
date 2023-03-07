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
