# Sentinel-MES: Endüstriyel Veri ve Anomali İzleme Merkezi

**Sentinel-MES**, Temiz Mimari (Clean Architecture) ile tasarlanmış, Kaggle verileriyle desteklenen, internete kapalı (Air-gapped) OT sistemleri için yapay zekâ destekli siber güvenlik ve operasyonel izleme platformudur.

##  Proje Fazları

### Faz 1: Hibrit Simülasyon Motoru (Data Streaming & Threat Injection)
Sıfırdan rastgele veri üretmek yerine, gerçek dünya verisi kullanılarak oluşturulan ve üzerine siber zafiyetlerin eklendiği canlı veri akış katmanı.

* **Fiziksel Akış (Kaggle SCADA Verisi):** Gerçek bir rüzgar türbinine (veya üretim makinesine) ait geçmiş veriler (sıcaklık, rüzgar hızı, güç) `CsvHelper` ile satır satır okunur. Zaman damgası "şu anki zamana" güncellenerek saniyede bir PostgreSQL veritabanına (`MachineTelemetry` tablosu) yazılır. Sistem gerçek bir PLC gibi davranır.
* **Siber Tehdit Enjeksiyonu (Bogus Kütüphanesi):** Fiziksel veri akarken, arka planda çalışan C# Worker Service bir "Olasılık Zarı" atar. Çıkan sonuca göre `SystemAuditLogs` tablosuna sahte siber loglar basılır.
* **Senaryolar:**
  * **Normal Operasyon (%84):** Sadece fiziksel veri akar.
  * **Rutin Giriş (%10):** Vardiya başı operatör giriş logları.
  * **Kaba Kuvvet Saldırısı (%4):** Dış IP'lerden peş peşe hatalı "Admin" şifre denemeleri.
  * **İç Tehdit / Sabotaj (%2):** Gece saatlerinde yetkisiz bir IP'den gelen "Reçete Güncelleme" (UPDATE_RECIPE) komutları.

### Faz 2: SQL ve Veritabanı Katmanı (Merkezi Zeka ve OT SOC Kuralları)
Sistemin bel kemiği olan PostgreSQL, sadece depo değil, aynı zamanda ilk savunma hattıdır.

* **Dinamik Eşik Tabloları (SystemThresholds):** Makine sensörlerinin alt/üst sınırları SQL tablosunda tutulur. Başka bir fabrikaya kurulurken sadece bu tablo güncellenir.
* **Ağırlaştırılmış Log Tabloları (SystemAuditLogs):** OT SOC mantığına uygun olarak; MAC adresi, IP adresi, hedef tablo (örneğin reçeteler veya iş emirleri) ve işlem tipinin kaydedildiği yapı.
* **SQL Trigger'ları:** Aşırı kritik bir durum olduğunda (örn: Reçete değişimi) .NET'i beklemeden doğrudan veritabanı seviyesinde alarm flag'leri üretilmesi.

### Faz 3: .NET 9 Backend ve Temiz Mimari (OT SOC Kural Motoru)
Sistemin beyni olan API, MES dokümanlarındaki teorik riskleri gerçek zamanlı olarak izler ve yakalar.

* **Veri Toplama (Data Acquisition) Koruması:** Saniyede 10 satır veri atan bir makineden saniyede 1000 satır veri gelirse DDoS / Sorgu Taşkını alarmı üretilir.
* **Ağ ve Donanım (Network/Hardware) Koruması:** Sistemde daha önce hiç görülmemiş bir IP veya MAC adresi ağda veri çekmeye çalışırsa Gölge IT (Shadow IT) alarmı üretilir. Makine IP'leri birbiriyle konuşmaya çalışırsa Yatay Hareket (Lateral Movement) tespiti yapılır.
* **Üretim Teorisi (MES Production) Koruması:** Reçete (Recipe) değişiklikleri, iş emri (Work Order) silinmesi veya bir operatörün admin ekranına girmesi İç Tehdit ve Ayrıcalık Yükseltme olarak algılanıp engellenir.

### Faz 4: Açıklanabilir Yapay Zekâ (XAI Entegrasyonu)
Klasik statik kuralların (Faz 3) göremediği, sistemdeki gizli dengesizlikleri bulan yapay zekâ modülü.

* **Stacking Ensemble Modeli:** Kaggle'dan gelen fiziksel SCADA verileri ile Bogus'tan gelen siber loglar harmanlanarak modele öğretilir. Model, "Bu makine çalışmıyorken neden buradan SQL'e veri geliyor?" gibi mantıksal bozuklukları (Fiziksel İmkansızlık/Spoofing) öğrenir.
* **SHAP (Açıklanabilir AI):** Dashboard'da "Anomali var" demek yerine; "Uyarı: %92 Anomali. Sebep: Makine hızı normal, ancak bağlı olduğu PLC'nin MES sunucusuna attığı SQL sorgu frekansı son 1 saatte 10 kat arttı (Potansiyel Zararlı Yazılım)." şeklinde net açıklamalar üretir.

### Faz 5: Dashboard ve Kriz İletişimi (Kullanıcı Arayüzü)
Tüm verilerin izlendiği ve internete kapalı ağlarda bile bildirim gönderebilen kontrol merkezi.

* **OT SOC Paneli:** Ekranın solunda canlı makine değerleri (Rüzgar/Sıcaklık), sağında ise gerçek zamanlı siber tehdit uyarıları (Kırmızı alarmlar, engellenen IP'ler).
* **Offline Bildirim Sistemi:** Şirket internete kapalı (Air-gapped) olduğu için e-posta kullanılamadığından, sunucuya bağlı Fiziksel GSM Modem üzerinden çalışan .NET servisi ile kritik olaylarda (Örn: Reçete manipülasyonu) yöneticilere doğrudan SMS atılması.
