using FCG.Api.Middlewares;
using FCG.Api.Options;

namespace FCG.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseApiPresentation(this WebApplication app)
        {
            var apiOptions = app.Configuration
                .GetSection(ApiOptions.SectionName)
                .Get<ApiOptions>() ?? new ApiOptions();

            app.UseMiddleware<StructuredRequestLoggingMiddleware>();

            if (app.Environment.IsDevelopment() && apiOptions.UseDeveloperExceptionPage)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
