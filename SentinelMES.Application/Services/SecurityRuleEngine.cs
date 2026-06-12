using System;
using System.Threading.Tasks;
using SentinelMES.Application.Interfaces;
using SentinelMES.Domain.Entities;

namespace SentinelMES.Application.Services;

public class SecurityRuleEngine : ISecurityRuleEngine
{
    private readonly IAllowedAssetRepository _assetRepository;
    private readonly IAlertRepository _alertRepository;

    // Bağımlılıkları Constructor üzerinden gevşek bağlı (Loose Coupling) olarak alıyoruz
    public SecurityRuleEngine(IAllowedAssetRepository assetRepository, IAlertRepository alertRepository)
    {
        _assetRepository = assetRepository;
        _alertRepository = alertRepository;
    }

    public async Task<bool> ProcessNetworkTrafficAsync(string ip, string mac, string username, string actionType, string details)
    {
        //  1. KRİTİK KURAL: IP-MAC Eşleşme ve Spoofing Kontrolü (IP-MAC Binding)
        var asset = await _assetRepository.GetByIpAsync(ip);

        if (asset == null)
        {
            // Senaryo A: Ağda tamamen yabancı veya envanter dışı şüpheli bir IP adresi var!
            await GenerateSiberAlarmAsync("UNAUTHORIZED_IP_ACCESS", "CRITICAL",
                $"Ağ Güvenlik İhlali! Kayıtsız cihaz erişimi engellendi. IP: {ip}, MAC: {mac}");
            return false;
        }

        if (!string.Equals(asset.AllowedMac, mac, StringComparison.OrdinalIgnoreCase))
        {
            // Senaryo B: IP adresi sistemde kayıtlı ama fiziksel MAC adresi uyuşmuyor! 
            // Bu tam bir IP-MAC Spoofing (Ortadaki Adam / İkiz Cihaz) saldırısı kanıtıdır.
            await GenerateSiberAlarmAsync("IP_MAC_SPOOFING", "CRITICAL",
                $"Siber Saldırı Algılandı! {asset.DeviceName} jeneratörüne ait {ip} adresi, sahte bir donanım MAC adresi ({mac}) tarafından taklit ediliyor!");
            return false;
        }

        //  2. KURAL: Gelen istek simülatördeki kaba kuvvet (Brute-Force) girişiyse log tablosuna işle
        if (string.Equals(actionType, "LOGIN_ATTEMPT", StringComparison.OrdinalIgnoreCase) && details.Contains("Hatalı"))
        {
            var auditLog = new SystemAuditLog
            {
                Timestamp = DateTime.UtcNow,
                SourceIp = ip,
                UserName = username,
                ActionType = actionType,
                Status = "FAILED",
                Details = details,
                // Veritabanı artık bu iki alanı kesin bekliyor, dolduralım:
                CountryCode = "TR",
                CountryName = "Turkey"
            };
            await _alertRepository.AddAuditLogAsync(auditLog);
        }

        return true; // Paket tüm siber filtrelerden başarıyla geçti, temiz.
    }

    private async Task GenerateSiberAlarmAsync(string alertType, string severity, string message)
    {
        var alert = new ActiveAlert
        {
            Timestamp = DateTime.UtcNow,
            AlertType = alertType,
            Severity = severity,
            Message = message
        };

        await _alertRepository.AddAlertAsync(alert);
    }
}