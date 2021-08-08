using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RemoteTest.Models;

namespace RemoteTest.Extensions
{
    public static class HostExtensions
    {
        public static IHost CreateDbIfNoneExists(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DatabaseContext>();
                context.Database.EnsureCreated();
                var initializer = services.GetRequiredService<DbInitializer>();
                initializer.InitializeWithFile("Test_Accounts.csv");
            }
            catch (Exception ex)
            {
                services.GetRequiredService<ILogger<Program>>()
                    .LogError(ex, "An error occurred creating the database");
            }

            return host;
        }
    }
}