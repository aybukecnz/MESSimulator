using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using SentinelMES.Simulator.Models;
using SentinelMES.Simulator.Data;
using Microsoft.Extensions.Logging;
using Bogus;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        _logger.LogInformation("Kaggle SCADA dosyasından canlı fiziksel veri akışı başlatılıyor...");

        var faker = new Faker("tr");
        var random = new Random();

        //  SİBER İSTİHBARAT: Dışarıdan gelecek saldırılar için yasaklı ülke havuzu
        var threatCountries = new[] {
            ("RU", "Russia"),
            ("CN", "China"),
            ("IR", "Iran"),
            ("KP", "North Korea")
        };

        while (await csv.ReadAsync() && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. FİZİKSEL AKIŞ (Makineler Çalışıyor)
                var telemetry = csv.GetRecord<MachineTelemetry>();
                telemetry.Timestamp = DateTime.UtcNow;
                await _repository.InsertTelemetryAsync(telemetry);

                _logger.LogInformation("SCADA Verisi: Rüzgar {WindSpeed} m/s | Güç {Power} kW",
                                        telemetry.WindSpeed, telemetry.ActivePower);

                // 2. SİBER AKIŞ (Threat Injection & Data Enrichment)
                int dice = random.Next(1, 101);

                if (dice <= 2)
                {
                    // %2 İHTİMAL: İç Tehdit (Lateral Movement) - Yerel Ağdan Geldiği İçin Türkiye (TR)
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = "192.168.1.15",
                        UserName = faker.Internet.UserName(),
                        ActionType = "UPDATE_RECIPE",
                        Status = "SUCCESS",
                        Details = "KRİTİK: Laminasyon sıcaklık reçetesi izinsiz olarak 150C'den 250C'ye yükseltildi!",
                        CountryCode = "TR",         
                        CountryName = "Turkey"      
                    };
                    await _repository.InsertAuditLogAsync(log);
                    _logger.LogWarning("SİBER TEHDİT: Reçete Manipülasyonu (İç Tehdit)!");
                }
                else if (dice <= 6)
                {
                    // %4 İHTİMAL: Kaba Kuvvet (Brute Force) Saldırısı - Dışarıdan Geliyor
                    string attackerIp = faker.Internet.Ip();
                    var attackerGeo = faker.PickRandom(threatCountries); // Rastgele yasaklı ülke seç

                    for (int i = 0; i < 15; i++)
                    {
                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow.AddMilliseconds(i * 50),
                            SourceIP = attackerIp,
                            UserName = "Admin",
                            ActionType = "LOGIN_ATTEMPT",
                            Status = "FAILED",
                            Details = "Hatalı şifre denemesi.",
                            CountryCode = attackerGeo.Item1,  // "RU"
                            CountryName = attackerGeo.Item2   // "Russia"
                        };
                        await _repository.InsertAuditLogAsync(log);
                    }
                    _logger.LogCritical("SİBER TEHDİT: {Ip} ({Country}) adresinden Brute-Force saldırısı!", attackerIp, attackerGeo.Item1);
                }
                else if (dice > 6 && dice <= 10)
                {
                    // %4 İHTİMAL: SCADA Güç Değeri Manipülasyonu (Spoofing)
                    var attackerGeo = faker.PickRandom(threatCountries);

                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = faker.Internet.Ip(),
                        UserName = "Bilinmeyen_SCADA_Cihazı",
                        ActionType = "SİBER_ŞÜPHE",
                        Status = "CRITICAL",
                        Details = $"Anormal güç verisi (Spoofing) tespit edildi! Orijinal: {telemetry.ActivePower} kW",
                        CountryCode = attackerGeo.Item1,
                        CountryName = attackerGeo.Item2
                    };
                    await _repository.InsertAuditLogAsync(log);
                    _logger.LogWarning("SİBER TEHDİT: {Country} kaynaklı SCADA Güç Spoofing'i yakalandı!", attackerGeo.Item2);
                }
                else if (dice <= 16)
                {
                    // %10 İHTİMAL: Normal Operatör Girişi (Masum) - Şirket içinden olduğu için TR
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = $"192.168.1.{random.Next(50, 100)}",
                        UserName = faker.Name.FirstName() + "_Operatör",
                        ActionType = "LOGIN_SUCCESS",
                        Status = "SUCCESS",
                        Details = "Vardiya başlangıcı rutin giriş.",
                        CountryCode = "TR",
                        CountryName = "Turkey"
                    };
                    await _repository.InsertAuditLogAsync(log);
                }

                await Task.Delay(1000, stoppingToken); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satır okunurken veya tehdit üretilirken hata oluştu.");
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