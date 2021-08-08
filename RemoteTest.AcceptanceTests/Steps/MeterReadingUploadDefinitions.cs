using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NUnit.Framework;
using RemoteTest.AcceptanceTests.Drivers;
using RestSharp;
using TechTalk.SpecFlow;

namespace RemoteTest.AcceptanceTests.Steps
{
    [Binding]
    public sealed class MeterReadingUploadDefinitions
    {
        private readonly ApiDriver _apiDriver;
        private readonly IDbDriver _dbDriver;
        
        private IRestResponse _response;

        public MeterReadingUploadDefinitions(ApiDriver apiDriver, IDbDriver dbDriver)
        {
            _apiDriver = apiDriver;
            _dbDriver = dbDriver;
        }

        [When(@"POST request is made to ""(.*)"" with:")]
        public void WhenPostRequestIsMadeToWith(string path, string multilineText)
        {
            _response = _apiDriver.PostCsv(path, multilineText);
            
        }

        [Then(@"the response code should be (.*)")]
        public void ThenTheResponseCodeShouldBe(int code)
        {
            if (_response.ResponseStatus != ResponseStatus.Completed)
            {
                switch (_response.ResponseStatus)
                {
                    case ResponseStatus.None:
                        Assert.Fail("Request was never sent");
                        break;
                    case ResponseStatus.Error:
                        // We expect this to not be null. But we want to fail this test giving a clear error.
                        Assert.IsNull(_response.ErrorException);
                        break;
                    case ResponseStatus.TimedOut:
                        Assert.Fail("Request timed out.");
                        break;
                    case ResponseStatus.Aborted:
                        Assert.Fail("Request was aborted.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            Assert.That(_response.ResponseStatus, Is.EqualTo(ResponseStatus.Completed), "Error completing HTTP request {0}", _response.ErrorMessage);
            Assert.That(_response.StatusCode, Is.EqualTo((HttpStatusCode) code));
        }

        [Given(@"table ""([^""]+)"" is empty")]
        public void GivenTableIsEmpty(string tableName)
        {
            _dbDriver.ClearTable(tableName);
        }

        [Then(@"table ""([^""]+)"" has (\d.*) rows")]
        public void ThenTableHasEntry(string tableName, long rows)
        {
            var actual = _dbDriver.RowCount(tableName);
            
            Assert.That(actual, Is.EqualTo(rows));
        }

        /// <summary>
        /// Check contents of a table. Assumes table is ordered by primary key.
        /// </summary>
        /// <param name="tableName">Name of database table.</param>
        /// <param name="table">Rows of database.</param>
        [Then(@"table ""(.*)"" has data:")]
        public void ThenTableHasData(string tableName, Table table)
        {
            _dbDriver.CheckContents(tableName, table);
        }

        [Then(@"Response Body JSON is:")]
        public void ThenResponseBodyJsonIs(string multilineString)
        {
            Assert.That( _response.ContentType, Does.StartWith("application/json;"));

            var expected = JsonConvert.DeserializeObject<Dictionary<string, object>>(multilineString);
            var actual = JsonConvert.DeserializeObject<Dictionary<string, object>>(_response.Content);
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}