namespace SentinelMES.WebUI.Models;

public class AlertViewModel
{
    public int AlertId { get; set; }
    public DateTime Timestamp { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}