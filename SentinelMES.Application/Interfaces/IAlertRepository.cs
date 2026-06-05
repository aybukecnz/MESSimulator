using System.Threading.Tasks;
using SentinelMES.Domain.Entities;

namespace SentinelMES.Application.Interfaces;

public interface IAlertRepository
{
    Task AddAlertAsync(ActiveAlert alert);
    Task AddAuditLogAsync(SystemAuditLog log);
}