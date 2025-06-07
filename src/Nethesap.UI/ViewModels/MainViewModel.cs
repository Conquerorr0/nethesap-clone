using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Nethesap.Domain.Entities;
using Nethesap.UI.Views;
using MaterialDesignThemes.Wpf;
using Nethesap.UI.Localization;
using Nethesap.UI.Services;

namespace Nethesap.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private System.Windows.Controls.UserControl _currentView;
        private string _currentViewTitle;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public System.Windows.Controls.UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set
            {
                _currentViewTitle = value;
                OnPropertyChanged();
            }
        }

        // Constructor - public yapıcı metot
        public MainViewModel()
        {
            try
            {
                // Ayarları yükle
                var settings = SettingsService.LoadSettings();
                
                // Tema ayarını yükle
                bool isDarkTheme = false;
                if (settings.TryGetValue("DarkThemeEnabled", out var themeValue))
                {
                    if (themeValue is System.Text.Json.JsonElement element && element.ValueKind == System.Text.Json.JsonValueKind.True)
                    {
                        isDarkTheme = true;
                    }
                    else if (themeValue is bool boolValue)
                    {
                        isDarkTheme = boolValue;
                    }
                }
                
                Console.WriteLine($"Loaded theme: {(isDarkTheme ? "Dark" : "Light")}");
                
                // Uygulama başlatıldığında tema ayarını uygula
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();
                
                theme.SetBaseTheme(isDarkTheme ? 
                    Theme.Dark : 
                    Theme.Light);
                    
                paletteHelper.SetTheme(theme);
                
                // Statik değişkenleri güncelle
                SettingsViewModel.SetTheme(isDarkTheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            // Default view is Dashboard
            NavigateToDashboard();
        }

        // Navigation methods
        public void NavigateToDashboard()
        {
            CurrentView = new DashboardView { DataContext = new DashboardViewModel() };
            CurrentViewTitle = "Gösterge Paneli";
        }

        public void NavigateToCustomers()
        {
            CurrentView = new CustomersView { DataContext = new CustomersViewModel() };
            CurrentViewTitle = "Müşteriler";
        }

        public void NavigateToCustomerDetail(Customer customer)
        {
            CurrentView = new CustomerDetailView { DataContext = new CustomerDetailViewModel(customer) };
            CurrentViewTitle = $"Müşteri: {customer.FirstName} {customer.LastName}";
        }

        public void NavigateToProducts()
        {
            CurrentView = new ProductsView { DataContext = new ProductsViewModel() };
            CurrentViewTitle = "Ürünler";
        }

        public void NavigateToSales()
        {
            CurrentView = new SalesView { DataContext = new SalesViewModel() };
            CurrentViewTitle = "Satışlar";
        }

        public void NavigateToAccounts()
        {
            CurrentView = new AccountsView { DataContext = new AccountsViewModel() };
            CurrentViewTitle = "Hesaplar";
        }

        public void NavigateToSettings()
        {
            var settingsViewModel = new SettingsViewModel();
            
            // Mevcut tema ayarını yükle
            settingsViewModel.DarkThemeEnabled = SettingsViewModel.GetCurrentTheme();
            
            // Geri dönüş olayını dinle
            settingsViewModel.GoBack += () => NavigateToDashboard();
            
            CurrentView = new SettingsView { DataContext = settingsViewModel };
            CurrentViewTitle = "Ayarlar";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 