using FCG.Api.Extensions;
using FCG.Application;
using FCG.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPresentation();

app.Run();
