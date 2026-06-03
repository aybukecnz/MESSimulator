using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentinelMES.Simulator.Models;

public class MachineTelemetry
{
    public int TelemetryId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal ActivePower { get; set; }
    public decimal WindSpeed { get; set; }
    public decimal TheoreticalPower { get; set; }
    public decimal WindDirection { get; set; }
}
