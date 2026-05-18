using Microsoft.Data.Sqlite;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Watchman.Data
{
    public static class DatabaseArchiver
    {
        public static bool IsDatabaseRotating { get; private set; } = false;

        // Merkezi log toplama sunucunuzun API adresi (İleride APP_SETTINGS tablosundan dinamik de okunabilir)
        private const string RemoteLogServerUrl = "https://api.watchman-security.com/v1/logs/upload";
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task RotateAndUploadDatabaseAsync()
        {
            Console.WriteLine("[DB ARCHIVER] Log döndürme döngüsü tetiklendi...");
            IsDatabaseRotating = true;

            string basePath = AppContext.BaseDirectory;
            string dbPath = Path.Combine(basePath, "monitor.db");
            string backupPath = Path.Combine(basePath, $"monitor_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            try
            {
                SqliteConnection.ClearAllPools();
                await Task.Delay(500);

                if (File.Exists(dbPath))
                {
                    File.Move(dbPath, backupPath);
                    Console.WriteLine($"[DB ARCHIVER] Eski loglar paketlendi: {Path.GetFileName(backupPath)}");
                }

                // Yukarıda güncellediğimiz metot: Sıfır DB açacak
                DatabaseInitializer.Initialize("base-db.sql");
            }
            finally
            {
                // Yeni DB oluştuktan hemen sonra kilit açılır, NIDS paket yazmaya devam edebilir
                IsDatabaseRotating = false;
            }

            // Arka planda sunucuya yükleme ve temizlik başlasın (Ağ trafiği kesintiye uğramaz)
            _ = UploadToServerAndDeleteAsync(backupPath);
        }

        private static async Task UploadToServerAndDeleteAsync(string filePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                Console.WriteLine($"[DB ARCHIVER] {Path.GetFileName(filePath)} uzak sunucuya transfer ediliyor...");

                // HTTP POST isteği için form içeriği hazırlama
                using var form = new MultipartFormDataContent();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var streamContent = new StreamContent(fileStream);

                // Sunucu güvenliği için dosya tipini binary akış olarak işaretliyoruz
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Form verisine dosyayı ekliyoruz ("file" parametre adı sunucu tarafındaki karşılayıcı ile aynı olmalı)
                form.Add(streamContent, "file", Path.GetFileName(filePath));

                // İstek gönderiliyor
                HttpResponseMessage response = await _httpClient.PostAsync(RemoteLogServerUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[DB ARCHIVER] Transfer başarılı. Sunucu log paketini onayladı.");

                    // Dosya akışını (stream) kapatıp diski serbest bırakıyoruz ki silebilelim
                    fileStream.Close();

                    File.Delete(filePath);
                    Console.WriteLine("[DB ARCHIVER] Şifreli yerel yedek güvenli bir şekilde imha edildi.");
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[HATA] Sunucu logu kabul etmedi. Durum Kodu: {response.StatusCode}, Detay: {errorDetails}");
                    // File.Delete ÇALIŞMAZ. Dosya diskte kalır, bir sonraki döngüde manuel veya otomatik tekrar denenebilir.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KRİTİK HATA] Log transferi ağ hatası nedeniyle başarısız oldu: {ex.Message}");
            }
        }
    }
}