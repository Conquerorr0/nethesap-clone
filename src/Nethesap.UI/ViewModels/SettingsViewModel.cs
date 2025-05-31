using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Nethesap.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _companyName;
        private string _email;
        private string _phone;
        private string _address;
        private string _taxNumber;
        private bool _darkThemeEnabled;
        private string _language;
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
                _darkThemeEnabled = value;
                OnPropertyChanged();
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged();
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

        public string[] AvailableLanguages => new[] { "Türkçe", "English" };
        public string[] AvailableCurrencies => new[] { "₺ (TL)", "$ (USD)", "€ (EUR)" };

        public ICommand SaveSettingsCommand { get; }
        public ICommand BrowseBackupLocationCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }

        public SettingsViewModel()
        {
            // Initialize with demo data
            CompanyName = "Demo Şirket";
            Email = "info@demo.com";
            Phone = "+90 555 123 4567";
            Address = "İstanbul, Türkiye";
            TaxNumber = "1234567890";
            DarkThemeEnabled = false;
            Language = "Türkçe";
            AutoBackupEnabled = true;
            BackupLocation = @"C:\Nethesap\Backups";
            Currency = "₺ (TL)";

            SaveSettingsCommand = new RelayCommand(SaveSettings);
            BrowseBackupLocationCommand = new RelayCommand(BrowseBackupLocation);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
        }

        private void SaveSettings(object parameter)
        {
            // Here we would implement the actual saving of settings
            // For now, we'll just simulate a save operation
            // TODO: Implement actual settings persistence
        }

        private void BrowseBackupLocation(object parameter)
        {
            // Here we would implement folder browser dialog
            // TODO: Implement folder browser dialog
        }

        private void ResetToDefaults(object parameter)
        {
            // Reset to default settings
            CompanyName = "Demo Şirket";
            Email = "info@demo.com";
            Phone = "+90 555 123 4567";
            Address = "İstanbul, Türkiye";
            TaxNumber = "1234567890";
            DarkThemeEnabled = false;
            Language = "Türkçe";
            AutoBackupEnabled = true;
            BackupLocation = @"C:\Nethesap\Backups";
            Currency = "₺ (TL)";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 