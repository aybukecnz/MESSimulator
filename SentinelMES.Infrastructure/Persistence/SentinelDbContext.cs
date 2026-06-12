using Microsoft.EntityFrameworkCore;
using SentinelIMES.Domain.Entities;
using SentinelMES.Domain.Entities;

namespace SentinelMES.Infrastructure.Persistence;

public class SentinelDbContext : DbContext
{
    public SentinelDbContext(DbContextOptions<SentinelDbContext> options) : base(options)
    {
    }

    public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
    public DbSet<ActiveAlert> ActiveAlerts { get; set; }
    public DbSet<AllowedAsset> AllowedAssets { get; set; }
    public DbSet<FirewallPolicy> FirewallPolicies { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL'in orijinal küçük harfli tablolarına Fluent API mapping yapıyoruz
        modelBuilder.Entity<SystemAuditLog>(entity =>
        {
            entity.ToTable("systemauditlogs", "public");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.LogId).HasColumnName("logid");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.SourceIp).HasColumnName("sourceip");
            entity.Property(e => e.UserName).HasColumnName("username");
            entity.Property(e => e.ActionType).HasColumnName("actiontype");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Details).HasColumnName("details");
        });

        modelBuilder.Entity<ActiveAlert>(entity =>
        {
            entity.ToTable("activealerts", "public");
            entity.HasKey(e => e.AlertId);
            entity.Property(e => e.AlertId).HasColumnName("alertid");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.AlertType).HasColumnName("alerttype");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Message).HasColumnName("message");
        });

        modelBuilder.Entity<AllowedAsset>(entity =>
        {
            entity.ToTable("allowedassets", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeviceName).HasColumnName("devicename");
            entity.Property(e => e.AllowedIp).HasColumnName("allowedip");
            entity.Property(e => e.AllowedMac).HasColumnName("allowedmac");
        });
    }
}