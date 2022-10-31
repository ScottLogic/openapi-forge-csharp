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

        [When(@"calling the method (\w+) with (array|parameters) ""(.+)""")]
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

        [When(@"calling the method (\w+) without params")]
        public async Task CallMethodWithStringParameters(string methodName)
        {
            await CallMethod(methodName);
        }
    }
}
