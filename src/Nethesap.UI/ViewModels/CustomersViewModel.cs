using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using Nethesap.Domain.Entities;
using Nethesap.UI.Commands;
using Nethesap.UI.Services;

namespace Nethesap.UI.ViewModels
{
    public class CustomersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _filteredCustomers;
        private string _searchText;
        private bool _isAddDialogOpen;
        private Customer _newCustomer;
        private ICommand _addCustomerCommand;
        private ICommand _saveCustomerCommand;
        private ICommand _cancelAddCommand;
        private ICommand _showTransactionHistoryCommand;
        private bool _isLoading;
        private readonly CustomerService _customerService;

        public static ObservableCollection<Customer> GlobalCustomerList { get; private set; } = new ObservableCollection<Customer>();

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                GlobalCustomerList = value; // Static listeyi de güncelle
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set
            {
                _filteredCustomers = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }

        public bool IsAddDialogOpen
        {
            get => _isAddDialogOpen;
            set
            {
                _isAddDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public Customer NewCustomer
        {
            get => _newCustomer;
            set
            {
                _newCustomer = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand AddCustomerCommand => _addCustomerCommand ??= new RelayCommand(OpenAddCustomerDialog);
        public ICommand SaveCustomerCommand => _saveCustomerCommand ??= new RelayCommand(SaveCustomer);
        public ICommand CancelAddCommand => _cancelAddCommand ??= new RelayCommand(CancelAdd);
        public ICommand ShowTransactionHistoryCommand => _showTransactionHistoryCommand ??= new RelayCommand<Customer>(ShowTransactionHistory);

        // Constructor
        public CustomersViewModel()
        {
            _customerService = new CustomerService();
            NewCustomer = new Customer();
            LoadCustomers();
        }

        // Filter customers based on search text
        private async void FilterCustomers()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    FilteredCustomers = new ObservableCollection<Customer>(Customers);
                }
                else
                {
                    // Veritabanından arama yap
                    var customers = await _customerService.SearchCustomersAsync(SearchText);
                    FilteredCustomers = customers;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri filtreleme hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadCustomers()
        {
            try
            {
                IsLoading = true;
                Customers = await _customerService.GetAllCustomersAsync();
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
                GlobalCustomerList = Customers; // Static listeyi de güncelle
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşterileri yükleme hatası: {ex.Message}");
                Customers = new ObservableCollection<Customer>();
                FilteredCustomers = new ObservableCollection<Customer>();
                GlobalCustomerList = Customers; // Static listeyi de güncelle
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenAddCustomerDialog(object obj)
        {
            NewCustomer = new Customer();
            IsAddDialogOpen = true;
        }

        private async void SaveCustomer(object obj)
        {
            try
            {
                IsLoading = true;

                if (NewCustomer == null) return;

                // Müşteriyi veritabanına ekle
                bool success = await _customerService.AddCustomerAsync(NewCustomer);

                if (success)
                {
                    // UI'ı güncelle
                    Customers.Add(NewCustomer);
                    FilteredCustomers.Add(NewCustomer);
                    GlobalCustomerList.Add(NewCustomer); // Static listeyi de güncelle
                    IsAddDialogOpen = false;
                    NewCustomer = new Customer();
                }
                else
                {
                    // Hata durumunu kullanıcıya bildir
                    Console.WriteLine("Müşteri eklenemedi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri kaydetme hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelAdd(object obj)
        {
            IsAddDialogOpen = false;
            NewCustomer = new Customer();
        }

        private void ShowTransactionHistory(Customer customer)
        {
            if (customer == null) return;

            try
            {
                // Ana view model üzerinden müşteri detay sayfasına git
                var mainViewModel = System.Windows.Application.Current.MainWindow.DataContext as MainViewModel;
                if (mainViewModel == null)
                {
                    Console.WriteLine("MainViewModel bulunamadı, müşteri detayına gidilemedi.");
                    return;
                }

                // Müşteriyi işlem geçmişi ile birlikte yeniden yükle
                using (var context = new Nethesap.Infrastructure.Data.AppDbContext())
                {
                    var customerRepository = new Nethesap.Infrastructure.RepositoryImplementations.CustomerRepository(context);
                    var customerWithTransactions = customerRepository.GetCustomerWithTransactionsAsync(customer.Id).GetAwaiter().GetResult();

                    if (customerWithTransactions == null)
                    {
                        Console.WriteLine($"ID'si {customer.Id} olan müşteri veritabanında bulunamadı, mevcut nesne ile devam ediliyor.");
                        customerWithTransactions = customer;
                    }

                    mainViewModel.NavigateToCustomerDetail(customerWithTransactions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İşlem geçmişi gösterilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                MessageBox.Show($"Müşteri işlem geçmişi gösterilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 