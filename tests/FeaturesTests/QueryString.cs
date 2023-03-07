using Gherkin.Ast;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(QueryString) + Constants.FeatureFileExtension)]
    public sealed class QueryString : FeatureBase
    {
        public QueryString(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
