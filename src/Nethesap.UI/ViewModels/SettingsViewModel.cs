using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using Nethesap.UI.Localization;
using Nethesap.UI.Services;
using System.Windows;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nethesap.UI.Commands;
using Microsoft.Win32;

namespace Nethesap.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _companyName;
        private string _email;
        private string _phone;
        private string _address;
        private string _taxNumber;
        private static bool _darkThemeEnabled;
        private bool _autoBackupEnabled;
        private string _backupLocation;
        private string _currency;

        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        public string TaxNumber
        {
            get => _taxNumber;
            set
            {
                _taxNumber = value;
                OnPropertyChanged();
            }
        }

        public bool DarkThemeEnabled
        {
            get => _darkThemeEnabled;
            set
            {
                if (_darkThemeEnabled != value)
                {
                    _darkThemeEnabled = value;
                    OnPropertyChanged();
                    
                    // Tema değişikliğini hemen uygula
                    ApplyThemeChange(_darkThemeEnabled);
                }
            }
        }

        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set
            {
                _autoBackupEnabled = value;
                OnPropertyChanged();
            }
        }

        public string BackupLocation
        {
            get => _backupLocation;
            set
            {
                _backupLocation = value;
                OnPropertyChanged();
            }
        }

        public string Currency
        {
            get => _currency;
            set
            {
                _currency = value;
                OnPropertyChanged();
            }
        }

        public string[] AvailableCurrencies => new[] { "₺ (TL)", "$ (USD)", "€ (EUR)" };

        public ICommand SaveSettingsCommand { get; }
        public ICommand BrowseBackupLocationCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand BackupNowCommand { get; }

        // Geri dönüş için olay
        public event Action GoBack;

        public SettingsViewModel()
        {
            // Load settings from saved file
            var settings = SettingsService.LoadSettings();
            
            // Initialize with saved data or defaults
            CompanyName = GetSettingValue<string>(settings, "CompanyName", "Demo Şirket");
            Email = GetSettingValue<string>(settings, "Email", "info@demo.com");
            Phone = GetSettingValue<string>(settings, "Phone", "+90 555 123 4567");
            Address = GetSettingValue<string>(settings, "Address", "İstanbul, Türkiye");
            TaxNumber = GetSettingValue<string>(settings, "TaxNumber", "1234567890");
            
            // Load static settings and update the static variables
            _darkThemeEnabled = GetSettingValue<bool>(settings, "DarkThemeEnabled", false);
            
            // Set property values (this will also apply theme)
            DarkThemeEnabled = _darkThemeEnabled;
            
            AutoBackupEnabled = GetSettingValue<bool>(settings, "AutoBackupEnabled", true);
            BackupLocation = GetSettingValue<string>(settings, "BackupLocation", @"C:\Nethesap\Backups");
            Currency = GetSettingValue<string>(settings, "Currency", "₺ (TL)");

            SaveSettingsCommand = new RelayCommand(SaveSettings);
            BrowseBackupLocationCommand = new RelayCommand(BrowseBackupLocation);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            BackCommand = new RelayCommand(NavigateBack);
            BackupNowCommand = new RelayCommand(BackupNow);
        }

        private T GetSettingValue<T>(Dictionary<string, object> settings, string key, T defaultValue)
        {
            if (settings.TryGetValue(key, out var value))
            {
                try
                {
                    // JSON deserialization handling
                    if (value is System.Text.Json.JsonElement element)
                    {
                        if (typeof(T) == typeof(bool))
                        {
                            if (element.ValueKind == System.Text.Json.JsonValueKind.True)
                                return (T)(object)true;
                            if (element.ValueKind == System.Text.Json.JsonValueKind.False)
                                return (T)(object)false;
                        }
                        else if (typeof(T) == typeof(string) && element.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            return (T)(object)element.GetString();
                        }
                        else if (typeof(T) == typeof(int) && element.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            return (T)(object)element.GetInt32();
                        }
                        
                        // Diğer tipleri String olarak ele alıp dönüşüm deneyelim
                        return (T)Convert.ChangeType(element.ToString(), typeof(T));
                    }
                    
                    // Doğrudan dönüştürme dene
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    
                    // Tip dönüşümü dene
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Setting conversion error for key {key}: {ex.Message}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private void SaveSettings(object parameter)
        {
            try
            {
                // Tema değişikliğini uygula
                ApplyThemeChange(DarkThemeEnabled);
                
                // Ayarları kalıcı olarak kaydet
                var settings = new Dictionary<string, object>
                {
                    { "DarkThemeEnabled", DarkThemeEnabled },
                    { "CompanyName", CompanyName },
                    { "Email", Email },
                    { "Phone", Phone },
                    { "Address", Address },
                    { "TaxNumber", TaxNumber },
                    { "AutoBackupEnabled", AutoBackupEnabled },
                    { "BackupLocation", BackupLocation },
                    { "Currency", Currency }
                };
                
                SettingsService.SaveSettings(settings);
                
                // Otomatik yedekleme servisini yapılandır
                AutoBackupService.Instance.Configure(AutoBackupEnabled, BackupLocation);
                
                Console.WriteLine("Settings saved successfully:");
                Console.WriteLine($"- DarkThemeEnabled: {DarkThemeEnabled}");
                Console.WriteLine($"- AutoBackupEnabled: {AutoBackupEnabled}");
                Console.WriteLine($"- BackupLocation: {BackupLocation}");
                
                // Kullanıcıya başarılı mesajı göster
                MessageBox.Show(
                    "Ayarlar başarıyla kaydedildi.",
                    "Bilgi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                
                // Hata mesajı göster
                MessageBox.Show(
                    $"Ayarlar kaydedilirken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        
        private void ApplyThemeChange(bool isDarkTheme)
        {
            try
            {
                // Tema değişikliğini uygula
                var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
                var theme = paletteHelper.GetTheme();
                
                theme.SetBaseTheme(isDarkTheme ? 
                    MaterialDesignThemes.Wpf.Theme.Dark : 
                    MaterialDesignThemes.Wpf.Theme.Light);
                    
                paletteHelper.SetTheme(theme);
                
                // Static değişkeni güncelle
                _darkThemeEnabled = isDarkTheme;
                
                // Tema değişikliği hakkında bilgi
                Console.WriteLine($"Tema {(isDarkTheme ? "koyu" : "açık")} olarak değiştirildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema değişikliği uygulanırken hata: {ex.Message}");
            }
        }

        private void BrowseBackupLocation(object parameter)
        {
            try
            {
                // WindowsAPICodePack kullanarak klasör seçici diyalog oluştur
                var dialog = new CommonOpenFileDialog
                {
                    Title = "Yedekleme dizinini seçin",
                    IsFolderPicker = true,
                    InitialDirectory = !string.IsNullOrEmpty(BackupLocation) && Directory.Exists(BackupLocation) 
                        ? BackupLocation 
                        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    AddToMostRecentlyUsedList = false,
                    AllowNonFileSystemItems = false,
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    EnsureFileExists = true,
                    EnsurePathExists = true,
                    EnsureReadOnly = false,
                    EnsureValidNames = true,
                    Multiselect = false,
                    ShowPlacesList = true
                };

                // Diyaloğu göster
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    BackupLocation = dialog.FileName;
                    Console.WriteLine($"Backup location set to: {BackupLocation}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in folder selection: {ex.Message}");
                MessageBox.Show($"Klasör seçiminde hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults(object parameter)
        {
            // Reset to default settings
            CompanyName = "Demo Şirket";
            Email = "info@demo.com";
            Phone = "+90 555 123 4567";
            Address = "İstanbul, Türkiye";
            TaxNumber = "1234567890";
            
            // Statik değişkenleri doğrudan değiştir
            _darkThemeEnabled = false; // Açık tema varsayılan
            
            // Değerleri UI'a bildir
            DarkThemeEnabled = _darkThemeEnabled;
            
            AutoBackupEnabled = true;
            BackupLocation = @"C:\Nethesap\Backups";
            Currency = "₺ (TL)";
        }

        private void NavigateBack(object parameter)
        {
            // Geri dönüş olayını tetikle
            GoBack?.Invoke();
        }

        private async void BackupNow(object parameter)
        {
            try
            {
                // Yedekleme konumunu kontrol et
                if (string.IsNullOrEmpty(BackupLocation))
                {
                    MessageBox.Show(
                        "Lütfen önce yedekleme konumunu belirleyin.",
                        "Uyarı",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
                
                // Yedekleme işlemi başlıyor mesajı
                MessageBox.Show(
                    "Yedekleme işlemi başlatılıyor...",
                    "Bilgi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                
                // Yedekleme işlemini başlat
                bool success = await BackupService.BackupDataAsync(BackupLocation);
                
                if (success)
                {
                    MessageBox.Show(
                        $"Yedekleme işlemi başarıyla tamamlandı.\nKonum: {BackupLocation}",
                        "Başarılı",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Yedekleme işlemi başarısız oldu.",
                        "Hata",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in manual backup: {ex.Message}");
                MessageBox.Show(
                    $"Yedekleme sırasında hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // Tema ayarını almak için statik yöntem
        public static bool GetCurrentTheme()
        {
            return _darkThemeEnabled;
        }
        
        // Tema ayarını değiştirmek için statik yöntem
        public static void SetTheme(bool darkThemeEnabled)
        {
            _darkThemeEnabled = darkThemeEnabled;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 