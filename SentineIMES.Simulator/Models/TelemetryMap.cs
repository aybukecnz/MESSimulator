using CsvHelper.Configuration;
using SentinelMES.Simulator.Models; // MachineTelemetry sınıfının olduğu yer

namespace SentinelMES.Simulator.Models
{
    public sealed class TelemetryMap : ClassMap<MachineTelemetry>
    {
        public TelemetryMap()
        {
            // CSV dosyasındaki başlıklar ile C# sınıfındaki property'leri eşliyoruz
            // CSV'deki başlık ismini tam olarak "Name" kısmına yazmalısın
            Map(m => m.WindSpeed).Name("Wind Speed (m/s)");
            Map(m => m.ActivePower).Name("LV ActivePower (kW)");
            Map(m => m.TheoreticalPower).Name("Theoretical_Power_Curve (KWh)");
            Map(m => m.WindDirection).Name("Wind Direction (°)");
        }
    }
}