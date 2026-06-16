
namespace SentinelMES.Simulator.Services
{
    // Python'a gönderilecek SCADA verisi
    public class ScadaTelemetryRequest
    {
        public double wind_speed { get; set; }
        public double active_power { get; set; }
        public double theoretical_power { get; set; }
        public double wind_direction { get; set; }
    }

    // Python'dan gelecek Yapay Zeka yanıtı
    public class AiAnalysisResponse
    {
        public bool is_anomaly { get; set; }
        public double confidence_score { get; set; }
        public string xai_explanation { get; set; }
        public ScadaTelemetryRequest received_data { get; set; }
    }
}