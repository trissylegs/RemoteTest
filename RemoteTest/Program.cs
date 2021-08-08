using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RemoteTest.Extensions;

namespace RemoteTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build()
                .CreateDbIfNoneExists()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            CreateHostBuilder(args, null, null);
        
        public static IHostBuilder CreateHostBuilder(string[] args, string? url, string? root) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    if (url != null)
                    {
                        webBuilder.UseUrls(url);
                    }

                    if (root != null)
                    {
                        webBuilder.UseWebRoot(root);
                    }
                });
    }
}