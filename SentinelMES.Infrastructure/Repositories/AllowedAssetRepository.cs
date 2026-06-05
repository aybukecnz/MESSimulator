using Microsoft.EntityFrameworkCore;
using SentinelMES.Application.Interfaces;
using SentinelMES.Domain.Entities;
using SentinelMES.Infrastructure.Persistence;

namespace SentinelMES.Infrastructure.Repositories;

public class AllowedAssetRepository : IAllowedAssetRepository
{
    private readonly SentinelDbContext _context;

    // Veritabanı bağlantımızı (DbContext) Constructor üzerinden içeri alıyoruz
    public AllowedAssetRepository(SentinelDbContext context)
    {
        _context = context;
    }

    public async Task<AllowedAsset?> GetByIpAsync(string ip)
    {
        // Gelen IP adresini veritabanındaki izinli listesinde arar
        return await _context.AllowedAssets.FirstOrDefaultAsync(a => a.AllowedIp == ip);
    }

    public async Task<List<AllowedAsset>> GetAllAssetsAsync()
    {
        // Tüm güvenli cihaz envanterini getirir
        return await _context.AllowedAssets.ToListAsync();
    }
}