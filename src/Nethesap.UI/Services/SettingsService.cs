using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Nethesap.UI.Services
{
    public class SettingsService
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Nethesap",
            "settings.json");

        public static Dictionary<string, object> LoadSettings()
        {
            try
            {
                // Eğer dosya yoksa varsayılan ayarları döndür
                if (!File.Exists(SettingsFilePath))
                {
                    Console.WriteLine($"Settings file not found at {SettingsFilePath}, using defaults.");
                    return GetDefaultSettings();
                }

                // Dosyadan ayarları oku
                string json = File.ReadAllText(SettingsFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Settings file is empty, using defaults.");
                    return GetDefaultSettings();
                }
                
                Console.WriteLine($"Loaded settings from {SettingsFilePath}");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                
                if (settings == null || settings.Count == 0)
                {
                    Console.WriteLine("Failed to deserialize settings or settings are empty, using defaults.");
                    return GetDefaultSettings();
                }
                
                // Debug: Yüklenen ayarları konsola yaz
                Console.WriteLine("Loaded settings:");
                foreach (var kvp in settings)
                {
                    Console.WriteLine($"- {kvp.Key}: {kvp.Value}");
                }
                
                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Settings could not be loaded: {ex.Message}");
                return GetDefaultSettings();
            }
        }

        public static void SaveSettings(Dictionary<string, object> settings)
        {
            try
            {
                // Dizin yoksa oluştur
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Ayarları JSON olarak kaydet
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(settings, options);
                
                // Debug: Kaydedilen JSON'ı konsola yaz
                Console.WriteLine($"Saving settings to {SettingsFilePath}");
                Console.WriteLine($"JSON: {json}");
                
                File.WriteAllText(SettingsFilePath, json);
                Console.WriteLine("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Settings could not be saved: {ex.Message}");
            }
        }

        private static Dictionary<string, object> GetDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                { "DarkThemeEnabled", false },
                { "CompanyName", "Demo Şirket" },
                { "Email", "info@demo.com" },
                { "Phone", "+90 555 123 4567" },
                { "Address", "İstanbul, Türkiye" },
                { "TaxNumber", "1234567890" },
                { "AutoBackupEnabled", true },
                { "BackupLocation", @"C:\Nethesap\Backups" },
                { "Currency", "₺ (TL)" }
            };
        }
    }
} 