using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SentinelMES.Simulator.Services;
using System.IO;

namespace SentinelMES.Simulator
{
    public class Worker : BackgroundService
    {
        private readonly CsvStreamingService _csvService;

        public Worker(CsvStreamingService csvService)
        {
            _csvService = csvService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Veritabanını kirleten eski Bogus kodlarının hepsini sildim
            // Sadece tek bir gerçek motor çalışacak:

            string scadaPath = Path.Combine(Directory.GetCurrentDirectory(), "wind_turbine_scada.csv");

            await _csvService.StreamDataAsync(scadaPath, stoppingToken);
        }
    }
}