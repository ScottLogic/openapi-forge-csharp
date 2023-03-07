using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(Components) + Constants.FeatureFileExtension)]
    public sealed class Components : FeatureBase
    {
        public Components(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [When(@"calling the method (\w+) with (?:object|array|parameters) ""(.+)""")]
        public async Task CallMethodWithParameters(string methodName, string rawParameters)
        {
            var paramStringValues = rawParameters.Split(",");
            var parameters = new object[]
            {
                paramStringValues[0],
                int.TryParse(paramStringValues[1], out var parsed) ? new Nullable<int>(parsed) : null
            };

            await CallMethod(methodName, parameters);
        }
    }
}
