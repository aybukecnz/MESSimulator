using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using SentinelMES.Simulator.Models;
using SentinelMES.Simulator.Data;
using Microsoft.Extensions.Logging;
using Bogus;

namespace SentinelMES.Simulator.Services;

public class CsvStreamingService
{
    private readonly TelemetryRepository _repository;
    private readonly ILogger<CsvStreamingService> _logger;
    private readonly ThreatDatasetReader _threatReader; // Gerçek dataset motorumuz eklendi

    public CsvStreamingService(TelemetryRepository repository, ILogger<CsvStreamingService> logger, ThreatDatasetReader threatReader)
    {
        _repository = repository;
        _logger = logger;
        _threatReader = threatReader;
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

        // 🚨 ÇÖZÜM 1: Motoru Ateşliyoruz! CSV dosyalarını belleğe alıyoruz.
        // Projenin ana dizinindeki Scripts klasörüne yolları gösteriyoruz.
        string ddosPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Friday-WorkingHours-Afternoon-DDos.pcap_ISCX.csv");
        string portScanPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Friday-WorkingHours-Afternoon-PortScan.pcap_ISCX.csv");

        _threatReader.LoadDatasets(ddosPath, portScanPath);

        _logger.LogInformation("Kaggle SCADA dosyasından canlı fiziksel veri akışı başlatılıyor...");

        var faker = new Faker("tr");
        var random = new Random();

        while (await csv.ReadAsync() && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. FİZİKSEL AKIŞ
                var telemetry = csv.GetRecord<MachineTelemetry>();
                telemetry.Timestamp = DateTime.UtcNow;
                await _repository.InsertTelemetryAsync(telemetry);

                _logger.LogInformation("SCADA Verisi: Rüzgar {WindSpeed} m/s | Güç {Power} kW",
                                        telemetry.WindSpeed, telemetry.ActivePower);

                // 2. SİBER AKIŞ
                int dice = random.Next(1, 101);

                if (dice <= 2)
                {
                    // %2 İHTİMAL: İç Tehdit (TR)
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = "192.168.1.15",
                        UserName = faker.Internet.UserName(),
                        ActionType = "INSIDER_THREAT", // İNGİLİZCE VEKTÖR
                        Status = "CRITICAL", // İNGİLİZCE SEVİYE
                        Details = "KRİTİK: Laminasyon sıcaklık reçetesi izinsiz olarak 150C'den 250C'ye yükseltildi!", // TÜRKÇE RAPOR
                        CountryCode = "TR",
                        CountryName = "Turkey"
                    };
                    await _repository.InsertAuditLogAsync(log);
                    _logger.LogWarning("SİBER TEHDİT: Reçete Manipülasyonu (İç Tehdit)!");
                }
                else if (dice <= 6)
                {
                    // %4 İHTİMAL: DDOS Saldırısı (RU)
                    var payload = _threatReader.GetRandomAttack("DDOS");
                    if (payload != null)
                    {
                        string ddosIp = "195.14.161." + random.Next(1, 255);
                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow,
                            SourceIP = ddosIp,
                            UserName = "UNKNOWN",
                            ActionType = "DDOS_ATTACK", // İNGİLİZCE VEKTÖR
                            Status = "FAILED", // İNGİLİZCE SEVİYE
                            Details = $"[AĞ İHLALİ] Yüksek hacimli anormal trafik paketi (DDoS) tespit edildi. İstek düşürüldü. (Hedef IP: {ddosIp})", // TÜRKÇE RAPOR
                            CountryCode = "RU",
                            CountryName = "Russia"
                        };
                        await _repository.InsertAuditLogAsync(log);
                        _logger.LogCritical("🔥 SİBER SALDIRI: Rusya kaynaklı DDoS engellendi!");
                    }
                }
                else if (dice > 6 && dice <= 10)
                {
                    // %4 İHTİMAL: PortScan Keşfi (CN)
                    var payload = _threatReader.GetRandomAttack("PORTSCAN");
                    if (payload != null)
                    {
                        string scanIp = "114.114.114." + random.Next(1, 255);
                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow,
                            SourceIP = scanIp,
                            UserName = "UNKNOWN",
                            ActionType = "PORT_SCAN", // İNGİLİZCE VEKTÖR
                            Status = "FAILED", // İNGİLİZCE SEVİYE
                            Details = $"[SİBER KEŞİF] Ağ üzerinde yetkisiz port taraması yapılıyor. (Kaynak IP: {scanIp})", // TÜRKÇE RAPOR
                            CountryCode = "CN",
                            CountryName = "China"
                        };
                        await _repository.InsertAuditLogAsync(log);
                        _logger.LogWarning("⚠️ TEHDİT: Çin kaynaklı PortScan keşfi durduruldu.");
                    }
                }
                else if (dice <= 16)
                {
                    // %6 İHTİMAL: Normal Operatör (TR)
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = $"192.168.1.{random.Next(50, 100)}",
                        UserName = faker.Name.FirstName() + "_Operatör",
                        ActionType = "LOGIN_SUCCESS",
                        Status = "SUCCESS",
                        Details = "Vardiya başlangıcı rutin operatör girişi.", // TÜRKÇE RAPOR
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