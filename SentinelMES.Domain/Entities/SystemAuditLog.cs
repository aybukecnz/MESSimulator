using System;

namespace SentinelMES.Domain.Entities;

public class SystemAuditLog
{
    public int LogId { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourceIp { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}