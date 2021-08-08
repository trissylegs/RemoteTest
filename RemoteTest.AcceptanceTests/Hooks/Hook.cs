using System.Net;
using BoDi;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using RemoteTest.AcceptanceTests.Drivers;
using RestSharp;
using TechTalk.SpecFlow;

namespace RemoteTest.AcceptanceTests.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly TestContext _testContext;

        public Hooks(TestContext testContext)
        {
            _testContext = testContext;
        }

        [BeforeScenario(Order = 1)]
        public void RegisterSslCertificateHandler()
        {
            // Need to pass self-signed certs for testing.
            // TODO: Only pass our self-signed certificates. 
            ServicePointManager.ServerCertificateValidationCallback += 
                (sender, certificate, chain, errors) => true;
        }
        
        [BeforeScenario(Order = 2)]
        public void RegisterDependencies(IObjectContainer objectContainer)
        {
            var url = "https://localhost:5001/";
            var connectionString = "data source=./Production.db";
            var webServerDriver = new WebServerDriver(url, connectionString);
            objectContainer.RegisterInstanceAs(webServerDriver);
            objectContainer.RegisterInstanceAs(new RestClient(webServerDriver.Url));
            objectContainer.RegisterTypeAs<ApiDriver, ApiDriver>();

            objectContainer.RegisterFactoryAs((oc) =>
            {
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection;
            });
            objectContainer.RegisterTypeAs<SqliteDriver, IDbDriver>();
        }
    }
}