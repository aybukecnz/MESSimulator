using System;

namespace SentinelMES.Domain.Entities;

public class ActiveAlert
{
    public int AlertId { get; set; }
    public DateTime Timestamp { get; set; }
    public string AlertType { get; set; } = string.Empty; // Örn: 'IP_MAC_SPOOFING'
    public string Severity { get; set; } = string.Empty;  // Örn: 'CRITICAL'
    public string Message { get; set; } = string.Empty;   // Alarm detay mesajı
}