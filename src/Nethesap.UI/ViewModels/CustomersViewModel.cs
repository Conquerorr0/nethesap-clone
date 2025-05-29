using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Nethesap.Domain.Entities;

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

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
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

        // Commands
        public ICommand AddCustomerCommand => _addCustomerCommand ??= new RelayCommand(OpenAddCustomerDialog);
        public ICommand SaveCustomerCommand => _saveCustomerCommand ??= new RelayCommand(SaveCustomer);
        public ICommand CancelAddCommand => _cancelAddCommand ??= new RelayCommand(CancelAdd);
        public ICommand ShowTransactionHistoryCommand => _showTransactionHistoryCommand ??= new RelayCommand<Customer>(ShowTransactionHistory);

        // Constructor
        public CustomersViewModel()
        {
            LoadSampleData();
            NewCustomer = new Customer();
        }

        // Filter customers based on search text
        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
                return;
            }

            FilteredCustomers = new ObservableCollection<Customer>(
                Customers.Where(c => 
                    c.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && 
                        c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                )
            );
        }

        private void LoadSampleData()
        {
            // In a real app, this would come from a repository or service
            Customers = new ObservableCollection<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Ahmet",
                    LastName = "Yılmaz",
                    Phone = "(555) 123-4567",
                    Email = "ahmet.yilmaz@email.com",
                    Address = "Ankara, Çankaya",
                    Balance = 1500.00m,
                    Transactions = new Collection<Transaction>
                    {
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 500.00m, 
                            Type = TransactionType.Debt, 
                            Description = "Ürün satışı",
                            TransactionDate = DateTime.Now.AddDays(-5)
                        },
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 1000.00m, 
                            Type = TransactionType.Debt, 
                            Description = "Hizmet bedeli",
                            TransactionDate = DateTime.Now.AddDays(-10)
                        }
                    }
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Mehmet",
                    LastName = "Kaya",
                    Phone = "(532) 234-5678",
                    Email = "mehmet.kaya@email.com",
                    Address = "İstanbul, Kadıköy",
                    Balance = -750.50m,
                    Transactions = new Collection<Transaction>
                    {
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 1250.00m, 
                            Type = TransactionType.Credit, 
                            Description = "Avans ödemesi",
                            TransactionDate = DateTime.Now.AddDays(-2)
                        },
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 500.00m, 
                            Type = TransactionType.Debt, 
                            Description = "Malzeme temini",
                            TransactionDate = DateTime.Now.AddDays(-15)
                        }
                    }
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Ayşe",
                    LastName = "Demir",
                    Phone = "(505) 345-6789",
                    Email = "ayse.demir@email.com",
                    Address = "İzmir, Karşıyaka",
                    Balance = 2800.25m,
                    Transactions = new Collection<Transaction>
                    {
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 1500.00m, 
                            Type = TransactionType.Debt, 
                            Description = "Yazılım hizmeti",
                            TransactionDate = DateTime.Now.AddDays(-7)
                        },
                        new Transaction 
                        { 
                            Id = Guid.NewGuid(),
                            Amount = 1300.25m, 
                            Type = TransactionType.Debt, 
                            Description = "Danışmanlık ücreti",
                            TransactionDate = DateTime.Now.AddDays(-20)
                        }
                    }
                }
            };

            FilteredCustomers = new ObservableCollection<Customer>(Customers);
        }

        private void OpenAddCustomerDialog(object obj)
        {
            NewCustomer = new Customer();
            IsAddDialogOpen = true;
        }

        private void SaveCustomer(object obj)
        {
            // In a real app, this would save to a database
            NewCustomer.Id = Guid.NewGuid();
            NewCustomer.Transactions = new Collection<Transaction>();
            
            Customers.Add(NewCustomer);
            FilterCustomers();
            
            IsAddDialogOpen = false;
            NewCustomer = new Customer();
        }

        private void CancelAdd(object obj)
        {
            IsAddDialogOpen = false;
            NewCustomer = new Customer();
        }

        private void ShowTransactionHistory(Customer customer)
        {
            // Navigate to transaction history view
            var mainViewModel = System.Windows.Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.NavigateToCustomerDetail(customer);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 