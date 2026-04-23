using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Application.Clients.Abstractions;
using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Common.Files;
using Kodvian.Core.Application.Dashboard.Abstractions;
using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Projects.Abstractions;
using Kodvian.Core.Application.Tasks.Abstractions;
using Kodvian.Core.Infrastructure.Auth;
using Kodvian.Core.Infrastructure.Persistence;
using Kodvian.Core.Infrastructure.Storage;
using Kodvian.Core.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kodvian.Core.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KodvianDbContext>(options =>
            options.UseNpgsql(BuildConnectionString(configuration)));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IProjectDeveloperContractService, ProjectDeveloperContractService>();
        services.AddScoped<IDeveloperPaymentService, DeveloperPaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFinancialCategoryService, FinancialCategoryService>();
        services.AddScoped<IFinancialMovementService, FinancialMovementService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var railwayDatabaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(railwayDatabaseUrl))
        {
            var uri = new Uri(railwayDatabaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                MaxPoolSize = 20,
                Timeout = 15,
                CommandTimeout = 30
            };

            return builder.ToString();
        }

        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string was not configured.");
    }
}
