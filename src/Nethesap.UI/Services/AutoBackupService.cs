using System;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Linq;

namespace Nethesap.UI.Services
{
    public class AutoBackupService
    {
        private static AutoBackupService _instance;
        private System.Timers.Timer _backupTimer;
        private bool _isEnabled;
        private string _backupLocation;
        private const int BACKUP_INTERVAL_HOURS = 24; // Varsayılan olarak günlük yedekleme
        
        // Singleton pattern - tek bir örnek oluştur
        public static AutoBackupService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AutoBackupService();
                }
                return _instance;
            }
        }
        
        private AutoBackupService()
        {
            // Timer oluştur ama başlatma
            _backupTimer = new System.Timers.Timer();
            _backupTimer.Elapsed += OnBackupTimerElapsed;
            _backupTimer.Interval = TimeSpan.FromHours(BACKUP_INTERVAL_HOURS).TotalMilliseconds;
            _backupTimer.AutoReset = true;
        }
        
        /// <summary>
        /// Otomatik yedekleme servisini başlatır
        /// </summary>
        /// <param name="isEnabled">Servis etkin mi?</param>
        /// <param name="backupLocation">Yedekleme konumu</param>
        public void Configure(bool isEnabled, string backupLocation)
        {
            _isEnabled = isEnabled;
            _backupLocation = backupLocation;
            
            // Timer'ı durdur
            _backupTimer.Stop();
            
            // Eğer etkinse, başlat
            if (_isEnabled && !string.IsNullOrEmpty(_backupLocation))
            {
                Console.WriteLine("Auto backup service configured and enabled.");
                Console.WriteLine($"Backup location: {_backupLocation}");
                Console.WriteLine($"Backup interval: {BACKUP_INTERVAL_HOURS} hours");
                
                // Yedekleme konumunu kontrol et
                EnsureBackupLocationExists();
                
                // Başlangıçta bir yedek al ve timer'ı başlat
                Task.Run(async () => await PerformBackupAsync());
                _backupTimer.Start();
            }
            else
            {
                Console.WriteLine("Auto backup service is disabled.");
            }
        }
        
        /// <summary>
        /// Zamanlanmış yedekleme işlemi
        /// </summary>
        private async void OnBackupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isEnabled && !string.IsNullOrEmpty(_backupLocation))
            {
                Console.WriteLine("Scheduled backup started...");
                await PerformBackupAsync();
            }
        }
        
        /// <summary>
        /// Yedekleme işlemini gerçekleştirir
        /// </summary>
        private async Task PerformBackupAsync()
        {
            try
            {
                if (!_isEnabled || string.IsNullOrEmpty(_backupLocation))
                {
                    return;
                }
                
                // Yedekleme konumunu kontrol et
                EnsureBackupLocationExists();
                
                // Yedekleme işlemini gerçekleştir
                bool success = await BackupService.BackupDataAsync(_backupLocation);
                
                if (success)
                {
                    Console.WriteLine("Automatic backup completed successfully.");
                    
                    // Eski yedekleri temizle (isteğe bağlı)
                    CleanupOldBackups();
                }
                else
                {
                    Console.WriteLine("Automatic backup failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in automatic backup: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Yedekleme konumunun var olduğundan emin olur
        /// </summary>
        private void EnsureBackupLocationExists()
        {
            if (!string.IsNullOrEmpty(_backupLocation) && !Directory.Exists(_backupLocation))
            {
                try
                {
                    Directory.CreateDirectory(_backupLocation);
                    Console.WriteLine($"Created backup directory: {_backupLocation}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create backup directory: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Belirli bir süre öncesine ait yedekleri temizler
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                // Son 10 yedeği tut, gerisini sil
                var backupFiles = BackupService.GetBackupFiles(_backupLocation)
                    .OrderByDescending(f => new FileInfo(f).CreationTime)
                    .ToList();
                
                const int MAX_BACKUP_FILES = 10;
                
                if (backupFiles.Count > MAX_BACKUP_FILES)
                {
                    // Fazla olan dosyaları sil
                    foreach (string file in backupFiles.Skip(MAX_BACKUP_FILES))
                    {
                        try
                        {
                            File.Delete(file);
                            Console.WriteLine($"Deleted old backup: {file}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete old backup {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up old backups: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Servis durdurulduğunda zamanlamayı durdur
        /// </summary>
        public void Stop()
        {
            _backupTimer.Stop();
            Console.WriteLine("Auto backup service stopped.");
        }
    }
} 