using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// DİKKAT: Burada 'I' harfi var mı yok mu kendi projene göre kontrol et. 
// Çoğu yerde SentinelMES kullanmıştık.
namespace SentinelMES.Simulator.Services
{
    public class ThreatDatasetReader
    {
        private List<string[]> _ddosPayloads = new();
        private List<string[]> _portScanPayloads = new();
        private readonly Random _random = new();

        public void LoadDatasets(string ddosPath, string portScanPath)
        {
            try
            {
                if (File.Exists(ddosPath))
                {
                    Console.WriteLine("[SİBER İSTİHBARAT] DDoS Dataset belleğe alınıyor...");
                    _ddosPayloads = File.ReadLines(ddosPath).Skip(1).Take(5000).Select(line => line.Split(',')).ToList();
                }

                if (File.Exists(portScanPath))
                {
                    Console.WriteLine("[SİBER İSTİHBARAT] PortScan Dataset belleğe alınıyor...");
                    _portScanPayloads = File.ReadLines(portScanPath).Skip(1).Take(5000).Select(line => line.Split(',')).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HATA] Dataset okuma hatası: {ex.Message}");
            }
        }

        public string[] GetRandomAttack(string attackType)
        {
            var targetList = attackType == "DDOS" ? _ddosPayloads : _portScanPayloads;
            if (targetList == null || !targetList.Any()) return null;
            int index = _random.Next(targetList.Count);
            return targetList[index];
        }
    }
}