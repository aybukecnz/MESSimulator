using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentinelMES.Simulator.Models;

public class SystemAuditLog
{
    public int LogId { get; set; }
    public DateTime Timestamp { get; set; }
    public required string SourceIP { get; set; }
    public required string UserName { get; set; }
    public required string ActionType { get; set; }
    public required string Status { get; set; }
    public required string CountryName { get; set; }
    public required string CountryCode { get; set; }

    public required string Details { get; set; }
}