using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Nethesap.Domain.Entities;
using Nethesap.UI.Commands;
using Nethesap.UI.Services;

namespace Nethesap.UI.ViewModels
{
    public class CustomerDetailViewModel : INotifyPropertyChanged
    {
        private Customer _customer;
        private ObservableCollection<Transaction> _filteredTransactions;
        private string _searchText;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private ICommand _backCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public Customer Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged();
                FilterTransactions();
            }
        }

        public ObservableCollection<Transaction> FilteredTransactions
        {
            get => _filteredTransactions;
            set
            {
                _filteredTransactions = value;
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
                FilterTransactions();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                FilterTransactions();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                FilterTransactions();
            }
        }

        // Commands
        public ICommand BackCommand => _backCommand ??= new RelayCommand(GoBack);

        // Constructor
        public CustomerDetailViewModel(Customer customer)
        {
            Customer = customer;
            
            // Set initial date range to last 30 days
            EndDate = DateTime.Now;
            StartDate = DateTime.Now.AddDays(-30);
            
            FilterTransactions();
        }

        // Methods
        private void FilterTransactions()
        {
            if (Customer?.Transactions == null)
            {
                FilteredTransactions = new ObservableCollection<Transaction>();
                return;
            }

            IEnumerable<Transaction> filteredList = Customer.Transactions;

            // Filter by date range
            if (StartDate.HasValue)
            {
                DateTime start = StartDate.Value.Date;
                filteredList = filteredList.Where(t => t.TransactionDate.Date >= start);
            }

            if (EndDate.HasValue)
            {
                DateTime end = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                filteredList = filteredList.Where(t => t.TransactionDate <= end);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredList = filteredList.Where(t => 
                    t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    t.Amount.ToString().Contains(SearchText)
                );
            }

            // Order by date (newest first)
            filteredList = filteredList.OrderByDescending(t => t.TransactionDate);

            FilteredTransactions = new ObservableCollection<Transaction>(filteredList);
        }

        private void GoBack(object obj)
        {
            // Navigate back to customers view
            var mainViewModel = System.Windows.Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.NavigateToCustomers();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 