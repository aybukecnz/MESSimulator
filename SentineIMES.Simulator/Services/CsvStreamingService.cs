using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
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
    private readonly ThreatDatasetReader _threatReader;
    private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };

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
                // ==========================================
                // 1. FİZİKSEL AKIŞ
                // ==========================================
                var telemetry = csv.GetRecord<MachineTelemetry>();
                telemetry.Timestamp = DateTime.UtcNow;

                if (random.Next(1, 11) == 1)
                {
                    telemetry.WindSpeed = 1.2m;
                    telemetry.ActivePower = 4500.0m;
                    _logger.LogWarning("⚠️ DİKKAT: Sisteme sentetik Spoofing saldırısı enjekte edildi, AI bekleniyor...");
                }

                await _repository.InsertTelemetryAsync(telemetry);

                _logger.LogInformation("SCADA Verisi: Rüzgar {WindSpeed} m/s | Güç {Power} kW", telemetry.WindSpeed, telemetry.ActivePower);

                // ==========================================
                // 2. YAPAY ZEKA (XAI) ANALİZİ
                // ==========================================
                var aiRequest = new ScadaTelemetryRequest
                {
                    wind_speed = (double)telemetry.WindSpeed,
                    active_power = (double)telemetry.ActivePower,
                    theoretical_power = (double)telemetry.TheoreticalPower,
                    wind_direction = (double)telemetry.WindDirection
                };

                AiAnalysisResponse aiResponse = null;
                try
                {
                    var httpResponse = await _httpClient.PostAsJsonAsync("/analyze", aiRequest, stoppingToken);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        aiResponse = await httpResponse.Content.ReadFromJsonAsync<AiAnalysisResponse>(cancellationToken: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Yapay Zeka Motoruna bağlanılamadı! : {ex.Message}");
                }

                if (aiResponse != null && aiResponse.is_anomaly)
                {
                    var aptCountries = new[] {
                        new { Code = "RU", Name = "Russia", IP = $"195.14.161.{random.Next(1, 255)}" },
                        new { Code = "CN", Name = "China", IP = $"114.114.114.{random.Next(1, 255)}" },
                        new { Code = "IR", Name = "Iran", IP = $"5.200.253.{random.Next(1, 255)}" },
                        new { Code = "KP", Name = "North Korea", IP = $"175.45.176.{random.Next(1, 255)}" }
                    };

                    var threatOrigin = aptCountries[random.Next(aptCountries.Length)];

                    var aiLog = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = threatOrigin.IP,
                        UserName = "APT_HACKER",
                        ActionType = "AI_ANOMALY",
                        Status = "CRITICAL",
                        Details = aiResponse.xai_explanation,
                        CountryCode = threatOrigin.Code,
                        CountryName = threatOrigin.Name
                    };

                    await _repository.InsertAuditLogAsync(aiLog);
                    _logger.LogCritical($"YAPAY ZEKA ALARMI ({threatOrigin.Code}): {aiResponse.xai_explanation}");
                }

                // ==========================================
                // 3. SİBER AKIŞ
                // ==========================================
                int dice = random.Next(1, 101);

                if (dice <= 2)
                {
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = "192.168.1.15",
                        UserName = faker.Internet.UserName(),
                        ActionType = "INSIDER_THREAT",
                        Status = "CRITICAL",
                        Details = "KRİTİK: Laminasyon sıcaklık reçetesi izinsiz olarak 150C'den 250C'ye yükseltildi!",
                        CountryCode = "TR",
                        CountryName = "Turkey"
                    };
                    await _repository.InsertAuditLogAsync(log);
                }
                else if (dice <= 6)
                {
                    var payload = _threatReader.GetRandomAttack("DDOS");
                    if (payload != null)
                    {
                        string ddosIp = "195.14.161." + random.Next(1, 255);
                        string fakeMac = faker.Internet.Mac();

                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow,
                            SourceIP = ddosIp,
                            UserName = "UNKNOWN",
                            ActionType = "DDOS_ATTACK",
                            Status = "FAILED",
                            Details = $"[AĞ İHLALİ] Yüksek hacimli anormal trafik paketi (DDoS) tespit edildi. İstek düşürüldü. (Hedef IP: {ddosIp} | Kaynak MAC: {fakeMac})",
                            CountryCode = "RU",
                            CountryName = "Russia"
                        };
                        await _repository.InsertAuditLogAsync(log);
                    }
                }
                else if (dice > 6 && dice <= 10)
                {
                    var payload = _threatReader.GetRandomAttack("PORTSCAN");
                    if (payload != null)
                    {
                        string scanIp = "114.114.114." + random.Next(1, 255);
                        string fakeMac = faker.Internet.Mac();

                        var log = new SystemAuditLog
                        {
                            Timestamp = DateTime.UtcNow,
                            SourceIP = scanIp,
                            UserName = "UNKNOWN",
                            ActionType = "PORT_SCAN",
                            Status = "FAILED",
                            Details = $"[SİBER KEŞİF] Ağ üzerinde yetkisiz port taraması yapılıyor. (Kaynak IP: {scanIp} | Kaynak MAC: {fakeMac})",
                            CountryCode = "CN",
                            CountryName = "China"
                        };
                        await _repository.InsertAuditLogAsync(log);
                    }
                }
                else if (dice <= 16)
                {
                    var log = new SystemAuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        SourceIP = $"192.168.1.{random.Next(50, 100)}",
                        UserName = faker.Name.FirstName() + "_Operatör",
                        ActionType = "LOGIN_SUCCESS",
                        Status = "SUCCESS",
                        Details = "Vardiya başlangıcı rutin operatör girişi.",
                        CountryCode = "TR",
                        CountryName = "Turkey"
                    };
                    await _repository.InsertAuditLogAsync(log);
                }

                await Task.Delay(2000, stoppingToken);
            } // END OF TRY BLOCK
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satır okunurken veya tehdit üretilirken hata oluştu.");
            } // END OF CATCH BLOCK
        } // END OF WHILE LOOP
    } // END OF METHOD
} // END OF CLASS