using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelMES.Infrastructure.Persistence;
using System.Runtime.InteropServices;
using SentinelMES.WebUI.Models;



namespace SentinelMES.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly SentinelDbContext _context;

    public AlertController(SentinelDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        // En son çıkan 300 aktif siber güvenlik alarmını getir
        var alerts = await _context.ActiveAlerts
                                   .OrderByDescending(a => a.Timestamp)
                                   .Take(300)
                                   .ToListAsync();
        return Ok(alerts);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        // Veritabanından ham verileri çek
        var logs = await _context.SystemAuditLogs
                                         .OrderByDescending(l => l.Timestamp)
                                         .Take(300)
                                         .ToListAsync();

        //  Burada veritabanı modelini, UI'ın beklediği ViewModel'e dönüştürüyoruz (MAPPING)
        var viewModels = logs.Select(l => new AlertViewModel
        {
            AlertId = l.LogId, // LogId'yi AlertId'ye eşliyoruz
            Timestamp = l.Timestamp,
            SourceIP = l.SourceIp,
            AlertType = l.ActionType, // ActionType'ı AlertType olarak eşliyoruz
            Message = l.Details,      // Eski Message alanı
            Details = l.Details       //  YENİ: İşte eksik olan Details alanı buraya geliyor!
        }).ToList();

        return Ok(viewModels);
    }
}