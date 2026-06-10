namespace SentinelMES.WebUI.Models;

public class AuditLogDto
{
    public int LogId { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourceIp { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}