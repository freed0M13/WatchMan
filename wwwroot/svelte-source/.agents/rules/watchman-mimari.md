---
trigger: always_on
---

# Watchman NIDS - Proje Bağlamı ve Mimari Kurallar

## 1. Proje Özeti
Watchman, ağ trafiğini anlık olarak izleyen (DPI, DoS, Port Tarama) ve Tak-Çalıştır (Drop-and-Run) mantığıyla çalışan bir Ağ Saldırı Tespit Sistemidir (NIDS). Proje C# .NET 8 Worker Service + API ve Svelte Frontend mimarisinden oluşmaktadır.

## 2. Backend (C# .NET 8) Mimarisi
* **Paket Dinleyici:** `SharpPcap` ve `PacketDotNet` kullanılarak ağ kartından geçen veriler "Promiscuous" modda dinlenmektedir.
* **Kurulum:** Sistem ilk açıldığında Npcap sürücüsü yoksa, gömülü kaynaklardan (Embedded Resource) otomatik olarak kurulum ekranını başlatır.
* **Tehdit Analizi:** Syn Flood (DoS), Port Tarama ve DPI (Derin Paket İnceleme - X-Forwarded-For vb.) tespiti yapılmaktadır.
* **Log Rotasyonu:** Veritabanı şişmesini önlemek için `DatabaseArchiver` sınıfı logları periyodik olarak uzak sunucuya yedekler ve yerel dosyayı sıfırlar.

## 3. Veritabanı (SQLite + SQLCipher)
* **Dosya Adı:** `monitor.db` (Proje kök dizininde çalışır).
* **Şifreleme:** AES-256 (`SQLitePCLRaw.bundle_e_sqlcipher` ile).
* **Veritabanı Şifresi:** `SiberGuvenlik_Watchman_Key_2026!`
* **Önemli Tablolar:**
    * `ALERTS`: Tespit edilen tehditler (AlertType, Severity, SourceIp, Module, TargetPort).
    * `APP_SETTINGS`: Sistem ayarları (ActiveNetworkInterface, LogRetentionDays).

## 4. API Uç Noktaları (Svelte Tarafından Tüketilecek)
Svelte arayüzü aşağıdaki Kestrel REST API uçlarını (varsayılan: `http://localhost:5000`) kullanarak verileri çekecektir:

* **`GET /api/networkanalysis/metrics`**
    * *Dönen Veri:* `GlobalPacketRate`, `ThroughputBandwidth`, `TopTalkers` (Dizi), `ProtocolDistribution`. Canlı istatistikleri besler.
* **`GET /api/networkanalysis/threats`**
    * *Dönen Veri:* Yakalanan son canlı tehditlerin listesi (`Id`, `Time`, `Type`, `SourceIp`, `Severity`, `Module`).
* **`GET /api/settings/interfaces`**
    * *Dönen Veri:* Sistemdeki mevcut ağ kartı donanımları (`Id`, `Name`, `Description`, `MacAddress`).
* **`GET /api/settings`**
    * *Dönen Veri:* Mevcut kaydedilmiş ayarlar (Örn: `{ "ActiveNetworkInterface": "2", "LogRetentionDays": "7" }`).
* **`POST /api/settings`**
    * *Gönderilen Veri:* Dictionary (Key-Value) şeklinde yeni ayarlar. (Örn: `{ "ActiveNetworkInterface": "1" }`).

## 5. Svelte (Frontend) Geliştirme Kuralları
* Modern, koyu tema (Dark Mode) ağırlıklı, siber güvenlik temasına (Monitor/HIDS) uygun bir UI tasarlanmalıdır.
* `metrics` ve `threats` endpointleri Dashboard üzerinde `setInterval` ile sık sık (örn: 1-2 saniyede bir) çağrılmalı ve grafikler anlık güncellenmelidir.
* Ayarlar sayfasında, ağ kartı seçimi için `interfaces` apisinden gelen verilerle bir Dropdown (Select) oluşturulmalı ve varsayılan olarak `APP_SETTINGS`'ten gelen ID seçili bırakılmalıdır.