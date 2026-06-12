using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelMES.Infrastructure.Persistence; // Kendi projendeki doğru klasör olduğuna emin ol
using System.Linq;
using System.Threading.Tasks;

namespace SentinelIMES.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RadarController : ControllerBase
    {
        private readonly SentinelDbContext _context;

        public RadarController(SentinelDbContext context)
        {
            _context = context;
        }

        // UI'daki JS Fetch metodunun her 3 saniyede bir çağıracağı Endpoint
        [HttpGet("live-threats")]
        public async Task<IActionResult> GetLiveThreats()
        {
            var recentThreats = await _context.SystemAuditLogs
                .Where(log => log.CountryCode != null && log.CountryCode != "")
                // SADECE GERÇEK SİBER TEHDİTLER RADARA DÜŞSÜN:
                .Where(log => log.ActionType == "DDOS_ATTACK" || log.ActionType == "PORT_SCAN" || log.ActionType == "INSIDER_THREAT")
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .Select(log => new
                {
                    ip = log.SourceIp, // DİKKAT: Veritabanı modeline uygun olarak "SourceIp" yapıldı!
                    countryName = log.CountryName,
                    countryCode = log.CountryCode,
                    attackType = log.ActionType,
                    time = log.Timestamp.ToString("HH:mm:ss")
                })
                .ToListAsync();

            return Ok(recentThreats);
        }
    }
}