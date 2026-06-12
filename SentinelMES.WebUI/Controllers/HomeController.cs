using Microsoft.AspNetCore.Mvc;
using SentinelMES.WebUI.Models;
using System.Net.Http.Json;

namespace SentinelMES.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    public async Task<IActionResult> Index()
    {
        List<AlertViewModel> unifiedAlerts = new();

        try
        {
            // 1. AÐ ÝHLALLERÝ (Eðer Alert tablosunda aktif bir þey varsa)
            var activeAlerts = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/active")
                               ?? new List<AlertViewModel>();
            unifiedAlerts.AddRange(activeAlerts);

            // 2. ÝÇ AÐ & SCADA TEHDÝTLERÝ (Bizim simülatörden basýlan loglar)
            var auditLogs = await _httpClient.GetFromJsonAsync<List<AuditLogDto>>("/api/Alert/audit-logs")
                            ?? new List<AuditLogDto>();

            foreach (var log in auditLogs)
            {
                // DÝKKAT: Filtreyi tamamen yeni Global Ýngilizce standartlarýmýza göre açtýk!
                if (log.ActionType == "DDOS_ATTACK" ||
                    log.ActionType == "PORT_SCAN" ||
                    log.ActionType == "INSIDER_THREAT" ||
                    log.ActionType == "LOGIN_SUCCESS")
                {
                    unifiedAlerts.Add(new AlertViewModel
                    {
                        AlertId = log.LogId,
                        Timestamp = log.Timestamp,
                        AlertType = log.ActionType,
                        Severity = log.Status == "SUCCESS" ? "INFO" : (log.Status ?? "CRITICAL"),

                        // Zaten CsvStreamingService'de mesajý (IP dahil) harika þekilde hazýrladýðýmýz için, 
                        // buraya ekstra bir þey eklemeden doðrudan veritabanýndaki o güzel metni alýyoruz:
                        Message = log.Details
                    });
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"Siber Güvenlik API'sine baðlanýlamadý! Hata: {ex.Message}";
        }

        // Bütün alarmlarý zamana göre diz (En yeni en üstte)
        unifiedAlerts = unifiedAlerts.OrderByDescending(a => a.Timestamp).ToList();

        return View(unifiedAlerts);
    }
}