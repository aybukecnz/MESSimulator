using System.Collections.Generic;
using System.Threading.Tasks;
using SentinelMES.Domain.Entities;

namespace SentinelMES.Application.Interfaces;

public interface IAllowedAssetRepository
{
    Task<AllowedAsset?> GetByIpAsync(string ip);
    Task<List<AllowedAsset>> GetAllAssetsAsync();
}