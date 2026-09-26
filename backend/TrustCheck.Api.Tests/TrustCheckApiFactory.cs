using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustCheck.Api.Repositories;

namespace TrustCheck.Api.Tests;

public class TrustCheckApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDb:TableName"] = "Verifications"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IVerificationRepository));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IVerificationRepository, InMemoryVerificationRepository>();
        });
    }
}
