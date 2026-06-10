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
            // 1. AÐ ÝHLALLERÝ (Port taramalarý, IP eriþimleri)
            var activeAlerts = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/active")
                               ?? new List<AlertViewModel>();
            unifiedAlerts.AddRange(activeAlerts);

            // 2. ÝÇ AÐ & SCADA TEHDÝTLERÝ (Brute-Force ve Spoofing)
            var auditLogs = await _httpClient.GetFromJsonAsync<List<AuditLogDto>>("/api/Alert/audit-logs")
                            ?? new List<AuditLogDto>();

            foreach (var log in auditLogs)
            {
                // DÝKKAT: Filtreye "SÝBER_ÞÜPHE" ihtimalini de ekledik! Artýk SCADA verileri çöpe gitmeyecek.
                if (log.Status == "FAILED" || log.ActionType == "UPDATE_RECIPE" || log.ActionType == "SÝBER_ÞÜPHE")
                {
                    unifiedAlerts.Add(new AlertViewModel
                    {
                        AlertId = log.LogId,
                        Timestamp = log.Timestamp,
                        AlertType = log.ActionType,
                        Severity = "CRITICAL",
                        // SCADA verisiyse doðrudan detayý yaz, deðilse kullanýcý/IP bilgisi ekle
                        Message = log.ActionType == "SÝBER_ÞÜPHE"
                                  ? log.Details
                                  : $"[ÝÇ AÐ TEHDÝDÝ] {log.Details} (Kullanýcý: {log.UserName}, IP: {log.SourceIp})"
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