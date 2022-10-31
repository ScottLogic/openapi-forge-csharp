using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick;

namespace Features
{
    [FeatureFile(nameof(HTTPVerbs) + Constants.FeatureFileExtension)]
    public sealed class HTTPVerbs : FeatureBase
    {
        public MockedRequest request;

        public HTTPVerbs(ITestOutputHelper testOutputHelper) :
            base(testOutputHelper)
        {
        }

        [When(@"calling the spied method (\w+) without params")]
        public async Task
        CallWithoutParameters(string methodName)
        {
            await CallMethod(methodName);
        }

        [Then(@"the request method should be of type (.*)")]
        public void CheckRequestMethod(string value)
        {
            Assert.Equal(new HttpMethod(value), _request.Method);
        }
    }
}
