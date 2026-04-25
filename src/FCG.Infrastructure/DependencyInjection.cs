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
            AddPersistence(services, configuration);
            AddSecurity(services, configuration);

            return services;
        }

        private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string was not configured.");

            services.AddDbContext<FcgDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        private static void AddSecurity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(JwtOptions.Create(configuration));
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        }
    }
}
