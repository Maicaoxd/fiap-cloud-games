using FCG.Api.Common;

namespace FCG.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiPresentation(this IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory =
                        ApiValidationProblemDetailsFactory.CreateInvalidModelStateResponse;
                });

            services.AddOpenApi();
            services.AddSwaggerGen();

            return services;
        }
    }
}
