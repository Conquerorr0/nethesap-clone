using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Nethesap.Domain.Entities;

namespace Nethesap.UI.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Payment> _sales;
        private ObservableCollection<Payment> _filteredSales;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<PaymentItem> _currentSaleItems;
        
        private bool _isNewSaleDialogOpen;
        private bool _isRefundDialogOpen;
        private bool _isProductSearchOpen;
        private bool _isSaleDetailsDialogOpen;
        private bool _isCustomerSearchOpen;
        
        private Product _selectedProduct;
        private Customer _selectedCustomer;
        private PaymentItem _selectedSaleItem;
        private Payment _selectedSale;
        private Payment _currentSale;
        
        private int _quantity = 1;
        private string _productSearchText;
        private string _customerSearchText;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private PaymentMethod? _filterPaymentMethod;
        
        private ICommand _addSaleCommand;
        private ICommand _saveSaleCommand;
        private ICommand _cancelSaleCommand;
        private ICommand _addProductToSaleCommand;
        private ICommand _removeProductFromSaleCommand;
        private ICommand _searchProductCommand;
        private ICommand _selectProductCommand;
        private ICommand _refundSaleCommand;
        private ICommand _processRefundCommand;
        private ICommand _cancelRefundCommand;
        private ICommand _filterSalesCommand;
        private ICommand _resetFilterCommand;
        private ICommand _viewSaleDetailsCommand;
        private ICommand _closeSaleDetailsCommand;
        private ICommand _generateReportCommand;
        private ICommand _searchCustomerCommand;
        private ICommand _selectCustomerCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public ObservableCollection<Payment> Sales
        {
            get => _sales;
            set
            {
                _sales = value;
                OnPropertyChanged();
                FilterSales(null);
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

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PaymentItem> CurrentSaleItems
        {
            get => _currentSaleItems;
            set
            {
                _currentSaleItems = value;
                OnPropertyChanged();
                CalculateTotalAmount();
            }
        }

        public bool IsNewSaleDialogOpen
        {
            get => _isNewSaleDialogOpen;
            set
            {
                _isNewSaleDialogOpen = value;
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

        public bool IsProductSearchOpen
        {
            get => _isProductSearchOpen;
            set
            {
                _isProductSearchOpen = value;
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

        public bool IsCustomerSearchOpen
        {
            get => _isCustomerSearchOpen;
            set
            {
                _isCustomerSearchOpen = value;
                OnPropertyChanged();
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Quantity = 1;
                }
            }
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                
                if (value != null && CurrentSale != null)
                {
                    CurrentSale.CustomerId = value.Id;
                    CurrentSale.Customer = value;
                }
            }
        }

        public PaymentItem SelectedSaleItem
        {
            get => _selectedSaleItem;
            set
            {
                _selectedSaleItem = value;
                OnPropertyChanged();
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

        public Payment CurrentSale
        {
            get => _currentSale;
            set
            {
                _currentSale = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                _customerSearchText = value;
                OnPropertyChanged();
                
                try
                {
                    // Arama işlemi için _customers kontrolü
                    if (_customers == null || _customers.Count == 0)
                    {
                        LoadSampleData();
                    }
                    
                    // Her değişiklikte müşterileri filtrele
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Arama için string'leri küçük harfe çevir (case-insensitive)
                        var searchText = value.ToLower();
                        
                        var filtered = _customers.Where(c =>
                            c.FirstName?.ToLower().Contains(searchText) == true ||
                            c.LastName?.ToLower().Contains(searchText) == true ||
                            c.Phone?.ToLower().Contains(searchText) == true ||
                            c.Email?.ToLower().Contains(searchText) == true).ToList();
                        
                        Customers = new ObservableCollection<Customer>(filtered);
                    }
                    else
                    {
                        // Boş metin ise tüm müşterileri göster
                        Customers = new ObservableCollection<Customer>(_customers);
                    }
                    
                    // Popup'ı her durumda aç
                    IsCustomerSearchOpen = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Müşteri aramada hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                
                try
                {
                    // Arama işlemi için _products kontrolü
                    if (_products == null || _products.Count == 0)
                    {
                        LoadSampleData();
                    }
                    
                    // Her değişiklikte ürünleri filtrele
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Arama için string'leri küçük harfe çevir (case-insensitive)
                        var searchText = value.ToLower();
                        
                        var filtered = _products.Where(p =>
                            p.Name?.ToLower().Contains(searchText) == true ||
                            p.Description?.ToLower().Contains(searchText) == true ||
                            p.Barcode?.ToLower().Contains(searchText) == true).ToList();
                        
                        Products = new ObservableCollection<Product>(filtered);
                    }
                    else
                    {
                        // Boş metin ise tüm ürünleri göster
                        Products = new ObservableCollection<Product>(_products);
                    }
                    
                    // Popup'ı her durumda aç
                    IsProductSearchOpen = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ürün aramada hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public PaymentMethod? FilterPaymentMethod
        {
            get => _filterPaymentMethod;
            set
            {
                _filterPaymentMethod = value;
                OnPropertyChanged();
            }
        }

        public Array PaymentMethods => Enum.GetValues(typeof(PaymentMethod));

        // Commands
        public ICommand AddSaleCommand => _addSaleCommand ??= new RelayCommand(OpenNewSaleDialog);
        public ICommand SaveSaleCommand => _saveSaleCommand ??= new RelayCommand(SaveSale);
        public ICommand CancelSaleCommand => _cancelSaleCommand ??= new RelayCommand(CancelSale);
        public ICommand AddProductToSaleCommand => _addProductToSaleCommand ??= new RelayCommand(AddProductToSale);
        public ICommand RemoveProductFromSaleCommand => _removeProductFromSaleCommand ??= new RelayCommand<PaymentItem>(RemoveProductFromSale);
        public ICommand SearchProductCommand => _searchProductCommand ??= new RelayCommand(OpenProductSearch);
        public ICommand SelectProductCommand => _selectProductCommand ??= new RelayCommand<Product>(SelectProduct);
        public ICommand RefundSaleCommand => _refundSaleCommand ??= new RelayCommand<Payment>(OpenRefundDialog);
        public ICommand ProcessRefundCommand => _processRefundCommand ??= new RelayCommand(ProcessRefund);
        public ICommand CancelRefundCommand => _cancelRefundCommand ??= new RelayCommand(CancelRefund);
        public ICommand FilterSalesCommand => _filterSalesCommand ??= new RelayCommand(FilterSales);
        public ICommand ResetFilterCommand => _resetFilterCommand ??= new RelayCommand(ResetFilter);
        public ICommand ViewSaleDetailsCommand => _viewSaleDetailsCommand ??= new RelayCommand<Payment>(ViewSaleDetails);
        public ICommand CloseSaleDetailsCommand => _closeSaleDetailsCommand ??= new RelayCommand(CloseSaleDetails);
        public ICommand GenerateReportCommand => _generateReportCommand ??= new RelayCommand(GenerateReport);
        public ICommand SearchCustomerCommand => _searchCustomerCommand ??= new RelayCommand(OpenCustomerSearch);
        public ICommand SelectCustomerCommand => _selectCustomerCommand ??= new RelayCommand<Customer>(SelectCustomer);

        // Constructor
        public SalesViewModel()
        {
            // Listeleri ilk olarak burada başlat
            _sales = new ObservableCollection<Payment>();
            Sales = new ObservableCollection<Payment>();
            _filteredSales = new ObservableCollection<Payment>();
            FilteredSales = new ObservableCollection<Payment>();
            
            _products = new ObservableCollection<Product>();
            Products = new ObservableCollection<Product>();
            
            _customers = new ObservableCollection<Customer>();
            Customers = new ObservableCollection<Customer>();
            
            _currentSaleItems = new ObservableCollection<PaymentItem>();
            CurrentSaleItems = new ObservableCollection<PaymentItem>();
            
            _productSearchText = string.Empty;
            _customerSearchText = string.Empty;
            
            // Sonra örnek verileri yükle
            LoadSampleData();

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
        }

        // Methods
        private void LoadSampleData()
        {
            try
            {
                // Sample customers
                var customersList = new List<Customer>
                {
                    new Customer { Id = Guid.NewGuid(), FirstName = "Ahmet", LastName = "Yılmaz", Phone = "555-123-4567", Email = "ahmet@example.com" },
                    new Customer { Id = Guid.NewGuid(), FirstName = "Mehmet", LastName = "Kaya", Phone = "555-234-5678", Email = "mehmet@example.com" },
                    new Customer { Id = Guid.NewGuid(), FirstName = "Ayşe", LastName = "Demir", Phone = "555-345-6789", Email = "ayse@example.com" }
                };
                
                // Sample products
                var productsList = new List<Product>
                {
                    new Product { Id = Guid.NewGuid(), Name = "Laptop", Description = "Yüksek performanslı dizüstü bilgisayar", Price = 12000, Barcode = "PRD-001", StockQuantity = 10 },
                    new Product { Id = Guid.NewGuid(), Name = "Tablet", Description = "Kompakt tablet bilgisayar", Price = 5000, Barcode = "PRD-002", StockQuantity = 15 },
                    new Product { Id = Guid.NewGuid(), Name = "Akıllı Telefon", Description = "Son model akıllı telefon", Price = 8000, Barcode = "PRD-003", StockQuantity = 20 }
                };
                
                // Sample sales
                var salesList = new List<Payment>
                {
                    new Payment { Id = Guid.NewGuid(), PaymentType = PaymentType.Sale, PaymentMethod = PaymentMethod.Cash, TotalAmount = 12000, CreatedDate = DateTime.Now.AddDays(-5) },
                    new Payment { Id = Guid.NewGuid(), PaymentType = PaymentType.Sale, PaymentMethod = PaymentMethod.CreditCard, TotalAmount = 5000, CreatedDate = DateTime.Now.AddDays(-3) },
                    new Payment { Id = Guid.NewGuid(), PaymentType = PaymentType.Sale, PaymentMethod = PaymentMethod.BankTransfer, TotalAmount = 8000, CreatedDate = DateTime.Now.AddDays(-1) }
                };
                
                // Listeleri temizle ve yeniden doldur
                _customers.Clear();
                foreach (var customer in customersList)
                {
                    _customers.Add(customer);
                }
                
                _products.Clear();
                foreach (var product in productsList)
                {
                    _products.Add(product);
                }
                
                _sales.Clear();
                foreach (var sale in salesList)
                {
                    _sales.Add(sale);
                }
                
                // Observable koleksiyonları güncelle
                Customers = new ObservableCollection<Customer>(_customers);
                Products = new ObservableCollection<Product>(_products);
                Sales = new ObservableCollection<Payment>(_sales);
                FilteredSales = new ObservableCollection<Payment>(_sales);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Örnek veri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenNewSaleDialog(object obj)
        {
            try
            {
                // Yeni satış oluştur
                CurrentSale = new Payment
                {
                    Id = Guid.NewGuid(),
                    PaymentType = PaymentType.Sale,
                    PaymentMethod = PaymentMethod.Cash,
                    CreatedDate = DateTime.Now,
                    PaymentItems = new List<PaymentItem>()
                };
                
                // Veri listelerini yeniden başlat
                if (CurrentSaleItems == null)
                    CurrentSaleItems = new ObservableCollection<PaymentItem>();
                else
                    CurrentSaleItems.Clear();
                    
                // Seçimleri temizle
                SelectedCustomer = null;
                SelectedProduct = null;
                
                // Arama kutularını temizle
                _customerSearchText = string.Empty;
                OnPropertyChanged(nameof(CustomerSearchText));
                
                _productSearchText = string.Empty;
                OnPropertyChanged(nameof(ProductSearchText));
                
                // Popup'ları kapat
                IsCustomerSearchOpen = false;
                IsProductSearchOpen = false;
                
                // Dialog'u göster
                IsNewSaleDialogOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yeni satış ekranı açılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSale(object obj)
        {
            if (CurrentSale.CustomerId == Guid.Empty)
            {
                MessageBox.Show("Lütfen bir müşteri seçiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentSaleItems.Count == 0)
            {
                MessageBox.Show("Lütfen satışa en az bir ürün ekleyiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add payment items to the payment
            foreach (var item in CurrentSaleItems)
            {
                item.PaymentId = CurrentSale.Id;
                item.Payment = CurrentSale;
                ((List<PaymentItem>)CurrentSale.PaymentItems).Add(item);
                
                // Update product stock
                var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // Set total amount
            CurrentSale.TotalAmount = CurrentSaleItems.Sum(i => i.TotalPrice);
            
            // Add payment to sales
            Sales.Insert(0, CurrentSale);
            FilterSales(null);
            
            IsNewSaleDialogOpen = false;
        }

        private void CancelSale(object obj)
        {
            IsNewSaleDialogOpen = false;
        }

        private void AddProductToSale(object obj)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Lütfen bir ürün seçiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Miktar 0'dan büyük olmalıdır!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Quantity > SelectedProduct.StockQuantity)
            {
                MessageBox.Show("Stokta yeterli ürün bulunmamaktadır!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if product already exists in the sale
            var existingItem = CurrentSaleItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
            if (existingItem != null)
            {
                // Update existing item
                existingItem.Quantity += Quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                // Create new payment item
                var paymentItem = new PaymentItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = SelectedProduct.Id,
                    Product = SelectedProduct,
                    Quantity = Quantity,
                    UnitPrice = SelectedProduct.Price,
                    TotalPrice = SelectedProduct.Price * Quantity
                };

                CurrentSaleItems.Add(paymentItem);
            }

            // Clear selection
            SelectedProduct = null;
            ProductSearchText = string.Empty;
            Quantity = 1;
            
            IsProductSearchOpen = false;
            CalculateTotalAmount();
        }

        private void RemoveProductFromSale(PaymentItem item)
        {
            if (item != null)
            {
                CurrentSaleItems.Remove(item);
                CalculateTotalAmount();
            }
        }

        private void OpenProductSearch(object obj)
        {
            try
            {
                // Ürün listesini sıfırla ve arama sonuçlarını göster
                if (string.IsNullOrWhiteSpace(ProductSearchText))
                {
                    // Boş ise tüm ürünleri göster
                    if (_products != null)
                        Products = new ObservableCollection<Product>(_products);
                }
                else
                {
                    // Zaten metin varsa arama yap
                    SearchProducts();
                }
                
                // Popup'ı göster - popup her zaman açılsın
                IsProductSearchOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectProduct(Product product)
        {
            try
            {
                if (product != null)
                {
                    SelectedProduct = product;
                    
                    // ProductSearchText'i ürün bilgisiyle doldur ama arama yapmaması için
                    _productSearchText = product.Name;
                    OnPropertyChanged(nameof(ProductSearchText));
                    
                    // Popup'ı kapat
                    IsProductSearchOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün seçimi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchProducts()
        {
            try
            {
                if (_products == null)
                {
                    // _products null ise oluştur
                    LoadSampleData();
                }
                    
                if (string.IsNullOrWhiteSpace(ProductSearchText))
                {
                    Products = new ObservableCollection<Product>(_products);
                    IsProductSearchOpen = true;  // Her durumda popup'ı aç
                    return;
                }

                var filteredProducts = _products.Where(p =>
                    p.Name.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Barcode?.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                Products = new ObservableCollection<Product>(filteredProducts);
                IsProductSearchOpen = true;  // Her durumda popup'ı aç
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün filtrelemede hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchCustomers()
        {
            try
            {
                if (_customers == null)
                {
                    // _customers null ise oluştur
                    LoadSampleData();
                }
                    
                if (string.IsNullOrWhiteSpace(CustomerSearchText))
                {
                    Customers = new ObservableCollection<Customer>(_customers);
                    IsCustomerSearchOpen = true;  // Her durumda popup'ı aç
                    return;
                }

                var filteredCustomers = _customers.Where(c =>
                    c.FirstName.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.LastName.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Email?.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                Customers = new ObservableCollection<Customer>(filteredCustomers);
                IsCustomerSearchOpen = true;  // Her durumda popup'ı aç
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Müşteri filtrelemede hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalAmount()
        {
            if (CurrentSale != null)
            {
                CurrentSale.TotalAmount = CurrentSaleItems.Sum(i => i.TotalPrice);
                OnPropertyChanged(nameof(CurrentSale));
            }
        }

        private void OpenRefundDialog(Payment sale)
        {
            if (sale != null)
            {
                SelectedSale = sale;
                IsRefundDialogOpen = true;
            }
        }

        private void ProcessRefund(object obj)
        {
            if (SelectedSale == null)
            {
                return;
            }

            // Create a refund payment
            var refundPayment = new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = SelectedSale.CustomerId,
                Customer = SelectedSale.Customer,
                PaymentMethod = SelectedSale.PaymentMethod,
                PaymentType = PaymentType.Refund,
                Description = $"İade - {SelectedSale.Id}",
                TotalAmount = -SelectedSale.TotalAmount,
                CreatedDate = DateTime.Now,
                PaymentItems = new List<PaymentItem>()
            };

            // Add refund items
            foreach (var item in SelectedSale.PaymentItems)
            {
                var refundItem = new PaymentItem
                {
                    Id = Guid.NewGuid(),
                    PaymentId = refundPayment.Id,
                    Payment = refundPayment,
                    ProductId = item.ProductId,
                    Product = item.Product,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = -item.TotalPrice
                };

                ((List<PaymentItem>)refundPayment.PaymentItems).Add(refundItem);
                
                // Update product stock
                var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }

            // Add refund to sales
            Sales.Insert(0, refundPayment);
            FilterSales(null);
            
            IsRefundDialogOpen = false;
            SelectedSale = null;
        }

        private void CancelRefund(object obj)
        {
            IsRefundDialogOpen = false;
            SelectedSale = null;
        }

        private void FilterSales(object obj = null)
        {
            var filteredSales = Sales.AsEnumerable();

            // Filter by date range
            if (StartDate.HasValue)
            {
                var startDate = StartDate.Value.Date;
                filteredSales = filteredSales.Where(s => s.CreatedDate.Date >= startDate);
            }

            if (EndDate.HasValue)
            {
                var endDate = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                filteredSales = filteredSales.Where(s => s.CreatedDate <= endDate);
            }

            // Filter by payment method
            if (FilterPaymentMethod.HasValue)
            {
                filteredSales = filteredSales.Where(s => s.PaymentMethod == FilterPaymentMethod.Value);
            }

            FilteredSales = new ObservableCollection<Payment>(filteredSales);
        }

        private void ResetFilter(object obj)
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            FilterPaymentMethod = null;
            FilterSales(null);
        }

        private void ViewSaleDetails(Payment sale)
        {
            if (sale != null)
            {
                SelectedSale = sale;
                IsSaleDetailsDialogOpen = true;
            }
        }

        private void CloseSaleDetails(object obj)
        {
            IsSaleDetailsDialogOpen = false;
        }

        private void GenerateReport(object obj)
        {
            if (FilteredSales == null || FilteredSales.Count == 0)
            {
                MessageBox.Show("Rapor oluşturmak için satış verisi bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var reportContent = new System.Text.StringBuilder();
            reportContent.AppendLine("NETHESAP - SATIŞ RAPORU");
            reportContent.AppendLine("=======================");
            reportContent.AppendLine();
            
            // Filtreleme bilgileri
            reportContent.AppendLine($"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            
            if (StartDate.HasValue)
                reportContent.AppendLine($"Başlangıç Tarihi: {StartDate:dd.MM.yyyy}");
            
            if (EndDate.HasValue)
                reportContent.AppendLine($"Bitiş Tarihi: {EndDate:dd.MM.yyyy}");
            
            if (FilterPaymentMethod.HasValue)
                reportContent.AppendLine($"Ödeme Yöntemi: {FilterPaymentMethod}");
            
            reportContent.AppendLine();
            reportContent.AppendLine($"Toplam Satış Adedi: {FilteredSales.Count}");
            reportContent.AppendLine($"Toplam Satış Tutarı: {FilteredSales.Sum(s => s.TotalAmount):C2}");
            reportContent.AppendLine();
            
            // Ödeme yöntemine göre gruplandırma
            var paymentMethodGroups = FilteredSales.GroupBy(s => s.PaymentMethod);
            reportContent.AppendLine("ÖDEME YÖNTEMİNE GÖRE SATIŞLAR");
            reportContent.AppendLine("----------------------------");
            
            foreach (var group in paymentMethodGroups)
            {
                string paymentMethodName = group.Key.ToString();
                switch (group.Key)
                {
                    case PaymentMethod.Cash:
                        paymentMethodName = "Nakit";
                        break;
                    case PaymentMethod.CreditCard:
                        paymentMethodName = "Kredi Kartı";
                        break;
                    case PaymentMethod.BankTransfer:
                        paymentMethodName = "Havale";
                        break;
                }
                
                reportContent.AppendLine($"{paymentMethodName}: {group.Count()} adet, {group.Sum(s => s.TotalAmount):C2}");
            }
            
            reportContent.AppendLine();
            reportContent.AppendLine("SATIŞ LİSTESİ");
            reportContent.AppendLine("------------");
            
            foreach (var sale in FilteredSales)
            {
                reportContent.AppendLine($"Tarih: {sale.CreatedDate:dd.MM.yyyy HH:mm}, Müşteri: {sale.Customer.FirstName} {sale.Customer.LastName}, Tutar: {sale.TotalAmount:C2}");
            }
            
            // Raporu bir dosyaya kaydetmek için SaveFileDialog kullanımı
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text dosyaları (*.txt)|*.txt|Tüm dosyalar (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"Nethesap_Satis_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, reportContent.ToString());
                    MessageBox.Show($"Rapor başarıyla kaydedildi: {saveFileDialog.FileName}", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Rapor kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenCustomerSearch(object obj)
        {
            try
            {
                // Ürün listesini sıfırla ve arama sonuçlarını göster
                if (string.IsNullOrWhiteSpace(CustomerSearchText))
                {
                    // Boş ise tüm müşterileri göster
                    if (_customers != null)
                        Customers = new ObservableCollection<Customer>(_customers);
                }
                else
                {
                    // Zaten metin varsa arama yap
                    SearchCustomers();
                }
                
                // Popup'ı göster - popup her zaman açılsın
                IsCustomerSearchOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Müşteri arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectCustomer(Customer customer)
        {
            try
            {
                if (customer != null)
                {
                    SelectedCustomer = customer;
                    
                    // CustomerSearchText'i müşteri bilgisiyle doldur ama arama yapmaması için
                    _customerSearchText = $"{customer.FirstName} {customer.LastName}";
                    OnPropertyChanged(nameof(CustomerSearchText));
                    
                    // Satış bilgisini güncelle
                    if (CurrentSale != null)
                    {
                        CurrentSale.CustomerId = customer.Id;
                        CurrentSale.Customer = customer;
                    }
                    
                    // Popup'ı kapat
                    IsCustomerSearchOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Müşteri seçimi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 