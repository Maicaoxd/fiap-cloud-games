using FCG.Application.Games.Create;
using FCG.Application.Games.Deactivate;
using FCG.Application.Games.Get;
using FCG.Application.Games.List;
using FCG.Application.Games.Update;
using FCG.Application.Libraries.Acquire;
using FCG.Application.Libraries.List;
using FCG.Application.Users.Authenticate;
using FCG.Application.Users.ChangePassword;
using FCG.Application.Users.Deactivate;
using FCG.Application.Users.ForgotPassword;
using FCG.Application.Users.List;
using FCG.Application.Users.Register;
using FCG.Application.Users.Update;
using FCG.Application.Users.UpdateCurrent;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            AddGameUseCases(services);
            AddLibraryUseCases(services);
            AddUserUseCases(services);

            return services;
        }

        private static void AddGameUseCases(IServiceCollection services)
        {
            services.AddScoped<CreateGameUseCase>();
            services.AddScoped<DeactivateGameUseCase>();
            services.AddScoped<GetGameUseCase>();
            services.AddScoped<ListGamesUseCase>();
            services.AddScoped<UpdateGameUseCase>();
        }

        private static void AddLibraryUseCases(IServiceCollection services)
        {
            services.AddScoped<AcquireGameUseCase>();
            services.AddScoped<ListLibraryGamesUseCase>();
        }

        private static void AddUserUseCases(IServiceCollection services)
        {
            services.AddScoped<AuthenticateUserUseCase>();
            services.AddScoped<ChangePasswordUseCase>();
            services.AddScoped<DeactivateUserUseCase>();
            services.AddScoped<ForgotPasswordUseCase>();
            services.AddScoped<ListUsersUseCase>();
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<UpdateCurrentUserUseCase>();
        }
    }
}
