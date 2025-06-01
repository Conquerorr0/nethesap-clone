using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Nethesap.UI.Services
{
    public class BackupService
    {
        private static string DatabaseFolderPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Nethesap",
            "Database");
            
        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Nethesap",
            "settings.json");
            
        /// <summary>
        /// Veritabanı ve uygulama ayarlarını belirlenen dizine yedekler
        /// </summary>
        /// <param name="backupPath">Yedekleme dizini yolu</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        public static async Task<bool> BackupDataAsync(string backupPath)
        {
            try
            {
                // Dizin kontrolü
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                
                // Yedek dosya adı: Nethesap_Backup_YYYY-MM-DD_HH-mm-ss.zip
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFileName = $"Nethesap_Backup_{timestamp}.zip";
                string backupFilePath = Path.Combine(backupPath, backupFileName);
                
                // Geçici dizin oluştur
                string tempDir = Path.Combine(Path.GetTempPath(), "NethesapBackup_" + timestamp);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);
                
                // Veritabanı ve ayarları geçici dizine kopyala
                string tempDbFolder = Path.Combine(tempDir, "Database");
                Directory.CreateDirectory(tempDbFolder);
                
                // Veritabanı dosyalarını kopyala
                if (Directory.Exists(DatabaseFolderPath))
                {
                    foreach (string file in Directory.GetFiles(DatabaseFolderPath))
                    {
                        string destFile = Path.Combine(tempDbFolder, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                }
                
                // Ayarlar dosyasını kopyala
                if (File.Exists(SettingsFilePath))
                {
                    string settingsFileName = Path.GetFileName(SettingsFilePath);
                    File.Copy(SettingsFilePath, Path.Combine(tempDir, settingsFileName), true);
                }
                
                // Geçici dizini sıkıştır
                await Task.Run(() => ZipFile.CreateFromDirectory(tempDir, backupFilePath));
                
                // Geçici dizini temizle
                Directory.Delete(tempDir, true);
                
                Console.WriteLine($"Backup completed successfully: {backupFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backup failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Yedekten geri yükleme işlemi
        /// </summary>
        /// <param name="backupFilePath">Yedek dosya yolu</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        public static async Task<bool> RestoreDataAsync(string backupFilePath)
        {
            try
            {
                // Dosya kontrolü
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("Backup file not found", backupFilePath);
                }
                
                // Geçici dizin oluştur
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string tempDir = Path.Combine(Path.GetTempPath(), "NethesapRestore_" + timestamp);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);
                
                // Zip dosyasını geçici dizine çıkart
                await Task.Run(() => ZipFile.ExtractToDirectory(backupFilePath, tempDir));
                
                // Hedef dizinleri oluştur
                string appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Nethesap");
                    
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }
                
                // Veritabanı dosyalarını geri yükle
                string tempDbFolder = Path.Combine(tempDir, "Database");
                if (Directory.Exists(tempDbFolder))
                {
                    // Hedef veritabanı klasörünü oluştur
                    if (!Directory.Exists(DatabaseFolderPath))
                    {
                        Directory.CreateDirectory(DatabaseFolderPath);
                    }
                    
                    foreach (string file in Directory.GetFiles(tempDbFolder))
                    {
                        string destFile = Path.Combine(DatabaseFolderPath, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                }
                
                // Ayarlar dosyasını geri yükle
                string settingsFileName = Path.GetFileName(SettingsFilePath);
                string tempSettingsPath = Path.Combine(tempDir, settingsFileName);
                if (File.Exists(tempSettingsPath))
                {
                    File.Copy(tempSettingsPath, SettingsFilePath, true);
                }
                
                // Geçici dizini temizle
                Directory.Delete(tempDir, true);
                
                Console.WriteLine($"Restore completed successfully from: {backupFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Restore failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Belirli bir konumdaki tüm yedek dosyalarını listeler
        /// </summary>
        /// <param name="backupPath">Yedeklerin bulunduğu dizin</param>
        /// <returns>Yedek dosyalarının tam yollarını içeren liste</returns>
        public static List<string> GetBackupFiles(string backupPath)
        {
            List<string> backupFiles = new List<string>();
            
            try
            {
                if (Directory.Exists(backupPath))
                {
                    string[] files = Directory.GetFiles(backupPath, "Nethesap_Backup_*.zip");
                    backupFiles.AddRange(files);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing backup files: {ex.Message}");
            }
            
            return backupFiles;
        }
    }
} 