# Sentinel-MES: Industrial OT-SIEM & SOAR Platform

**Sentinel-MES**, endüstriyel operasyonel teknoloji (OT) ortamları için özel olarak geliştirilmiş; **Purdue Modeli (ICS Referans Mimarisi)** hiyerarşisine tam uyumlu, hibrit bir **OT-SIEM** ve **SOAR** platformudur.

Kritik altyapıların (enerji santralleri, akıllı fabrikalar) dijital ikizlerini simüle ederek, fiziksel SCADA verileri ile siber tehditleri (DDoS, Spoofing, Insider Threat) eş zamanlı izler ve **Açıklanabilir Yapay Zeka (XAI)** ile forensik raporlar üretir.**Zero-Trust** prensibine dayalı olarak; ağdaki cihazların **IP/MAC eşleşmesini** anlık denetler ve ağ üzerindeki anomalileri forensik düzeyde analiz eder.

![Dashboard Overview](Docs/dashboard.png)

![Canlı Telemetri Simülasyonu](Docs/MESSimulatorr.gif)

---
## Purdue Modeli ve Endüstriyel Hiyerarşi
Sentinel-MES, endüstriyel ağ güvenliğinin temelini oluşturan **Purdue Model (ICS Referans Mimarisi)** hiyerarşisine göre modüler tasarlanmıştır:

* **Level 0-2 (Process & Control Layer):** `SentinelMES.Simulator` katmanı; Kaggle SCADA verilerini ve sahte siber saldırı vektörlerini (Bogus) enjekte ederek gerçek bir operasyonel simülasyon ortamı simüle eder.
* **Level 3 (Site Operations Layer):** `SentinelMES.WebAPI` (Clean Architecture); üretim mantığını yönetir, kural motoru üzerinden ağ trafiğini filtreler ve "Zero-Trust" prensibini uygular.
* **Level 4-5 (Enterprise/SOC Layer):** `SentinelMES.WebUI`; XAI destekli forensik analiz, anlık tehdit radarı ve olay müdahale merkezi görevini görür.

> **Mimari Vizyon:** Sistem, API (Backend) ve UI (Frontend) olarak izole edilmiştir. Ayrıca veritabanı ile kullanıcı arayüzü sunucularının farklı ağlarda tutulması (Air-gap) siber saldırılara karşı maksimum izolasyon sağlar.

##  Dataset ve Veri Kaynağı
Sentinel-MES, siber güvenlik ve operasyonel verilerin simülasyonu için iki ana veri kaynağından beslenmektedir:

