using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using SentinelMES.Simulator.Models;
using SentinelMES.Simulator.Data;
using Microsoft.Extensions.Logging;
using Bogus; // Siber logları üretmek için ekledik

namespace SentinelMES.Simulator.Services;

public class CsvStreamingService
{
    private readonly TelemetryRepository _repository;
    private readonly ILogger<CsvStreamingService> _logger;

    public CsvStreamingService(TelemetryRepository repository, ILogger<CsvStreamingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task StreamDataAsync(string filePath, CancellationToken stoppingToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TelemetryMap>();

        _logger.LogInformation("Kaggle CSV dosyasından canlı veri akışı başlatılıyor...");

        // Sahte veri üretici motorumuz (Türkçe isimler için 'tr' lokasyonu)
        var faker = new Faker("tr");
        var random = new Random();

        while (await csv.ReadAsync() && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. FİZİKSEL AKIŞ (Masum Kısım - Makineler Çalışıyor)
                var telemetry = csv.GetRecord<MachineTelemetry>();
                telemetry.Timestamp = DateTime.UtcNow; // Zamanı şu an yap
                await _repository.InsertTelemetryAsync(telemetry);

                _logger.LogInformation("SCADA Verisi: Rüzgar {WindSpeed} m/s | Güç {Power} kW",
                                        telemetry.WindSpeed, telemetry.ActivePower);

                // -------------------------------------------------------------
                // 2. SİBER AKIŞ (Threat Injection / Zar Atma Mekanizması)
                // -------------------------------------------------------------
                int dice = random.Next(1, 101); // 1 ile 100 arasında bir sayı tut

                if (dice <= 2)
                {
                    // %2 İHTİMAL: İç Tehdit (Reçete Manipülasyonu)
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = "192.168.1.15", // Fabrika içi (Lateral) bir IP
                        UserName = faker.Internet.UserName(),
                        ActionType = "UPDATE_RECIPE",
                        Status = "SUCCESS",
                        Details = "KRİTİK: Laminasyon sıcaklık reçetesi izinsiz olarak 150C'den 250C'ye yükseltildi!"
                    };
                    await _repository.InsertAuditLogAsync(log);
                    _logger.LogWarning("SİBER TEHDİT: Reçete Manipülasyonu tespit edildi (İç Tehdit)!");
                }
                else if (dice <= 6)
                {
                    // %4 İHTİMAL: Kaba Kuvvet (Brute Force) Saldırısı
                    string attackerIp = faker.Internet.Ip(); // Dışarıdan rastgele bir IP

                    // Saniyeler içinde 15 defa hatalı şifre girildiğini simüle ediyoruz
                    for (int i = 0; i < 15; i++)
                    {
                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow.AddMilliseconds(i * 50),
                            SourceIP = attackerIp,
                            UserName = "Admin",
                            ActionType = "LOGIN_ATTEMPT",
                            Status = "FAILED",
                            Details = "Hatalı şifre denemesi."
                        };
                        await _repository.InsertAuditLogAsync(log);
                    }
                    _logger.LogCritical("SİBER TEHDİT: {Ip} adresinden Admin hesabına Brute-Force saldırısı!", attackerIp);
                }
                else if (dice <= 16)
                {
                    // %10 İHTİMAL: Normal Operatör Girişi (Masum Hareket)
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = $"192.168.1.{random.Next(50, 100)}", // Operatörlerin IP bloğu
                        UserName = faker.Name.FirstName() + "_Operatör",
                        ActionType = "LOGIN_SUCCESS",
                        Status = "SUCCESS",
                        Details = "Vardiya başlangıcı rutin giriş."
                    };
                    await _repository.InsertAuditLogAsync(log);
                }
                // Geriye kalan %84'lük kısımda (dice > 16) siber log üretilmez, sistem sakindir.

                // Döngü saniyede bir dönsün diye bekletiyoruz
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satır okunurken hata oluştu.");
            }
        }
    }
}

public sealed class TelemetryMap : ClassMap<MachineTelemetry>
{
    public TelemetryMap()
    {
        Map(m => m.ActivePower).Name("LV ActivePower (kW)");
        Map(m => m.WindSpeed).Name("Wind Speed (m/s)");
        Map(m => m.TheoreticalPower).Name("Theoretical_Power_Curve (KWh)");
        Map(m => m.WindDirection).Name("Wind Direction (°)");
    }
}