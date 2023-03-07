using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(Configuration) + Constants.FeatureFileExtension)]
    public sealed class Configuration : FeatureBase
    {

        public Configuration(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}