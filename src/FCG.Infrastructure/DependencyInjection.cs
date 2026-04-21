using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Persistence.Repositories;
using FCG.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string was not configured.");

            services.AddDbContext<FcgDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddSingleton(CreateJwtOptions(configuration));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();

            return services;
        }

        private static JwtOptions CreateJwtOptions(IConfiguration configuration)
        {
            var section = configuration.GetSection(JwtOptions.SectionName);

            if (!int.TryParse(section["ExpirationMinutes"], out var expirationMinutes))
                throw new InvalidOperationException("Jwt expiration minutes was not configured.");

            return new JwtOptions(
                section["Issuer"] ?? string.Empty,
                section["Audience"] ?? string.Empty,
                section["Secret"] ?? string.Empty,
                expirationMinutes);
        }
    }
}
