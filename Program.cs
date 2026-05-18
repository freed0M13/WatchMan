using Microsoft.Extensions.Hosting.WindowsServices;
using Watchman;
using Watchman.Services;

// Servis olarak çalıştığında kök dizini doğru bulması için ayar
var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

// 1. Windows Service Entegrasyonu
builder.Host.UseWindowsService(serviceOptions =>
{
    serviceOptions.ServiceName = "WatchmanSecurityService";
});

// 2. Servisleri (DI Container) Kaydetme
builder.Services.AddControllers();
// Veri köprüsünü singleton olarak kaydediyoruz
builder.Services.AddSingleton<SecurityStateStore>();
builder.Services.AddHostedService<Watchman.Workers.TrafficAnalyzerWorker>();

// Svelte ile geliştirme aşamasında CORS sorunları yaşamamak için (Canlıda kapatılabilir)
builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddPolicy("AllowDashboard", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// İleride buraya SQLite DbContext ve arka plan (Worker) servislerimizi ekleyeceğiz
// builder.Services.AddDbContext<WatchmanDbContext>(...);
// builder.Services.AddHostedService<SystemAnalyzerWorker>();

var app = builder.Build();

// 3. HTTP İstek Hattı (Pipeline) Yapılandırması
app.UseCors("AllowDashboard");

// Svelte'in derlenmiş dosyalarını (wwwroot içindeki) statik olarak sunmak için
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

// 1. Ağ izleme sürücüsünü kontrol et ve gerekirse kur
Watchman.Driver.NpcapInstaller.EnsureInstalled();

// 2. Veritabanını şifreli olarak başlat ve tabloları oluştur
Watchman.Data.DatabaseInitializer.Initialize("base-db.sql");

// Servisler ve Kestrel başlatılıyor...
app.Run();