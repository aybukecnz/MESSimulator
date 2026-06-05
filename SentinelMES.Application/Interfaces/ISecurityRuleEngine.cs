using System.Threading.Tasks;

namespace SentinelMES.Application.Interfaces;

public interface ISecurityRuleEngine
{
    Task<bool> ProcessNetworkTrafficAsync(string ip, string mac, string username, string actionType, string details);
}