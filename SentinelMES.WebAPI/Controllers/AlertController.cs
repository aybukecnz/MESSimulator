using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelMES.Infrastructure.Persistence;
using System.Runtime.InteropServices;

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
        // En son 300 şüpheli işlemi (Brute-Force, Reçete manipülasyonu vb.) getir
        var logs = await _context.SystemAuditLogs
                                 .OrderByDescending(l => l.Timestamp)
                                 .Take(300)
                                 .ToListAsync();
        return Ok(logs);
    }
}