using PromptPortal.Api.Endpoints;
using PromptPortal.Api.Middleware;
using PromptPortal.Application;
using PromptPortal.Infrastructure;
using PromptPortal.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<DevAuthMiddleware>();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.MapDefaultEndpoints();
app.MapPromptEndpoints();
app.MapSearchEndpoints();
app.MapMetadataEndpoints();

app.Run();

public partial class Program { }
