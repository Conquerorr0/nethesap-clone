using System;
using System.Windows;
using System.Configuration;
using System.Data;
using Nethesap.UI.Services;

namespace Nethesap.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Uygulama ayarlarını yükle
        var settings = SettingsService.LoadSettings();
        
        try
        {
            // Otomatik yedekleme ayarlarını al
            bool autoBackupEnabled = false;
            string backupLocation = @"C:\Nethesap\Backups"; // Varsayılan konum
            
            if (settings.TryGetValue("AutoBackupEnabled", out var backupEnabledValue))
            {
                if (backupEnabledValue is System.Text.Json.JsonElement element && 
                    element.ValueKind == System.Text.Json.JsonValueKind.True)
                {
                    autoBackupEnabled = true;
                }
                else if (backupEnabledValue is bool boolValue)
                {
                    autoBackupEnabled = boolValue;
                }
            }
            
            if (settings.TryGetValue("BackupLocation", out var backupLocationValue))
            {
                if (backupLocationValue is System.Text.Json.JsonElement element && 
                    element.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    backupLocation = element.GetString() ?? backupLocation;
                }
                else if (backupLocationValue is string strValue)
                {
                    backupLocation = strValue;
                }
            }
            
            // Otomatik yedekleme servisini yapılandır
            AutoBackupService.Instance.Configure(autoBackupEnabled, backupLocation);
            
            Console.WriteLine($"Application started with auto backup {(autoBackupEnabled ? "enabled" : "disabled")}");
            Console.WriteLine($"Backup location: {backupLocation}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring auto backup: {ex.Message}");
        }
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        // Otomatik yedekleme servisini durdur
        AutoBackupService.Instance.Stop();
        
        base.OnExit(e);
    }
}

