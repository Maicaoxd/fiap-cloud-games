using FCG.Application.Games.Create;
using FCG.Application.Users.Authenticate;
using FCG.Application.Users.Register;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<AuthenticateUserUseCase>();
            services.AddScoped<CreateGameUseCase>();
            services.AddScoped<RegisterUserUseCase>();

            return services;
        }
    }
}
