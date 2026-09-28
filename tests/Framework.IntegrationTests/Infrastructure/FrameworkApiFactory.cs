using Framework.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Framework.IntegrationTests.Infrastructure;

public sealed class FrameworkApiFactory : WebApplicationFactory<Framework.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string databaseName = $"framework-tests-{Guid.NewGuid():N}";

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<FrameworkDbContext>>();
            services.RemoveAll<DbContextOptions<FrameworkDbContext>>();
            services.RemoveAll<FrameworkDbContext>();

            services.AddDbContext<FrameworkDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            FrameworkDbContext dbContext = scope.ServiceProvider.GetRequiredService<FrameworkDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
    }
}

