using FCG.Application.Games.Create;
using FCG.Application.Games.Get;
using FCG.Application.Games.List;
using FCG.Application.Games.Update;
using FCG.Application.Libraries.Acquire;
using FCG.Application.Libraries.List;
using FCG.Application.Users.Authenticate;
using FCG.Application.Users.Register;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<AcquireGameUseCase>();
            services.AddScoped<AuthenticateUserUseCase>();
            services.AddScoped<CreateGameUseCase>();
            services.AddScoped<GetGameUseCase>();
            services.AddScoped<ListLibraryGamesUseCase>();
            services.AddScoped<ListGamesUseCase>();
            services.AddScoped<UpdateGameUseCase>();
            services.AddScoped<RegisterUserUseCase>();

            return services;
        }
    }
}
