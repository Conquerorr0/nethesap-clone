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
        private ICommand _viewSaleDetailsCommand;
        private ICommand _closeSaleDetailsCommand;
        private decimal _currentBalance;
        private Payment _selectedSale;
        private bool _isSaleDetailsDialogOpen;
        private SaleService _saleService;

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

        /// <summary>
        /// İlgili müşterinin tüm işlemlerine göre hesaplanan güncel bakiye (borç/alacak)
        /// </summary>
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set
            {
                _currentBalance = value;
                OnPropertyChanged();
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

        public Payment SelectedSale
        {
            get => _selectedSale;
            set
            {
                _selectedSale = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaleDetailsDialogOpen
        {
            get => _isSaleDetailsDialogOpen;
            set
            {
                _isSaleDetailsDialogOpen = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand BackCommand => _backCommand ??= new RelayCommand(GoBack);
        public ICommand ViewSaleDetailsCommand => _viewSaleDetailsCommand ??= new RelayCommand<Transaction>(ViewSaleDetails);
        public ICommand CloseSaleDetailsCommand => _closeSaleDetailsCommand ??= new RelayCommand(CloseSaleDetails);

        // Constructor
        public CustomerDetailViewModel(Customer customer)
        {
            _saleService = new SaleService();
            Customer = customer;
            
            // Tüm işlemleri göstermek için tarih filtresini başlangıçta null yap
            // Kullanıcı isterse tarih aralığı seçebilir
            EndDate = null;
            StartDate = null;
            
            FilterTransactions();
        }

        // Methods
        private void FilterTransactions()
        {
            if (Customer == null)
            {
                FilteredTransactions = new ObservableCollection<Transaction>();

                // Hiç işlem yoksa bakiye 0 kabul et
                CurrentBalance = 0;
                return;
            }

            IEnumerable<Transaction> filteredList = Customer.Transactions ?? Enumerable.Empty<Transaction>();

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

            // Genel bakiye: ilgili müşterinin tüm satışlarındaki kalan borç toplamı
            try
            {
                if (Customer.Payments != null && Customer.Payments.Any())
                {
                    // Her satışın RemainingAmount alanını toplayarak güncel borcu hesapla
                    CurrentBalance = Customer.Payments.Sum(p => p.RemainingAmount);
                }
                else
                {
                    CurrentBalance = 0;
                }
            }
            catch
            {
                // Her ihtimale karşı hata durumunda Customer.Balance'a geri düş
                CurrentBalance = Customer?.Balance ?? 0;
            }
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

        private async void ViewSaleDetails(Transaction transaction)
        {
            if (transaction == null || !transaction.PaymentId.HasValue) return;

            try
            {
                // Satış detaylarını getir
                SelectedSale = await _saleService.GetSaleDetailsAsync(transaction.PaymentId.Value);
                
                if (SelectedSale != null)
                {
                    IsSaleDetailsDialogOpen = true;
                }
                else
                {
                    MessageBox.Show("Satış detayları bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satış detayları getirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseSaleDetails(object obj)
        {
            IsSaleDetailsDialogOpen = false;
            SelectedSale = null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 