1. **Siber Güvenlik Verisi (Network):** [UNB CIC-IDS-2017 Datasets](https://www.unb.ca/cic/datasets/index.html). Siber saldırı vektörlerini (DDoS, Port Scan vb.) simüle etmek için kullanılır.
2. **Fiziksel SCADA Verisi (Physical):** Kaggle üzerinde paylaşılan açık kaynaklı "Wind Turbine SCADA Data" seti. Fiziksel rüzgar türbini parametrelerini (rüzgar hızı, sıcaklık, aktif güç) simüle etmek için kullanılır.

> **Kurulum:** Datasetleri ilgili kaynaklardan temin ettikten sonra `.csv` dosyalarını `SentinelMES.Simulator/Scripts/` dizinine yerleştirmeniz yeterlidir. Sistem, bu veriyi `Worker Service` üzerinden işleyerek canlı bir operasyonel simülasyon akışına dönüştürür.
---
##  Uçtan Uca Sistem Mimarisi (Clean Architecture)
Sentinel-MES, **Clean Architecture** prensiplerini temel alan **3 çekirdek katman** ile sistemin operasyonel işleyişini sağlayan **4 destekleyici modülün** entegrasyonuyla toplam **7 ana yapı** üzerinden çalışmaktadır:

![Sistem Mimari Şeması](Docs/db.drawio.png)

### A. Clean Architecture (Çekirdek Katmanlar)
1. **`SentinelMES.Domain` (Çekirdek):** Mimarinin en iç katmanıdır. `ActiveAlert` ve `SystemAuditLog` gibi sistemin ana varlıklarını (Entities) tutar. Hiçbir dış teknolojiye bağımlı değildir.
2. **`SentinelMES.Application` (İş Kuralları):** Sistemde "nelerin yapılacağını" (Örn: `IAlertRepository`, Use-Case handler'ları) tanımlar. Arayüzleri ve iş sözleşmelerini barındırır.
3. **`SentinelMES.Infrastructure` (Altyapı):** Veritabanı ve dış dünya ile asıl iletişimin kurulduğu yerdir. PostgreSQL bağlantıları, Entity Framework Core Migrations ve Repositories burada bulunur.

### B. Operasyonel Servisler (Destekleyici Modüller)
4. **`SentinelMES.Simulator` (Canlı Simülasyon):** Kaggle verilerini ve Bogus siber saldırılarını arka planda sisteme enjekte eden "Worker Service" test motorudur.
5. **`SentinelMES.WebAPI` (Güvenlik Duvarı):** Dış dünyaya açılan yegane kapıdır. Tüm veriler bu RESTful API'nin kural motorundan (Detect) geçerek filtrelenir ve yetkisiz erişimler bloklanır.
6. **`SentinelMES.AI` (XAI Motoru):** Python/FastAPI tabanlı, `XGBoost` ve `SHAP` kütüphanelerini kullanarak karmaşık siber saldırıları ve anomali kök nedenlerini analiz eden yapay zeka servisidir.
7. **`SentinelMES.WebUI` (SOC Komuta Kontrol):** Güvenlik analistleri için karanlık tema (Dark Mode), olay müdahale butonları (Incident Response) ve XAI analiz arayüzleri sunan MVC tabanlı izleme panelidir.

> **Mimari Vizyon:** Sistem, **API (Backend)**, **AI Engine (Python)** ve **UI (Frontend)** olarak izole edilmiştir. Bu modüler yapı sayesinde her servis bağımsızca ölçeklendirilebilir. Veritabanı ile kullanıcı arayüzü sunucularının farklı ağlarda tutulması (**Air-gap**), siber saldırılara karşı maksimum izolasyon sağlar.

##  Teknolojiler 

| Kategori | Teknolojiler |
| :--- | :--- |
| **Backend** | .NET 9.0 (Clean Architecture), WebAPI |
| **AI Engine** | Python (FastAPI), XGBoost, SHAP (XAI) |
| **Database** | PostgreSQL (Relational & Audit Storage) |
| **Frontend** | ASP.NET Core MVC (Razor), Bootstrap 5, Chart.js |
| **Dev/Simulation**| CsvHelper, Bogus (Threat Injection) |

---

## Proje Fazları ve Yol Haritası

### Faz 1: Hibrit Simülasyon Motoru (Data Streaming)
`CsvHelper` ile gerçek rüzgar türbini verileri işlenir. `Bogus` kütüphanesi ile sistem, saniyede %16 oranında siber manipülasyon (DDoS, Port Scan, Spoofing) enjekte eder.

### Faz 2: SQL ve Veritabanı (OT SOC Kuralları)
PostgreSQL; dinamik eşik tabloları (`SystemThresholds`) ve SQL Trigger'ları ile .NET'i beklemeden doğrudan veritabanı seviyesinde savunma hattı kurar.

### Faz 3: .NET 9 Backend (Kural Motoru)
DDoS, Yatay Hareket (Lateral Movement) ve yetkisiz reçete değişikliklerini anlık tespit eden kural motoru.

### Faz 4: Açıklanabilir Yapay Zekâ (XAI)
**XGBoost & SHAP** ile sistem sadece "Anomali var" demez; anomalinin hangi parametreden (Örn: "Spoofing: Güç verisi fiziksel olarak imkansız") kaynaklandığını açıklar.

![AI](Docs/ai-diagram.drawio.png)


![AI Anomali Analizi](Docs/ai_anomali.png)

### Faz 5: SOC Paneli ve Olay Müdahale (Incident Response)

İnternete kapalı (Air-gapped) ağlarda çalışan; anlık tehdit radarı, Geo-IP takibi ve SOC analistlerinin tehditleri yönetebildiği merkezi izleme panelidir.

---

##  SOC Komuta Kontrol Arayüzü

**1. Network Infractions (DDoS & Port Scan):**
![Network Tab](Docs/auditlogs_network.png)

**2. SCADA Telemetry Anomalies (Spoofing):**
![SCADA Tab](Docs/auditlogs_scada.png)

**3. Identity & Access Logs (Insider Threat):**
![Identity Tab](Docs/auditlogs_identity.png)