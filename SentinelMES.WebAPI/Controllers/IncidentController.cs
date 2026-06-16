using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelMES.Domain.Entities;
using SentinelMES.Infrastructure.Persistence;


namespace SentinelMES.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly SentinelDbContext _db;

    public IncidentController(SentinelDbContext db)
    {
        _db = db;
    }

    [HttpPost("triage")]
    public async Task<IActionResult> SaveTriage([FromBody] TriageRequest request)
    {
        // Önce kayıt var mı bak (Upsert mantığı)
        var existing = await _db.IncidentTriage.FindAsync(request.LogId);

        if (existing != null)
        {
            existing.Status = request.Status;
            existing.ResolvedAt = DateTime.UtcNow;
            _db.IncidentTriage.Update(existing);
        }
        else
        {
            _db.IncidentTriage.Add(new IncidentTriage
            {
                LogId = request.LogId,
                Status = request.Status,
                ResolvedAt = DateTime.UtcNow
            });
        }
        // 4. Çift tıklama çakışmalarını önlemek için güvenli kayıt işlemi
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { message = "Karar başarıyla kaydedildi." });
        }
        catch (DbUpdateException)
        {
            // Eğer saliselik bir çift tıklama olduysa ve veritabanı "bu zaten var" derse,
            // uygulamayı çökertme, sadece başarılı dön.
            return Ok(new { message = "Bu olay zaten daha önce işlenmiş." });
        }
       
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllTriage()
    {
        var list = await _db.IncidentTriage.ToListAsync();
        return Ok(list);
    }
}

public class TriageRequest { public int LogId { get; set; } public string Status { get; set; } }