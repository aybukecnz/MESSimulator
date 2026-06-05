using Microsoft.AspNetCore.Mvc;
using SentinelMES.Application.Interfaces;
using System.Runtime.InteropServices;

namespace SentinelMES.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NetworkTrafficController : ControllerBase
{
    private readonly ISecurityRuleEngine _ruleEngine;

    public NetworkTrafficController(ISecurityRuleEngine ruleEngine)
    {
        _ruleEngine = ruleEngine;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeTraffic([FromBody] NetworkPacketDto packet)
    {
        // Gelen paketi IP-MAC kural motoruna sokuyoruz
        bool isSafe = await _ruleEngine.ProcessNetworkTrafficAsync(
            packet.IpAddress,
            packet.MacAddress,
            packet.Username,
            packet.ActionType,
            packet.Details);

        if (!isSafe)
        {
            return BadRequest(new { Message = "SİBER GÜVENLİK İHLALİ! Paket engellendi ve SOC paneline raporlandı." });
        }

        return Ok(new { Message = "İşlem güvenli, ağ geçişine izin verildi." });
    }
}

// Gelen isteklerin JSON formatını karşılayacak Veri Transfer Nesnesi (DTO)
public class NetworkPacketDto
{
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}