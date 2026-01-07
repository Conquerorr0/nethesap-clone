using System;
using System.Windows;
using System.Configuration;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Nethesap.UI.Services;
using Nethesap.Infrastructure.Data;
using Nethesap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nethesap.UI.ViewModels;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Nethesap.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Türk Lirası kültür ayarlarını yap
        var culture = new CultureInfo("tr-TR");
        culture.NumberFormat.CurrencySymbol = "TL";
        culture.NumberFormat.CurrencyPositivePattern = 3; // n TL formatı
        culture.NumberFormat.CurrencyNegativePattern = 8; // -n TL formatı
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Uygulama genelinde işlenmeyen hataları yakala
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            LogException("Unhandled AppDomain Exception", exception);
        };

        // UI thread'inde işlenmeyen hataları yakala
        this.DispatcherUnhandledException += (sender, args) =>
        {
            LogException("Unhandled UI Exception", args.Exception);
            args.Handled = true; // Uygulamanın çökmesini engelle
        };

        try
        {
            base.OnStartup(e);
            
            // Veritabanını başlat
            InitializeDatabase();

            // Ana pencereyi oluştur
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            LogException("Application Startup Error", ex);
            MessageBox.Show($"Uygulama başlatılırken hata oluştu: {ex.Message}\n\nLütfen uygulama geliştiricisine bu hatayı bildirin.",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Kritik bir hata olduğunda uygulamayı kapat
            this.Shutdown();
        }
    }
    
    private void LogException(string context, Exception ex)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder.AppendLine($"[{DateTime.Now}] {context}:");
        errorBuilder.AppendLine($"Message: {ex.Message}");
        
        if (ex.InnerException != null)
        {
            errorBuilder.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            errorBuilder.AppendLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
        }
        
        errorBuilder.AppendLine($"Stack Trace: {ex.StackTrace}");
        errorBuilder.AppendLine(new string('-', 80));
        
        // Konsola yazdır
        Console.WriteLine(errorBuilder.ToString());
        
        // Hata log dosyasına yaz
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
            File.AppendAllText(logPath, errorBuilder.ToString());
        }
        catch
        {
            // Log dosyasına yazma hatalarını yoksay
        }
    }
    
    private void InitializeDatabase()
    {
        try
        {
            Console.WriteLine("Veritabanı kontrolü yapılıyor...");
            
            // Veritabanı bağlantısını oluştur
            var dbContext = new AppDbContext();
            
            // Veritabanının var olup olmadığını kontrol et ve gerekirse oluştur
            bool created = dbContext.Database.EnsureCreated();
            
            if (created)
            {
                Console.WriteLine("Veritabanı başarıyla oluşturuldu.");
                SeedInitialData(dbContext);
            }
            else
            {
                Console.WriteLine("Veritabanı zaten mevcut, veriler korunuyor.");
            }
            
            // Veritabanı bağlantısını test et
            bool canConnect = dbContext.Database.CanConnect();
            if (canConnect)
            {
                Console.WriteLine("Veritabanı bağlantısı başarılı.");
                
                // Varlık sayılarını kontrol et
                int customerCount = dbContext.Customers.Count();
                int productCount = dbContext.Products.Count();
                int paymentCount = dbContext.Payments.Count();
                
                Console.WriteLine($"Mevcut veri: {customerCount} müşteri, {productCount} ürün, {paymentCount} satış kaydı");
            }
            else
            {
                Console.WriteLine("Veritabanı bağlantısı kurulamadı!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veritabanı oluşturulurken hata: {ex.Message}");
            Console.WriteLine($"Detaylar: {ex.InnerException?.Message}");
            
            // Hatayı göster ama uygulamanın çalışmasına izin ver
            MessageBox.Show($"Veritabanı oluşturulurken hata oluştu: {ex.Message}\n\nDetaylar: {ex.InnerException?.Message}",
                "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private void SeedInitialData(AppDbContext dbContext)
    {
        try
        {
            // Örnek test verileri kaldırıldı - SQLite veritabanı kullanılıyor
            Console.WriteLine("Veritabanı başarıyla oluşturuldu. Test verileri eklenmedi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veritabanı işlemi sırasında hata: {ex.Message}");
        }
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        // Otomatik yedekleme servisini durdur
        AutoBackupService.Instance.Stop();
        
        base.OnExit(e);
    }
}

