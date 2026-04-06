using Microsoft.EntityFrameworkCore;
using NoviCode.Api.ExceptionHandling;
using NoviCode.Api.Workers;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Application.Wallets;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.EcbGateway;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates;
using NoviCode.Infrastructure.ExchangeRates.Caching;
using NoviCode.Infrastructure.Repositories;
using NoviCode.Infrastructure.Wallets;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEcbGateway(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExchangeRateDb")));

builder.Services.AddScoped<IExchangeRatesBulkWriter, ExchangeRatesBulkWriter>();
builder.Services.AddHostedService<ExchangeRatesSyncWorker>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddSingleton<IWalletBalanceAdjustmentStrategyResolver, WalletBalanceAdjustmentStrategyResolver>();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IExchangeRatesCache, RedisExchangeRatesCache>();

builder.Services.AddScoped<ILatestEurExchangeRatesReader, CachedLatestEurExchangeRatesReader>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
