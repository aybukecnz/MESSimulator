using SentinelMES.Application.Interfaces;
using SentinelMES.Domain.Entities;
using SentinelMES.Infrastructure.Persistence;

namespace SentinelMES.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly SentinelDbContext _context;

    public AlertRepository(SentinelDbContext context)
    {
        _context = context;
    }

    public async Task AddAlertAsync(ActiveAlert alert)
    {
        // Yeni bir siber güvenlik alarmını veritabanına ekle ve kaydet
        await _context.ActiveAlerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }

    public async Task AddAuditLogAsync(SystemAuditLog log)
    {
        // Kaba kuvvet (Brute-Force) loglarını denetim tablosuna ekle ve kaydet
        await _context.SystemAuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}