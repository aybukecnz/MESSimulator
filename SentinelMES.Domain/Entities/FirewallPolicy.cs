using System;

namespace SentinelIMES.Domain.Entities
{
    public class FirewallPolicy
    {
        public int Id { get; set; }

        // Örn: "RU", "CN", "US"
        public required string CountryCode { get; set; }

        // Örn: "Russia", "China"
        public required string CountryName { get; set; }

        // Bu ülke şu an engelli mi?
        public bool IsBlocked { get; set; }

        // Kuralın türü. Örn: "GEO_BLOCK" (Coğrafi Engelleme)
        public required string PolicyType { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}