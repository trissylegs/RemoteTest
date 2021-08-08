using RestSharp;

namespace RemoteTest.AcceptanceTests.Drivers
{
    public class ApiDriver
    {
        private readonly RestClient _restClient;

        public ApiDriver(RestClient restClient)
        {
            _restClient = restClient;
        }

        public IRestResponse PostCsv(string resource, string csvData)
        {
            RestRequest request = new(resource)
            {
                Method = Method.POST,
            };
            request.AddParameter("text/csv", csvData, ParameterType.RequestBody);
            var response = _restClient.Execute(request);
            
            return response;
        }
    }
}