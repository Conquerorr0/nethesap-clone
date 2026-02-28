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
        private ObservableCollection<Payment> _filteredSales; // Changed from Transactions to Sales (Payments)
        private string _searchText;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private ICommand _backCommand;
        
        // Commands matching SalesViewModel
        private ICommand _viewSaleDetailsCommand;
        private ICommand _closeSaleDetailsCommand;
        private ICommand _openPartialPaymentDialogCommand;
        private ICommand _makePartialPaymentCommand;
        private ICommand _cancelPartialPaymentCommand;
        private ICommand _viewPaymentHistoryCommand;
        private ICommand _closePaymentHistoryCommand;
        private ICommand _refundSaleCommand;

        private decimal _currentBalance;
        
        // Selection and Dialog properties
        private Payment _selectedSale;
        private bool _isSaleDetailsDialogOpen;
        private bool _isPartialPaymentDialogOpen;
        private bool _isPaymentHistoryDialogOpen;
        private bool _isRefundDialogOpen;
        
        // Helper properties for dialogs
        private decimal _paidAmount;
        private decimal _remainingAmount;
        private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
        private ObservableCollection<PaymentMethod> _paymentMethods = new ObservableCollection<PaymentMethod> 
        { 
            PaymentMethod.Cash, 
            PaymentMethod.CreditCard, 
            PaymentMethod.BankTransfer 
        };

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
                FilterSales();
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

        public ObservableCollection<Payment> FilteredSales
        {
            get => _filteredSales;
            set
            {
                _filteredSales = value;
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
                FilterSales();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                FilterSales();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                FilterSales();
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

        public bool IsPartialPaymentDialogOpen
        {
            get => _isPartialPaymentDialogOpen;
            set
            {
                _isPartialPaymentDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsPaymentHistoryDialogOpen
        {
            get => _isPaymentHistoryDialogOpen;
            set
            {
                _isPaymentHistoryDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefundDialogOpen
        {
            get => _isRefundDialogOpen;
            set
            {
                _isRefundDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (_paidAmount != value)
                {
                    _paidAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set
            {
                _remainingAmount = value;
                OnPropertyChanged();
            }
        }
        
        public PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PaymentMethod> PaymentMethods => _paymentMethods;


        // Commands
        public ICommand BackCommand => _backCommand ??= new RelayCommand(GoBack);
        public ICommand ViewSaleDetailsCommand => _viewSaleDetailsCommand ??= new RelayCommand<Payment>(ViewSaleDetails);
        public ICommand CloseSaleDetailsCommand => _closeSaleDetailsCommand ??= new RelayCommand(CloseSaleDetails);
        
        public ICommand OpenPartialPaymentDialogCommand => _openPartialPaymentDialogCommand ??= new RelayCommand<Payment>(OpenPartialPaymentDialog);
        public ICommand MakePartialPaymentCommand => _makePartialPaymentCommand ??= new RelayCommand(MakePartialPayment);
        public ICommand CancelPartialPaymentCommand => _cancelPartialPaymentCommand ??= new RelayCommand(CancelPartialPayment);
        
        public ICommand ViewPaymentHistoryCommand => _viewPaymentHistoryCommand ??= new RelayCommand<Payment>(ViewPaymentHistory);
        public ICommand ClosePaymentHistoryCommand => _closePaymentHistoryCommand ??= new RelayCommand(ClosePaymentHistory);
        
        public ICommand RefundSaleCommand => _refundSaleCommand ??= new RelayCommand<Payment>(OpenRefundDialog);


        // Constructor
        public CustomerDetailViewModel(Customer customer)
        {
            _saleService = new SaleService();
            Customer = customer;
            
            // Tüm işlemleri göstermek için tarih filtresini başlangıçta null yap
            EndDate = null;
            StartDate = null;
            
            FilterSales();
        }

        // Methods
        private void FilterSales()
        {
            if (Customer == null)
            {
                FilteredSales = new ObservableCollection<Payment>();
                CurrentBalance = 0;
                return;
            }

            // Müşterinin Ödemelerini (Satışlarını) alıyoruz
            IEnumerable<Payment> filteredList = Customer.Payments ?? Enumerable.Empty<Payment>();

            // Filter by date range
            if (StartDate.HasValue)
            {
                DateTime start = StartDate.Value.Date;
                filteredList = filteredList.Where(p => p.CreatedDate.Date >= start);
            }

            if (EndDate.HasValue)
            {
                DateTime end = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                filteredList = filteredList.Where(p => p.CreatedDate <= end);
            }

            // Filter by search text (TotalAmount or potentially description if needed)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearch = SearchText.ToLower();
                filteredList = filteredList.Where(p => 
                    p.TotalAmount.ToString().Contains(SearchText) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearch))
                );
            }

            // Order by date (chronological: oldest to newest)
            filteredList = filteredList.OrderBy(p => p.CreatedDate);

            FilteredSales = new ObservableCollection<Payment>(filteredList);

            // Genel bakiye: ilgili müşterinin tüm satışlarındaki kalan borç toplamı
            try
            {
                if (Customer.Payments != null && Customer.Payments.Any())
                {
                    CurrentBalance = Customer.Payments.Sum(p => p.RemainingAmount);
                }
                else
                {
                    CurrentBalance = 0;
                }
            }
            catch
            {
                CurrentBalance = Customer?.Balance ?? 0;
            }
        }

        private void GoBack(object obj)
        {
            var mainViewModel = System.Windows.Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.NavigateToCustomers();
            }
        }

        private async void ViewSaleDetails(Payment sale)
        {
            if (sale == null) return;

            try
            {
                SelectedSale = await _saleService.GetSaleDetailsAsync(sale.Id);
                
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

        // --- PAYMENT DIALOG LOGIC ---

        private void OpenPartialPaymentDialog(Payment sale)
        {
            if (sale == null) return;
            
            SelectedSale = sale;
            PaidAmount = 0; // Reset input
            SelectedPaymentMethod = PaymentMethod.Cash;
            IsPartialPaymentDialogOpen = true;
        }

        private async void MakePartialPayment(object obj)
        {
             try
            {
                if (SelectedSale == null) return;
                
                if (PaidAmount <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir tutar giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (PaidAmount > SelectedSale.RemainingAmount)
                {
                    MessageBox.Show($"Ödeme tutarı kalan tutardan ({SelectedSale.RemainingAmount:C2}) fazla olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool result = await _saleService.AddPartialPaymentAsync(SelectedSale.Id, PaidAmount, SelectedPaymentMethod);
                
                if (result)
                {
                    MessageBox.Show("Ödeme başarıyla alındı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsPartialPaymentDialogOpen = false;
                    
                    // Verileri güncelle
                    await RefreshCustomerData();
                }
                else
                {
                    MessageBox.Show("Ödeme işlemi başarısız oldu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Ödeme işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelPartialPayment(object obj)
        {
            IsPartialPaymentDialogOpen = false;
            SelectedSale = null;
            PaidAmount = 0;
        }

        // --- PAYMENT HISTORY LOGIC ---

        private async void ViewPaymentHistory(Payment sale)
        {
             if (sale == null) return;
            
            try
            {
                // Load details to get transactions
                var detailedSale = await _saleService.GetSaleDetailsAsync(sale.Id);
                if (detailedSale != null)
                {
                    SelectedSale = detailedSale;
                    IsPaymentHistoryDialogOpen = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme geçmişi yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClosePaymentHistory(object obj)
        {
            IsPaymentHistoryDialogOpen = false;
            SelectedSale = null;
        }

        // --- REFUND LOGIC ---

        private async void OpenRefundDialog(Payment sale)
        {
             if (sale == null) return;
             
             // Detayları tam al (Items vs)
             var fullSale = await _saleService.GetSaleDetailsAsync(sale.Id);
             if (fullSale == null) return;
             
             var vm = new RefundDialogViewModel(fullSale);
             var dialog = new Nethesap.UI.Views.RefundDialog 
             { 
                 DataContext = vm,
                 Owner = System.Windows.Application.Current.MainWindow 
             };

             if (dialog.ShowDialog() == true)
             {
                 var refundItems = vm.GetRefundItems();
                 if (refundItems.Any())
                 {
                     await ProcessRefundAsync(fullSale.Id, refundItems);
                 }
             }
        }

        private async Task ProcessRefundAsync(Guid saleId, List<PaymentItem> refundItems)
        {
            try
            {
                bool success = await _saleService.RefundSaleAsync(saleId, refundItems);
                
                if (success)
                {
                     MessageBox.Show("İade işlemi başarıyla tamamlandı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                     await RefreshCustomerData();
                }
                else
                {
                    MessageBox.Show("İade işlemi başarısız oldu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İade işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- HELPER ---

        private async Task RefreshCustomerData()
        {
             // Müşteriyi yeniden yükle ve listeyi güncelle
             // CustomerService üzerinden tam veri çekmek gerekebilir ama basitçe SaleService üzerinden yenilenip yenilenmediğine bakalım.
             // En temiz yöntem, Customer'ı DB'den tazelemek.
             
             try 
             {
                 // Burada CustomerService'e ihtiyaç duyabiliriz aslında ama elimizde zaten SaleService var. 
                 // Müşteri güncellemesi için CustomerService'i kullanmak daha doğru olur.
                 var customerService = new CustomerService(); 
                 // Basit bir refresh, bu örnekte tüm müşterileri çekmek yerine tek müşteriyi çekmek daha iyi olurdu ama
                 // mevcut yapıda GetAll veya Search kullanılıyor genelde.
                 // Şimdilik sadece FilterSales'i tetikleyelim (hafızadaki nesneler güncellendiyse).
                 // Ancak Entity Framework takipli değilse manuel yenilemek lazım.
                 
                 // Customer nesnesinin Payments koleksiyonunun güncel olduğundan emin olmalıyız.
                 // SaleService operasyonları (AddPartialPayment) DB'yi günceller.
                 // Bizim görünümümüzdeki Customer nesnesi eski kalmış olabilir.
                 
                 // En güvenli yol:
                 var updatedCustomer = (await customerService.SearchCustomersAsync(Customer.Phone)).FirstOrDefault(); // Telefon ya da ID ile bul
                 
                 // ID ile bulma metodu yoksa, basitçe listeyi yenilemeyi deneyelim.
                 // Şimdilik sadece bellekteki Payments listesinin güncellenmesini umalım (eğer aynı context ise).
                 // Context farklı ise, UI'daki Customer.Payments güncellenmeyecektir.
                 
                 // Hızlı çözüm: SaleService.GetSaleByIdAsync ile güncellenen satışı bulup listede yerine koymak.
                 if (SelectedSale != null)
                 {
                     var refreshedSale = await _saleService.GetSaleByIdAsync(SelectedSale.Id);
                     if (refreshedSale != null)
                     {
                         var existingItem = Customer.Payments.FirstOrDefault(p => p.Id == refreshedSale.Id);
                         if (existingItem != null)
                         {
                             // Özellikleri güncelle
                             existingItem.PaidAmount = refreshedSale.PaidAmount;
                             existingItem.RemainingAmount = refreshedSale.RemainingAmount;
                             existingItem.IsFullyPaid = refreshedSale.IsFullyPaid;
                             existingItem.PaymentType = refreshedSale.PaymentType;
                             // vs.
                         }
                     }
                 }
                 
                 FilterSales();
             }
             catch
             {
                 // Hata olursa sessizce devam et
             }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}