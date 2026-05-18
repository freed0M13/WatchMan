using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Watchman.Data
{
    public static class DatabaseInitializer
    {
        private const string DbName = "monitor.db";
        private const string DbPassword = "Watchman!9Qd7npDk";

        public static void Initialize(string sqlFileName)
        {
            try
            {
                SQLitePCL.Batteries_V2.Init();

                string basePath = AppContext.BaseDirectory;
                string dbPath = Path.Combine(basePath, DbName);
                string sqlScriptPath = Path.Combine(basePath, sqlFileName);

                // KRİTİK KONTROL: Eğer veritabanı dosyası zaten varsa adımları atla
                if (File.Exists(dbPath))
                {
                    Console.WriteLine("[Watchman DB] Veritabanı dosyası zaten mevcut. Yapılandırma korundu.");
                    return;
                }

                Console.WriteLine("[Watchman DB] Veritabanı bulunamadı. İlk kurulum başlatılıyor...");

                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = DbPassword
                }.ToString();

                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                if (File.Exists(sqlScriptPath))
                {
                    string script = File.ReadAllText(sqlScriptPath);
                    using var command = connection.CreateCommand();
                    command.CommandText = script;
                    command.ExecuteNonQuery();
                    Console.WriteLine("[Watchman DB] Temel şablon (base-db.sql) başarıyla uygulandı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KRİTİK HATA] Veritabanı başlatılamadı: {ex.Message}");
            }
        }
    }
}