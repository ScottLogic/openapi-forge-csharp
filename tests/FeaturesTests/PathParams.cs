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

        [When(@"calling the method (\w+) with (parameters|array) ""(.+)""")]
        public async Task CallMethodWithStringParameters(string methodName, string paramType, string parametersString)
        {
            var parameters = paramType switch
            {
                "array" => new object[] { parametersString.Split(",") },
                _ => parametersString.Split(",")
            };

            await CallMethod(methodName, parameters);
        }

        [When(@"calling the method (\w+) with object (.+)")]
        public async Task CallMethodWithStringObject(string methodName, string parametersString)
        {
            var parameters = new object[] { _testHelper.JsonToTypeInstance("InlineObject1", parametersString) };

            await CallMethod(methodName, parameters);
        }
    }
}
