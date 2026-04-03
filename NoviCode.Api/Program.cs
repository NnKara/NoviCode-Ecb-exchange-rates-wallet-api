using Microsoft.EntityFrameworkCore;
using NoviCode.Api.ExceptionHandling;
using NoviCode.Api.Workers;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.EcbGateway;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates;

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
