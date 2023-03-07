using System.Diagnostics;
using Gherkin.Ast;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(PathParams) + Constants.FeatureFileExtension)]
    public sealed class PathParams : FeatureBase
    {
        public PathParams(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
