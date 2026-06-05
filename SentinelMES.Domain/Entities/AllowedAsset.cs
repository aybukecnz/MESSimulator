namespace SentinelMES.Domain.Entities;

public class AllowedAsset
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string AllowedIp { get; set; } = string.Empty;
    public string AllowedMac { get; set; } = string.Empty;
}