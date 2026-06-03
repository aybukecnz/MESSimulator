using SentinelMES.Simulator;
using SentinelMES.Simulator.Data;
using SentinelMES.Simulator.Services;

var builder = Host.CreateApplicationBuilder(args);

// Repository'mizi sisteme Singleton (tekil) olarak ekliyoruz
builder.Services.AddSingleton<TelemetryRepository>();
builder.Services.AddSingleton<CsvStreamingService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();