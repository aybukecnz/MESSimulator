using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelMES.Infrastructure.Persistence;
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
            // Kural motorunu (FirewallPolicies) geçici olarak devre dışı bıraktık.
            // Doğrudan veritabanındaki son 5 siber olayı (Ülkesi olanları) çekiyoruz:
            var recentThreats = await _context.SystemAuditLogs
                .Where(log => log.CountryCode != null && log.CountryCode != "")
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .Select(static log => new
                {
                    ip = log.SourceIp, // Modelinde adı SourceIp ise onu yaz (altı kırmızı çizilmesin yeter)
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
