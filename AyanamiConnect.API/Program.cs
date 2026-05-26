using AyanamiConnect.API.Common;
using AyanamiConnect.API.Endpoints;
using AyanamiConnect.API.ServiceCollections;
using AyanamiConnect.Application.DI;
using AyanamiConnect.Infrastructure.DI;
using AyanamiConnect.Persistence.DI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(TelegramMiniAppAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TelegramMiniAppAuthenticationHandler>(
        TelegramMiniAppAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://app.ayanami-connect.online", "https://ayanami-connect.online")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddCustomRateLimiter();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(TelegramMiniAppAuthenticationHandler.SchemeName)
        .RequireAuthenticatedUser()
        .Build();
    //options.FallbackPolicy = options.DefaultPolicy;
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapUsersEndpoint();
app.MapSubscriptionsEndpoint();
app.MapAdminEndpoint();

app.Run();