using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FCG.Api.Common;
using FCG.Api.Swagger;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace FCG.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiPresentation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory =
                        ApiValidationProblemDetailsFactory.CreateInvalidModelStateResponse;
                });

            AddJwtAuthentication(services, configuration);

            services.AddOpenApi();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "Informe o token JWT no formato: Bearer {token}",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    });

                options.OperationFilter<AuthorizeOperationFilter>();
            });

            return services;
        }

        private static void AddJwtAuthentication(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtOptions = JwtOptions.Create(configuration);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateLifetime = true,
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = JwtClaimNames.Role,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            await WriteProblemDetailsAsync(
                                context.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                ApiMessages.Unauthorized.Title,
                                ApiMessages.Unauthorized.Detail);
                        },
                        OnForbidden = async context =>
                        {
                            await WriteProblemDetailsAsync(
                                context.HttpContext,
                                StatusCodes.Status403Forbidden,
                                ApiMessages.Forbidden.Title,
                                ApiMessages.Forbidden.Detail);
                        }
                    };
                });

            services.AddAuthorization();
        }

        private static async Task WriteProblemDetailsAsync(
            HttpContext httpContext,
            int statusCode,
            string title,
            string detail)
        {
            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json");
        }

    }
}
