using Microsoft.Extensions.Options;
using NoviCode.Api.ExceptionHandling;
using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.EcbGateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<EcbGatewayOptions>(
    builder.Configuration.GetSection(EcbGatewayOptions.SectionName));

builder.Services.AddHttpClient<IEcbCurrenciesRate, EcbCurrenciesRateGateway>((sp, client) =>
{
    var gatewayOptions = sp.GetRequiredService<IOptions<EcbGatewayOptions>>().Value;
    var seconds = gatewayOptions.TimeoutSeconds > 0 ? gatewayOptions.TimeoutSeconds : EcbGatewayOptions.DefaultTimeoutSeconds;
    client.Timeout = TimeSpan.FromSeconds(seconds);
});

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
