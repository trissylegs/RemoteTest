using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RemoteTest.Extensions;

namespace RemoteTest.AcceptanceTests.Drivers
{
    public class WebServerDriver: IDisposable
    {
        private IHost _host;

        public WebServerDriver(string url, string connectionString)
        {
            Url = url;
            ConnectionString = connectionString;
        }

        public string Url { get; }
        public string ConnectionString { get; }

        public void Start()
        {
            IHostBuilder hostBuilder = Program.CreateHostBuilder(
                new string[]{},
                Url,
                $"/home/nirya/src/RiderProjects/RemoteTest/RemoteTest");
            _host = hostBuilder.Build();
            _host.CreateDbIfNoneExists();
            _host.StartAsync().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _host.StopAsync(TimeSpan.FromSeconds(1));
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}