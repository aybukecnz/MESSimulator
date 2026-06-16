namespace SentinelMES.Domain.Entities;

public class IncidentTriage
{
    public int LogId { get; set; } // Audit Log ile eşleşecek ID
    public string Status { get; set; } // "TRUE_POSITIVE" veya "FALSE_POSITIVE"
    public string? AnalystNote { get; set; }
    public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
}