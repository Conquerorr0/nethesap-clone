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

        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                SearchProducts();
            }
        }

        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                _customerSearchText = value;
                OnPropertyChanged();
                SearchCustomers();
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

        // Constructor
        public SalesViewModel()
        {
            LoadSampleData();
            CurrentSaleItems = new ObservableCollection<PaymentItem>();
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
        }

        // Methods
        private void LoadSampleData()
        {
            // Sample customers
            Customers = new ObservableCollection<Customer>
            {
                new Customer { Id = Guid.NewGuid(), FirstName = "Ahmet", LastName = "Yılmaz", Phone = "555-123-4567", Email = "ahmet@example.com" },
                new Customer { Id = Guid.NewGuid(), FirstName = "Mehmet", LastName = "Kaya", Phone = "555-234-5678", Email = "mehmet@example.com" },
                new Customer { Id = Guid.NewGuid(), FirstName = "Ayşe", LastName = "Demir", Phone = "555-345-6789", Email = "ayse@example.com" }
            };

            // Sample products
            Products = new ObservableCollection<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Laptop", Description = "Oyun Bilgisayarı", Price = 25000.00m, StockQuantity = 10 },
                new Product { Id = Guid.NewGuid(), Name = "Telefon", Description = "Akıllı Telefon", Price = 15000.00m, StockQuantity = 20 },
                new Product { Id = Guid.NewGuid(), Name = "Klavye", Description = "Mekanik Klavye", Price = 1200.00m, StockQuantity = 50 },
                new Product { Id = Guid.NewGuid(), Name = "Mouse", Description = "Gaming Mouse", Price = 800.00m, StockQuantity = 30 },
                new Product { Id = Guid.NewGuid(), Name = "Monitor", Description = "27\" 4K Monitor", Price = 8000.00m, StockQuantity = 15 }
            };

            // Sample sales
            var sales = new List<Payment>();
            var random = new Random();

            for (int i = 0; i < 10; i++)
            {
                var customer = Customers[random.Next(Customers.Count)];
                var paymentMethod = (PaymentMethod)random.Next(Enum.GetValues(typeof(PaymentMethod)).Length);
                var date = DateTime.Now.AddDays(-random.Next(1, 30));

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Customer = customer,
                    PaymentMethod = paymentMethod,
                    PaymentType = PaymentType.Sale,
                    Description = "Örnek satış",
                    CreatedDate = date,
                    PaymentItems = new List<PaymentItem>()
                };

                var totalAmount = 0m;
                var itemCount = random.Next(1, 4);

                for (int j = 0; j < itemCount; j++)
                {
                    var product = Products[random.Next(Products.Count)];
                    var quantity = random.Next(1, 5);
                    var unitPrice = product.Price;
                    var totalPrice = unitPrice * quantity;

                    var paymentItem = new PaymentItem
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = payment.Id,
                        Payment = payment,
                        ProductId = product.Id,
                        Product = product,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    };

                    payment.PaymentItems.Add(paymentItem);
                    totalAmount += totalPrice;
                }

                payment.TotalAmount = totalAmount;
                sales.Add(payment);
            }

            Sales = new ObservableCollection<Payment>(sales.OrderByDescending(s => s.Id));
            FilteredSales = new ObservableCollection<Payment>(Sales);
        }

        private void OpenNewSaleDialog(object obj)
        {
            CurrentSale = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentType = PaymentType.Sale,
                PaymentMethod = PaymentMethod.Cash,
                CreatedDate = DateTime.Now,
                PaymentItems = new List<PaymentItem>()
            };
            
            CurrentSaleItems.Clear();
            SelectedCustomer = null;
            CustomerSearchText = string.Empty;
            
            IsNewSaleDialogOpen = true;
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
            ProductSearchText = string.Empty;
            SearchProducts();
            IsProductSearchOpen = true;
        }

        private void SelectProduct(Product product)
        {
            SelectedProduct = product;
            ProductSearchText = product.Name;
            IsProductSearchOpen = false;
        }

        private void SearchProducts()
        {
            if (string.IsNullOrWhiteSpace(ProductSearchText))
            {
                Products = new ObservableCollection<Product>(_products);
                return;
            }

            var filteredProducts = _products.Where(p =>
                p.Name.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Barcode?.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            Products = new ObservableCollection<Product>(filteredProducts);
        }

        private void SearchCustomers()
        {
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                Customers = new ObservableCollection<Customer>(_customers);
                return;
            }

            var filteredCustomers = _customers.Where(c =>
                c.FirstName.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Email?.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            Customers = new ObservableCollection<Customer>(filteredCustomers);
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 