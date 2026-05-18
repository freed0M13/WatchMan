using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace Watchman.Driver
{
    public class NpcapInstaller
    {
        public static void EnsureInstalled()
        {
            if (IsNpcapInstalled())
            {
                Console.WriteLine("[Watchman Setup] Npcap çekirdek sürücüsü zaten kurulu. Analiz motoruna geçiliyor.");
                return;
            }

            Console.WriteLine("[Watchman Setup] Npcap bulunamadı. Gömülü sessiz kurulum başlatılıyor...");
            ExtractAndInstall();
        }

        private static bool IsNpcapInstalled()
        {
            // Windows Kayıt Defterinden (Registry) Npcap'in kurulu olup olmadığını denetliyoruz
            // Hem 64-bit hem 32-bit (WOW6432Node) dizinleri kontrol edilir
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Npcap") ??
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Npcap");

            return key != null;
        }

        private static void ExtractAndInstall()
        {
            try
            {
                // 1. Gömülü dosyanın proje içindeki tam yolu (Namespace.KlasörAdı.DosyaAdı formatında olmalı)
                // "Watchman" kısmını kendi ana projenin adıyla (Namespace) değiştirmelisin.
                string resourceName = "Watchman.Driver.npcap-setup.exe";

                // 2. Dosyanın geçici olarak çıkartılacağı Temp klasörü
                string tempPath = Path.Combine(Path.GetTempPath(), "watchman-npcap-setup.exe");

                // 3. Dosyayı hafızadan okuyup diske yazma
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception($"Gömülü kaynak bulunamadı: {resourceName}. Build Action ayarını kontrol edin.");

                    using (FileStream fileStream = new FileStream(tempPath, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                    }
                }

                Console.WriteLine("[Watchman Setup] Kurulum dosyası geçici belleğe alındı, sürücü yükleniyor...");

                // 4. Arka planda sessiz kurulum komutu verme
                var processInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    // /S parametresi ücretsiz sürümde engellendiği için kaldırıldı.
                    Arguments = "/loopback=yes /winpcap_mode=yes",
                    Verb = "runas",
                    UseShellExecute = true,
                    // Kurulum penceresinin ekranda görünebilmesi için false yapıyoruz
                    CreateNoWindow = false
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit(); // Kurulum bitene kadar bekle

                if (process?.ExitCode == 0)
                    Console.WriteLine("[Watchman Setup] Npcap başarıyla kuruldu!");
                else
                    Console.WriteLine("[Watchman Setup] Uyarı: Kurulum beklenmedik bir kod ile sonlandı.");

                // 5. Temizlik
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KRİTİK HATA] Npcap kurulumu başarısız: {ex.Message}");
                Console.WriteLine("Ağ dinleme özellikleri devre dışı kalabilir.");
            }
        }
    }
}
