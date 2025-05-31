using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Nethesap.Domain.Entities;
using Nethesap.UI.Views;

namespace Nethesap.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;
        private string _currentViewTitle;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public UserControl CurrentView
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
            CurrentView = new SalesView();
            CurrentViewTitle = "Satışlar";
        }

        public void NavigateToAccounts()
        {
            CurrentView = new AccountsView();
            CurrentViewTitle = "Hesaplar";
        }

        public void NavigateToSettings()
        {
            CurrentView = new SettingsView { DataContext = new SettingsViewModel() };
            CurrentViewTitle = "Ayarlar";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 