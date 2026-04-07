using NoviCode.Api.DependencyInjection;
using NoviCode.Application.DependencyInjection;
using NoviCode.EcbGateway;
using NoviCode.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddRateLimitingPolicies();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEcbGateway(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapControllers();

app.Run();
