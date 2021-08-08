using System.Threading.Tasks;
using RemoteTest.AcceptanceTests.Drivers;
using TechTalk.SpecFlow;

namespace RemoteTest.AcceptanceTests.Hooks
{
    [Binding]
    public class DriverHooks
    {
        private readonly WebServerDriver _webServerDriver;

        public DriverHooks(WebServerDriver webServerDriver)
        {
            _webServerDriver = webServerDriver;
        }
        
        [BeforeScenario(Order = 1000)]
        public void RegisterRestClient()
        {
            _webServerDriver.Start();
        }
        
        [AfterScenario()]
        public async Task StopWebserver()
        {
            await _webServerDriver.Stop();
        }
    }
}