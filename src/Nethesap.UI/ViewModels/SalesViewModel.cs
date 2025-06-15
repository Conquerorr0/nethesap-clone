using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Nethesap.Domain.Entities;
using Nethesap.UI.Services;

namespace Nethesap.UI.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Payment> _sales = new ObservableCollection<Payment>();
        private ObservableCollection<Payment> _filteredSales = new ObservableCollection<Payment>();
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();
        private ObservableCollection<PaymentItem> _currentSaleItems = new ObservableCollection<PaymentItem>();
        
        private bool _isNewSaleDialogOpen;
        private bool _isRefundDialogOpen;
        private bool _isProductSearchOpen;
        private bool _isSaleDetailsDialogOpen;
        private bool _isCustomerSearchOpen;
        
        private Product? _selectedProduct = null;
        private Customer? _selectedCustomer = null;
        private PaymentItem? _selectedSaleItem = null;
        private Payment? _selectedSale = null;
        private Payment? _currentSale = null;
        
        private int _quantity = 1;
        private string _productSearchText = string.Empty;
        private string _customerSearchText = string.Empty;
        private DateTime? _startDate = null;
        private DateTime? _endDate = null;
        private PaymentMethod? _filterPaymentMethod = null;
        
        private ICommand? _addSaleCommand;
        private ICommand? _saveSaleCommand;
        private ICommand? _cancelSaleCommand;
        private ICommand? _addProductToSaleCommand;
        private ICommand? _removeProductFromSaleCommand;
        private ICommand? _searchProductCommand;
        private ICommand? _selectProductCommand;
        private ICommand? _refundSaleCommand;
        private ICommand? _processRefundCommand;
        private ICommand? _cancelRefundCommand;
        private ICommand? _filterSalesCommand;
        private ICommand? _resetFilterCommand;
        private ICommand? _viewSaleDetailsCommand;
        private ICommand? _closeSaleDetailsCommand;
        private ICommand? _generateReportCommand;
        private ICommand? _searchCustomerCommand;
        private ICommand? _selectCustomerCommand;
        private bool _isLoading;
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;
        private readonly SaleService _saleService;
        private int _totalSales;
        private decimal _totalRevenue;
        private decimal _averageRevenue;
        private ObservableCollection<PaymentType> _paymentTypes = new ObservableCollection<PaymentType>();
        private ObservableCollection<PaymentMethod> _paymentMethods = new ObservableCollection<PaymentMethod>();
        private ObservableCollection<Product> _filteredProducts = new ObservableCollection<Product>();

        public event PropertyChangedEventHandler? PropertyChanged;

        // Properties
        public ObservableCollection<Payment> Sales
        {
            get => _sales;
            set
            {
                _sales = value;
                OnPropertyChanged();
                
                // FilterSales metodu async olduğu için burada async çağrıdan kaçınalım
                // Bunun yerine, FilteredSales'i doğrudan güncelleyelim
                try
                {
                    if (value != null)
                    {
                        FilteredSales = new ObservableCollection<Payment>(value);
                    }
                    else
                    {
                        FilteredSales = new ObservableCollection<Payment>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sales property'sinde filtreleme hatası: {ex.Message}");
                }
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
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
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
                if (_isProductSearchOpen == value) return;
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
                if (_selectedProduct == value) return;
                _selectedProduct = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    ProductSearchText = value.Name;
                    IsProductSearchOpen = false;
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
                    // Arama işlemi için müşteri verilerini kontrol et
                    if (_customers == null || _customers.Count == 0)
                    {
                        // Veritabanından müşterileri yükle
                        SearchCustomersAsync();
                        return;
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
                if (_productSearchText == value) return;
                _productSearchText = value;
                OnPropertyChanged();
                
                // Arama metni değiştiğinde her zaman asenkron olarak ürünleri ara
                // Bu, UI'deki "Products" koleksiyonunu güncelleyecek ve arama sonuçlarını gösterecek.
                SearchProductsAsync(); 
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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public int TotalSales
        {
            get => _totalSales;
            set
            {
                if (_totalSales != value)
                {
                    _totalSales = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                if (_totalRevenue != value)
                {
                    _totalRevenue = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal AverageRevenue
        {
            get => _averageRevenue;
            set
            {
                if (_averageRevenue != value)
                {
                    _averageRevenue = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<PaymentType> PaymentTypes
        {
            get => _paymentTypes;
            set
            {
                _paymentTypes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PaymentMethod> PaymentMethods
        {
            get => _paymentMethods;
            set
            {
                _paymentMethods = value;
                OnPropertyChanged();
            }
        }

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
            try
            {
                Console.WriteLine("SalesViewModel başlatılıyor...");
                
                // Servisleri başlat
                _productService = new ProductService();
                _customerService = new CustomerService();
                _saleService = new SaleService();
                
                // Koleksiyonları başlat
                InitializeCollections();
                
                // Tarih filtre değerlerini ayarla
                StartDate = DateTime.Now.AddMonths(-1);
                EndDate = DateTime.Now;
                
                // Veri yüklemeyi başlat
                LoadDataAsync().ConfigureAwait(false);
                
                Console.WriteLine("SalesViewModel başlatıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SalesViewModel oluşturulurken hata: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Temel bileşenleri oluşturmaya çalış
                try 
                {
                    InitializeCollections();
                }
                catch 
                {
                    // En kötü durumda bile çökmeden devam etmeye çalış
                }
            }
        }

        // Methods
        private void OpenNewSaleDialog(object obj)
        {
            try
            {
                // Yeni bir satış oluştur
                CurrentSale = new Payment
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentType = PaymentType.Sale,
                    PaymentItems = new List<PaymentItem>()
                };
                
                // Satış detayları için ObservableCollection oluştur
                if (CurrentSaleItems == null)
                {
                    CurrentSaleItems = new ObservableCollection<PaymentItem>();
                }
                else
                {
                    CurrentSaleItems.Clear();
                }
                
                // Müşteri ve ürün seçimini temizle
                SelectedCustomer = null;
                SelectedProduct = null;
                
                // Miktar değerini sıfırla
                Quantity = 1;
                
                // Arama metinlerini temizle
                _customerSearchText = string.Empty;
                OnPropertyChanged(nameof(CustomerSearchText));
                _productSearchText = string.Empty;
                OnPropertyChanged(nameof(ProductSearchText));
                
                // Popup'ları kapat
                IsCustomerSearchOpen = false;
                IsProductSearchOpen = false;
                
                Console.WriteLine($"Yeni satış oluşturuldu: ID: {CurrentSale.Id}");
                
                // Satış formunu aç
                IsNewSaleDialogOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yeni satış formu açılırken hata oluştu: {ex.Message}\n\nAyrıntılar: {ex.InnerException?.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Yeni satış formu açılırken hata: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private async void SaveSale(object obj)
        {
            try
            {
                IsLoading = true;
                
                // Kontroller
                if (CurrentSaleItems == null || !CurrentSaleItems.Any())
                {
                    MessageBox.Show("Satış boş olamaz! Lütfen en az bir ürün ekleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CurrentSale == null)
                {
                    MessageBox.Show("Geçerli bir satış bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Lütfen bir müşteri seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Müşteri ID'sini doğru şekilde ayarla
                CurrentSale.CustomerId = SelectedCustomer.Id;
                CurrentSale.Customer = SelectedCustomer;
                
                Console.WriteLine($"Müşteri bilgileri: ID: {SelectedCustomer.Id}, Ad: {SelectedCustomer.FirstName} {SelectedCustomer.LastName}");
                Console.WriteLine($"CurrentSale.CustomerId: {CurrentSale.CustomerId}");

                try
                {
                    // Payment nesnesini hazırla
                    var saleId = CurrentSale.Id == Guid.Empty ? Guid.NewGuid() : CurrentSale.Id;
                    
                    // Basit bir Payment nesnesi oluştur
                    var saleToSave = new Payment
                    {
                        Id = saleId,
                        CustomerId = SelectedCustomer.Id, // Doğrudan SelectedCustomer'dan al
                        CreatedDate = DateTime.Now,
                        Description = $"{SelectedCustomer.FirstName} {SelectedCustomer.LastName} - {DateTime.Now:dd.MM.yyyy}",
                        PaymentMethod = PaymentMethod.Cash,
                        PaymentType = PaymentType.Sale,
                        TotalAmount = CurrentSaleItems.Sum(i => i.TotalPrice),
                        PaymentItems = new List<PaymentItem>()
                    };
                    
                    Console.WriteLine($"Kaydedilecek satış: ID: {saleToSave.Id}, Müşteri ID: {saleToSave.CustomerId}");
                    
                    // Yeni PaymentItem nesneleri oluştur ve PaymentId'yi doğru şekilde ayarla
                    foreach (var item in CurrentSaleItems)
                    {
                        var newItem = new PaymentItem
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = saleId,
                            ProductId = item.Product?.Id ?? item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.TotalPrice
                        };
                        
                        // Product navigation property'sini NULL olarak ayarla
                        newItem.Product = null;
                        
                        // Yeni oluşturulan item'ı listeye ekle
                        saleToSave.PaymentItems.Add(newItem);
                        Console.WriteLine($"Ürün eklendi: ID: {newItem.ProductId}, Miktar: {newItem.Quantity}, Fiyat: {newItem.TotalPrice}");
                    }
                    
                    Console.WriteLine($"Kaydedilecek satış: ID: {saleToSave.Id}, Müşteri: {saleToSave.CustomerId}, Ürün Sayısı: {saleToSave.PaymentItems.Count}, Toplam: {saleToSave.TotalAmount:C2}");
                    
                    // Satışı kaydet
                    bool success = await _saleService.AddSaleAsync(saleToSave);
                    
                    if (success)
                    {
                        // Satış listesine ekle (UI güncelleme)
                        saleToSave.Customer = SelectedCustomer; // UI için Customer referansını ekle
                        Sales.Add(saleToSave);
                        FilteredSales.Add(saleToSave);
                        
                        // İstatistikleri güncelle
                        CalculateStatistics();
                        
                        // Başarılı mesajı göster
                        MessageBox.Show($"Satış başarıyla kaydedildi.\nToplam Tutar: {saleToSave.TotalAmount:C2}", 
                            "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Yeni satış formunu aç
                        OpenNewSaleDialog(null);
                    }
                    else
                    {
                        MessageBox.Show("Satış kaydedilirken bir hata oluştu! Detaylar için konsol çıktısını kontrol edin.", 
                            "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Satış servisinde hata: {ex.Message}\n\nDetaylar: {ex.InnerException?.Message}", 
                        "Servis Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine($"SaleService.AddSaleAsync hatası: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satış kaydedilirken hata oluştu: {ex.Message}\n\nDetaylar: {ex.InnerException?.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"SaveSale metodu hatası: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelSale(object obj)
        {
            IsNewSaleDialogOpen = false;
        }

        private void AddProductToSale(object obj)
        {
            try
            {
                Console.WriteLine("Ürün ekleme işlemi başlatıldı...");
                Console.WriteLine($"AddProductToSale: SelectedProduct = {SelectedProduct?.Name ?? "NULL"}, ProductSearchText = '{ProductSearchText}'");
                
                // SelectedProduct null ise ve ProductSearchText dolu ise, arama sonuçlarından seçmeye çalış
                if (SelectedProduct == null && !string.IsNullOrWhiteSpace(ProductSearchText))
                {
                    var searchTextLower = ProductSearchText.ToLower();
                    // Products koleksiyonu SearchProductsAsync tarafından güncellendiği için,
                    // burada doğrudan o koleksiyon üzerinden arama yapabiliriz.
                    var matchingProduct = Products?.FirstOrDefault(p =>
                        p.Name?.ToLower() == searchTextLower ||
                        p.Barcode?.ToLower() == searchTextLower);

                    if (matchingProduct != null)
                    {
                        SelectedProduct = matchingProduct;
                        Console.WriteLine($"Ürün bulundu (arama sonuçlarından): {SelectedProduct.Name}");
                    }
                    else if (Products != null && Products.Count == 1) // Arama sonuçlarında tek ürün varsa onu seç
                    {
                        SelectedProduct = Products.First();
                        Console.WriteLine($"Ürün bulundu (tek arama sonucu): {SelectedProduct.Name}");
                    }
                    else
                    {
                        MessageBox.Show("Lütfen listeden bir ürün seçiniz veya arama yapınız!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else if (SelectedProduct == null)
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
                
                Console.WriteLine($"Ürün: {SelectedProduct.Name}, Miktar: {Quantity}, Fiyat: {SelectedProduct.Price:C2}");

                // CurrentSaleItems kontrolü
                if (CurrentSaleItems == null)
                {
                    Console.WriteLine("CurrentSaleItems null olduğu için yeni koleksiyon oluşturuluyor...");
                    CurrentSaleItems = new ObservableCollection<PaymentItem>();
                }

                // Check if product already exists in the sale
                var existingItem = CurrentSaleItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += Quantity;
                    existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                    existingItem.Product = SelectedProduct; // Ürün bilgisini güncelle
                    Console.WriteLine($"Mevcut ürün güncellendi: {existingItem.Product.Name}, Yeni Miktar: {existingItem.Quantity}");
                }
                else
                {
                    // Create new payment item
                    var paymentItem = new PaymentItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = SelectedProduct.Id,
                        Product = SelectedProduct, // Ürün bilgisini ekle
                        Quantity = Quantity,
                        UnitPrice = SelectedProduct.Price,
                        TotalPrice = SelectedProduct.Price * Quantity
                    };

                    CurrentSaleItems.Add(paymentItem);
                    Console.WriteLine($"Yeni ürün eklendi: {paymentItem.Product.Name}, Miktar: {paymentItem.Quantity}");
                }

                // Clear selection (ürün eklendikten sonra temizle, sonraki ekleme için)
                SelectedProduct = null;
                ProductSearchText = string.Empty;
                Quantity = 1;
                
                // Toplam tutarı güncelle
                CalculateTotalAmount();
                
                Console.WriteLine($"Güncel sepet: {CurrentSaleItems.Count} ürün, Toplam: {CurrentSale?.TotalAmount:C2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün eklenirken hata oluştu: {ex.Message}\n\nDetaylar: {ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ürün ekleme hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void RemoveProductFromSale(PaymentItem item)
        {
            try
            {
                if (item != null && CurrentSaleItems != null)
                {
                    // Sepetten ürünü kaldır
                    CurrentSaleItems.Remove(item);
                    
                    // Toplam tutarı güncelle
                    CalculateTotalAmount();
                    
                    Console.WriteLine($"Ürün sepetten kaldırıldı. Kalan ürün sayısı: {CurrentSaleItems.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün kaldırılırken hata oluştu: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.StackTrace}");
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
                    SearchProductsAsync();
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
                    ProductSearchText = product.Name;
                    Quantity = 1;
                    IsProductSearchOpen = false;
                    Console.WriteLine($"Ürün seçildi: {product.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün seçimi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchProductsAsync()
        {
            try
            {
                // _products koleksiyonunu ilk yüklemede veya boşsa doldur
                if (_products == null || !_products.Any())
                {
                    _products = new ObservableCollection<Product>(await _productService.GetAllProductsAsync());
                }

                if (string.IsNullOrWhiteSpace(ProductSearchText))
                {
                    Products = new ObservableCollection<Product>(_products);
                    IsProductSearchOpen = true;
                    return;
                }

                // Veritabanından arama yap
                var searchedProducts = await _productService.SearchProductsAsync(ProductSearchText);
                Products = new ObservableCollection<Product>(searchedProducts);

                // Arama sonuçlarını _products koleksiyonuna da ekle
                foreach (var product in searchedProducts)
                {
                    if (!_products.Any(p => p.Id == product.Id))
                    {
                        _products.Add(product);
                    }
                }

                // Popup'ı her durumda aç
                IsProductSearchOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün arama hatası: {ex.Message}");
                MessageBox.Show($"Ürün arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalAmount()
        {
            try
            {
                if (CurrentSale != null && CurrentSaleItems != null && CurrentSaleItems.Any())
                {
                    // Toplam tutarı hesapla
                    decimal total = CurrentSaleItems.Sum(i => i.TotalPrice);
                    
                    // CurrentSale'e toplam tutarı ata
                    CurrentSale.TotalAmount = total;
                    
                    // Güncellendiğini bildir
                    OnPropertyChanged(nameof(CurrentSale));
                    
                    // Debug amaçlı konsola yazdır
                    Console.WriteLine($"Toplam tutar hesaplandı: {total:C2} - {CurrentSaleItems.Count} ürün");
                }
                else if (CurrentSale != null)
                {
                    // Hiç ürün yoksa toplam sıfırla
                    CurrentSale.TotalAmount = 0;
                    OnPropertyChanged(nameof(CurrentSale));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Toplam tutar hesaplanırken hata oluştu: {ex.Message}");
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

        private void CancelRefund(object obj)
        {
            IsRefundDialogOpen = false;
            SelectedSale = null;
            SelectedSaleItem = null;
        }

        private void CloseSaleDetails(object obj)
        {
            IsSaleDetailsDialogOpen = false;
            SelectedSale = null;
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
                    SearchCustomersAsync();
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
                        Console.WriteLine($"Müşteri satışa atandı: {customer.FirstName} {customer.LastName}, ID: {customer.Id}");
                    }
                    else
                    {
                        Console.WriteLine("HATA: CurrentSale null, yeni bir satış oluşturuluyor...");
                        CurrentSale = new Payment
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            Customer = customer,
                            CreatedDate = DateTime.Now,
                            PaymentMethod = PaymentMethod.Cash,
                            PaymentType = PaymentType.Sale,
                            PaymentItems = new List<PaymentItem>()
                        };
                    }
                    
                    // Popup'ı kapat
                    IsCustomerSearchOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Müşteri seçimi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Müşteri seçim hatası: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                Console.WriteLine("Veri yükleme işlemi başladı...");
                
                // Sales koleksiyonunu hazırla
                if (Sales == null)
                {
                    Sales = new ObservableCollection<Payment>();
                }
                else
                {
                    Sales.Clear();
                }
                
                // Products koleksiyonunu hazırla
                if (Products == null)
                {
                    _products = new ObservableCollection<Product>();
                    Products = new ObservableCollection<Product>();
                }
                else
                {
                    Products.Clear();
                }
                
                // Customers koleksiyonunu hazırla
                if (Customers == null)
                {
                    _customers = new ObservableCollection<Customer>();
                    Customers = new ObservableCollection<Customer>();
                }
                else
                {
                    Customers.Clear();
                }
                
                // FilteredSales koleksiyonunu hazırla
                if (FilteredSales == null)
                {
                    _filteredSales = new ObservableCollection<Payment>();
                    FilteredSales = new ObservableCollection<Payment>();
                }
                else
                {
                    FilteredSales.Clear();
                }
                
                // Satışları getir
                try
                {
                    var sales = await _saleService.GetAllSalesAsync();
                    if (sales != null && sales.Count > 0)
                    {
                        foreach (var sale in sales)
                        {
                            Sales.Add(sale);
                        }
                        Console.WriteLine($"{sales.Count} adet satış yüklendi.");
                    }
                    else
                    {
                        Console.WriteLine("Hiç satış verisi bulunamadı.");
                    }
                    
                    // FilteredSales'i Sales ile güvenli bir şekilde senkronize et
                    FilteredSales = new ObservableCollection<Payment>(Sales);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Satış verileri yüklenirken hata: {ex.Message}");
                    Console.WriteLine($"İç hata: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Ürünleri getir
                try
                {
                    var products = await _productService.GetAllProductsAsync();
                    
                    if (products != null && products.Count > 0)
                    {
                        foreach (var product in products)
                        {
                            Products.Add(product);
                        }
                        Console.WriteLine($"{products.Count} adet ürün yüklendi.");
                    }
                    else
                    {
                        Console.WriteLine("Hiç ürün verisi bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ürün verileri yüklenirken hata: {ex.Message}");
                    Console.WriteLine($"İç hata: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Müşterileri getir
                try
                {
                    var customers = await _customerService.GetAllCustomersAsync();
                    
                    if (customers != null && customers.Count > 0)
                    {
                        foreach (var customer in customers)
                        {
                            Customers.Add(customer);
                        }
                        Console.WriteLine($"{customers.Count} adet müşteri yüklendi.");
                    }
                    else
                    {
                        Console.WriteLine("Hiç müşteri verisi bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Müşteri verileri yüklenirken hata: {ex.Message}");
                    Console.WriteLine($"İç hata: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Satış türlerini başlat
                _paymentTypes = new ObservableCollection<PaymentType>
                {
                    PaymentType.Sale,
                    PaymentType.Refund
                };
                
                // Ödeme yöntemlerini başlat
                _paymentMethods = new ObservableCollection<PaymentMethod>
                {
                    PaymentMethod.Cash,
                    PaymentMethod.CreditCard,
                    PaymentMethod.BankTransfer
                };
                
                Console.WriteLine("Veri yükleme işlemi tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri yükleme sırasında kritik hata: {ex.Message}");
                Console.WriteLine($"İç hata: {ex.InnerException?.Message}");
                Console.WriteLine($"Yığın izi: {ex.StackTrace}");
                
                // Kritik hata durumunda koleksiyonları yeniden başlat
                InitializeCollections();
                
                // Hata mesajını kullanıcıya göster
                MessageBox.Show($"Veri yüklenirken bir hata oluştu: {ex.Message}\n\nLütfen tekrar deneyin veya uygulamayı yeniden başlatın.",
                    "Veri Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // Koleksiyonları başlatmak için yardımcı metot
        private void InitializeCollections()
        {
            try
            {
                // Tüm koleksiyonları boş olarak başlat
                if (Sales == null) Sales = new ObservableCollection<Payment>();
                if (FilteredSales == null) FilteredSales = new ObservableCollection<Payment>();
                if (Products == null) Products = new ObservableCollection<Product>();
                if (Customers == null) Customers = new ObservableCollection<Customer>();
                if (CurrentSaleItems == null) CurrentSaleItems = new ObservableCollection<PaymentItem>();
                
                // Arama değişkenlerini sıfırla
                _productSearchText = string.Empty;
                _customerSearchText = string.Empty;
                
                // Dialog durumlarını kapat
                IsNewSaleDialogOpen = false;
                IsRefundDialogOpen = false;
                IsProductSearchOpen = false;
                IsSaleDetailsDialogOpen = false;
                IsCustomerSearchOpen = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Koleksiyonlar başlatılırken hata: {ex.Message}");
            }
        }

        private async void SearchCustomersAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomerSearchText))
                {
                    if (_customers != null)
                        Customers = new ObservableCollection<Customer>(_customers);
                    return;
                }

                // Veritabanından arama yap
                Customers = await _customerService.SearchCustomersAsync(CustomerSearchText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri arama hatası: {ex.Message}");
            }
        }

        private async Task RefreshSalesAsync()
        {
            try
            {
                IsLoading = true;
                
                // Veritabanından tüm satışları getir
                var allSales = await _saleService.GetSalesAsync();
                Sales = new ObservableCollection<Payment>(allSales);
                
                // İstatistikleri güncelle
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satışlar yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void FilterSales(object obj)
        {
            try
            {
                IsLoading = true;
                
                // Müşteriye göre filtrele
                if (SelectedCustomer != null && SelectedCustomer.Id != Guid.Empty)
                {
                    // Müşteri ve tarih filtresine göre satışları getir
                    Sales = new ObservableCollection<Payment>(
                        await _saleService.GetSalesByCustomerAndDateRangeAsync(SelectedCustomer.Id, StartDate, EndDate)
                    );
                }
                else
                {
                    // Sadece tarih filtresine göre satışları getir
                    Sales = new ObservableCollection<Payment>(
                        await _saleService.GetSalesByDateRangeAsync(StartDate, EndDate)
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satışlar filtrelenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetFilter(object obj)
        {
            try
            {
                // Filtreleri sıfırla
                SelectedCustomer = null;
                StartDate = DateTime.Now.AddMonths(-1);
                EndDate = DateTime.Now;
                
                // Tüm satışları yenile
                RefreshSalesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtreler sıfırlanırken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewSaleDetails(Payment sale)
        {
            if (sale != null)
            {
                try
                {
                    IsLoading = true;
                    
                    // Satış detaylarını getir
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
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async void ProcessRefund(object obj)
        {
            try
            {
                IsLoading = true;
                
                if (SelectedSale == null || SelectedSaleItem == null)
                {
                    MessageBox.Show("Lütfen iade edilecek ürün seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // İade edilecek ürünleri oluştur
                var refundItems = new List<PaymentItem> { SelectedSaleItem };
                
                // İade işlemini gerçekleştir
                bool success = await _saleService.RefundSaleAsync(SelectedSale.Id, refundItems);
                
                if (success)
                {
                    MessageBox.Show("İade işlemi başarıyla gerçekleştirildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Satış listesini yenile
                    await RefreshSalesAsync();
                    
                    // İade formunu kapat
                    IsRefundDialogOpen = false;
                }
                else
                {
                    MessageBox.Show("İade işlemi sırasında bir hata oluştu!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İade işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateStatistics()
        {
            try
            {
                // Toplam satış tutarını hesapla
                if (Sales != null && Sales.Count > 0)
                {
                    TotalSales = Sales.Count;
                    TotalRevenue = Sales.Sum(s => s.TotalAmount);
                    AverageRevenue = TotalRevenue / TotalSales;
                }
                else
                {
                    TotalSales = 0;
                    TotalRevenue = 0;
                    AverageRevenue = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistikler hesaplanırken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 