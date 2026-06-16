using Microsoft.AspNetCore.Mvc;
using SentinelMES.WebUI.Models;
using System.Net.Http.Json;

namespace SentinelMES.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController()
    {
        // WebAPI projesini dinleyen HttpClient temel yapılandırması
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    //  STRATEJİK KOMUTA MERKEZİ (DASHBOARD)
    [Route("")] // Tarayıcıya sadece localhost:8500 yazınca burası tetiklenecek.
    [Route("Home/Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        List<AlertViewModel> unifiedAlerts = new();

        try
        {
            var activeAlerts = await _httpClient.GetFromJsonAsync<List<AlertViewModel>>("/api/Alert/active")
                               ?? new List<AlertViewModel>();
            unifiedAlerts.AddRange(activeAlerts);

            var auditLogs = await _httpClient.GetFromJsonAsync<List<AuditLogDto>>("/api/Alert/audit-logs")
                            ?? new List<AuditLogDto>();

            foreach (var log in auditLogs)
            {
                if (log.ActionType == "DDOS_ATTACK" ||
                    log.ActionType == "PORT_SCAN" ||
                    log.ActionType == "INSIDER_THREAT" ||
                    log.ActionType == "LOGIN_SUCCESS" ||
                    log.ActionType == "AI_ANOMALY")
                {
                    unifiedAlerts.Add(new AlertViewModel
                    {
                        AlertId = log.LogId,
                        Timestamp = log.Timestamp,
                        AlertType = log.ActionType,
                        Severity = log.Status == "SUCCESS" ? "INFO" : (log.Status ?? "CRITICAL"),
                        Message = log.Details,
                        SourceIP = log.SourceIp
                    });
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"API Bağlantı Hatası: {ex.Message}";
        }

        return View(unifiedAlerts.OrderByDescending(a => a.Timestamp).ToList());
    }

    // 🚀 ADLİ BİLİŞİM & CENTRAL LOG DEPOSU (AUDIT LOGS)
    public async Task<IActionResult> AuditLogs()
    {
        List<AlertViewModel> archivedAlerts = new();

        try
        {
            var auditLogs = await _httpClient.GetFromJsonAsync<List<AuditLogDto>>("/api/Alert/audit-logs")
                            ?? new List<AuditLogDto>();

            foreach (var log in auditLogs)
            {
                if (log.ActionType == "DDOS_ATTACK" ||
                    log.ActionType == "PORT_SCAN" ||
                    log.ActionType == "INSIDER_THREAT" ||
                    log.ActionType == "LOGIN_SUCCESS" ||
                    log.ActionType == "AI_ANOMALY")
                {
                    archivedAlerts.Add(new AlertViewModel
                    {
                        AlertId = log.LogId,
                        Timestamp = log.Timestamp,
                        AlertType = log.ActionType,
                        Severity = log.Status == "SUCCESS" ? "INFO" : (log.Status ?? "CRITICAL"),
                        Message = log.Details,
                        SourceIP = log.SourceIp
                    });
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