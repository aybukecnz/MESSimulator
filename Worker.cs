using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SentinelMES.Simulator.Services;

namespace SentinelMES.Simulator;

public class Worker : BackgroundService
{
    private readonly CsvStreamingService _streamingService;
    private readonly ILogger<Worker> _logger;

    public Worker(CsvStreamingService streamingService, ILogger<Worker> logger)
    {
        _streamingService = streamingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sentinel-MES Simülatörü ayaða kalktý: {time}", DateTimeOffset.Now);

        // CSV dosyasýnýn dinamik yolunu buluyoruz
        string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wind_turbine_scada.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogError("HATA: CSV dosyasý bulunamadý! Lütfen {path} konumuna dosyayý ekleyin.", csvPath);
            return;
        }

        // Simülasyon motorunu çalýþtýr
        await _streamingService.StreamDataAsync(csvPath, stoppingToken);
    }
}