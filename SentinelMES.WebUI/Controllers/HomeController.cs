using Microsoft.AspNetCore.Mvc;
using SentinelMES.WebUI.Models;
using System.Net.Http.Json;

namespace SentinelMES.WebUI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };
    }

    // STRATEJİK KOMUTA MERKEZİ (DASHBOARD)
    [Route("")]
    [Route("Home/Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        List<AlertViewModel> unifiedAlerts = new();

        try
        {
            // API zaten AlertViewModel dönüyor, direkt alıyoruz.
            var activeAlerts = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/active")
                               ?? new List<AlertViewModel>();

            unifiedAlerts.AddRange(activeAlerts);

            // Audit logları da direkt AlertViewModel olarak alıyoruz
            var auditLogs = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/audit-logs")
                            ?? new List<AlertViewModel>();

            foreach (var log in auditLogs)
            {
                // API tarafında ActionType zaten AlertType olarak maplenmişti!
                if (log.AlertType == "DDOS_ATTACK" ||
                    log.AlertType == "PORT_SCAN" ||
                    log.AlertType == "INSIDER_THREAT" ||
                    log.AlertType == "LOGIN_SUCCESS" ||
                    log.AlertType == "AI_ANOMALY")
                {
                    // Veriyi ikinci kez dönüştürmeye gerek yok, doğrudan ekle
                    unifiedAlerts.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"API Bağlantı Hatası: {ex.Message}";
        }

        return View(unifiedAlerts.OrderByDescending(a => a.Timestamp).ToList());
    }

    // ADLİ BİLİŞİM & CENTRAL LOG DEPOSU (AUDIT LOGS)
    public async Task<IActionResult> AuditLogs()
    {
        List<AlertViewModel> archivedAlerts = new();

        try
        {
            // Yine doğrudan AlertViewModel olarak alıyoruz
            var auditLogs = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/audit-logs")
                            ?? new List<AlertViewModel>();

            foreach (var log in auditLogs)
            {
                if (log.AlertType == "DDOS_ATTACK" ||
                    log.AlertType == "PORT_SCAN" ||
                    log.AlertType == "INSIDER_THREAT" ||
                    log.AlertType == "LOGIN_SUCCESS" ||
                    log.AlertType == "AI_ANOMALY")
                {
                    archivedAlerts.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"Log Arşivine bağlanılamadı: {ex.Message}";
        }

        return View(archivedAlerts.OrderByDescending(a => a.Timestamp).ToList());
    }
}