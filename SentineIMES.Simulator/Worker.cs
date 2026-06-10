using Bogus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SentinelMES.Simulator.Services;
using System;
using System.IO;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SentinelMES.Simulator;

public class Worker : BackgroundService
{
    private readonly CsvStreamingService _streamingService;
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;

    // Faker nesnesini sınıf seviyesinde tanımlıyoruz ki her metot ulaşabilsin
    private readonly Faker _faker;

    public Worker(CsvStreamingService streamingService, ILogger<Worker> logger)
    {
        _streamingService = streamingService;
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        _faker = new Faker(); // Faker'ı Constructor içinde başlatıyoruz
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sentinel-MES Güvenlik Simülatörü ve Saldırgan modülü başlatıldı...");

        // 1. CSV'den veri akışını başlatan görev (Asenkron)
        string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wind_turbine_scada.csv");

        // 2. Saldırgan Döngüsü: Her 5 saniyede bir sahte trafik üret
        _ = Task.Run(async () => {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SimulateCyberAttack();
                await Task.Delay(5000, stoppingToken);
            }
        }, stoppingToken);

        await _streamingService.StreamDataAsync(csvPath, stoppingToken);
    }

    private async Task SimulateCyberAttack()
    {
        // DİKKAT: Her döngüde YEPYENİ bir IP ve MAC adresi üretmek için 
        // zar atma işlemini tam olarak bu metodun içine (döngünün kalbine) koyuyoruz!
        string randomIp = _faker.Internet.Ip();
        string randomMac = _faker.Internet.Mac();

        var maliciousPacket = new
        {
            IpAddress = randomIp,
            MacAddress = randomMac,
            Username = "hacker_bot",
            ActionType = "PORT_SCAN", // Ekranında Ağ İhlalleri kategorisinde şık durması için
            Details = "Sisteme yetkisiz bağlantı ve port tarama girişimi."
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/NetworkTraffic/analyze", maliciousPacket);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Yakalanan IP adresini siyah ekranda da gösterelim
                _logger.LogWarning("[!] SALDIRI YAKALANDI: API, {Ip} adresinden gelen sahte trafiği engelledi!", randomIp);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("API'ye ulaşılamadı. Lütfen WebAPI projesinin çalıştığından emin olun. Hata: {msg}", ex.Message);
        }
    }
}