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
        private ICommand? _openPartialPaymentDialogCommand;
        private ICommand? _makePartialPaymentCommand;
        private ICommand? _cancelPartialPaymentCommand;
        private ICommand? _viewProductHistoryCommand;
        private ICommand? _closeProductHistoryCommand;
        private ICommand? _viewPaymentHistoryCommand;
        private ICommand? _closePaymentHistoryCommand;
        private ICommand? _loadUnpaidSalesCommand;
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

        private ObservableCollection<Payment> _productSaleHistory = new ObservableCollection<Payment>();
        private ObservableCollection<Transaction> _paymentTransactions = new ObservableCollection<Transaction>();
        private ObservableCollection<Payment> _unpaidSales = new ObservableCollection<Payment>();
        
        private bool _isPartialPaymentDialogOpen;
        private bool _isProductHistoryDialogOpen;
        private bool _isPaymentHistoryDialogOpen;
        
        private decimal _paidAmount;
        private decimal _remainingAmount;
        private Product _selectedHistoryProduct;
        private Payment _selectedUnpaidSale;

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
                if (_customerSearchText == value) return;
                _customerSearchText = value;
                OnPropertyChanged();

                // Arama metni değiştiğinde servisten arama yap
                SearchCustomersAsync();
                
                // Popup'ı aç
                IsCustomerSearchOpen = true;
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

        public ObservableCollection<Payment> ProductSaleHistory
        {
            get => _productSaleHistory;
            set
            {
                _productSaleHistory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Transaction> PaymentTransactions
        {
            get => _paymentTransactions;
            set
            {
                _paymentTransactions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Payment> UnpaidSales
        {
            get => _unpaidSales;
            set
            {
                _unpaidSales = value;
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

        public bool IsProductHistoryDialogOpen
        {
            get => _isProductHistoryDialogOpen;
            set
            {
                _isProductHistoryDialogOpen = value;
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

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (_paidAmount != value)
                {
                    _paidAmount = value;
                    OnPropertyChanged();
                    
                    if (CurrentSale != null)
                    {
                        RemainingAmount = CurrentSale.TotalAmount - _paidAmount;
                    }
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

        public Product SelectedHistoryProduct
        {
            get => _selectedHistoryProduct;
            set
            {
                _selectedHistoryProduct = value;
                OnPropertyChanged();
                if (value != null)
                {
                    LoadProductHistory();
                }
            }
        }

        public Payment SelectedUnpaidSale
        {
            get => _selectedUnpaidSale;
            set
            {
                _selectedUnpaidSale = value;
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
        public ICommand OpenPartialPaymentDialogCommand => _openPartialPaymentDialogCommand ??= new RelayCommand<Payment>(OpenPartialPaymentDialog);
        public ICommand MakePartialPaymentCommand => _makePartialPaymentCommand ??= new RelayCommand(MakePartialPayment);
        public ICommand CancelPartialPaymentCommand => _cancelPartialPaymentCommand ??= new RelayCommand(CancelPartialPayment);
        public ICommand ViewProductHistoryCommand => _viewProductHistoryCommand ??= new RelayCommand<Product>(ViewProductHistory);
        public ICommand CloseProductHistoryCommand => _closeProductHistoryCommand ??= new RelayCommand(CloseProductHistory);
        public ICommand ViewPaymentHistoryCommand => _viewPaymentHistoryCommand ??= new RelayCommand<Payment>(ViewPaymentHistory);
        public ICommand ClosePaymentHistoryCommand => _closePaymentHistoryCommand ??= new RelayCommand(ClosePaymentHistory);
        public ICommand LoadUnpaidSalesCommand => _loadUnpaidSalesCommand ??= new RelayCommand(LoadUnpaidSales);

        // Constructor
        public SalesViewModel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SalesViewModel CONSTRUCTOR BAŞLADI ===");
                Console.WriteLine("=== SalesViewModel CONSTRUCTOR BAŞLADI ===");
                
                // Servisleri başlat
                _productService = new ProductService();
                _customerService = new CustomerService();
                _customerService.CustomerAdded += OnCustomerAdded;
                _saleService = new SaleService();
                
                System.Diagnostics.Debug.WriteLine("Servisler başlatıldı");
                Console.WriteLine("Servisler başlatıldı");
                
                // Veritabanı bağlantısını basit şekilde test et (veri çekmeden)
                try
                {
                    Console.WriteLine("Veritabanı bağlantısı test ediliyor...");
                    using (var context = new Nethesap.Infrastructure.Data.AppDbContext())
                    {
                        bool canConnect = context.Database.CanConnect();
                        Console.WriteLine($"Veritabanı bağlantısı: {(canConnect ? "BAŞARILI" : "BAŞARISIZ")}");
                        
                        // Veri çekme kısmını kaldırdık çünkü burada exception atıyordu
                        if (canConnect)
                        {
                            Console.WriteLine("Veritabanı hazır, veri yükleme LoadDataAsync() ile yapılacak");
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Veritabanı bağlantı hatası: {dbEx.Message}");
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                }
                
                // Koleksiyonları başlat
                InitializeCollections();
                
                // Tarih filtre değerlerini ayarla
                StartDate = DateTime.Now.AddMonths(-1);
                EndDate = DateTime.Now;
                
                // Veri yüklemeyi arka planda başlat (UI thread güvenli)
                Task.Run(async () =>
                {
                    try
                    {
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Arka plan veri yükleme hatası: {ex.Message}");
                    }
                });
                
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

        private async void OnCustomerAdded(Nethesap.Domain.Entities.Customer customer)
        {
            // Yeni müşteri hem _customers'a hem Customers'a eklenmeli
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _customers.Add(customer);
                Customers.Add(customer);
            });
        }

        private async Task RefreshCustomersAsync()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _customers.Clear();
                    Customers.Clear();
                    foreach (var c in customers)
                    {
                        _customers.Add(c);
                        Customers.Add(c);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri listesi güncellenirken hata oluştu: {ex.Message}");
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

                // Ödeme alanlarını sıfırla
                PaidAmount = 0;
                RemainingAmount = 0;
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
            IsLoading = true;
            
            System.Diagnostics.Debug.WriteLine("=== SATIŞ KAYDETME İŞLEMİ BAŞLADI ===");
            Console.WriteLine("=== SATIŞ KAYDETME İŞLEMİ BAŞLADI ===");
            
            try
            {
                // Basit kontroller
                if (CurrentSale == null)
                {
                    MessageBox.Show("Satış bilgileri bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsLoading = false;
                    return;
                }
                
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Lütfen bir müşteri seçiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsLoading = false;
                    return;
                }
                
                if (CurrentSaleItems?.Count == 0)
                {
                    MessageBox.Show("Lütfen sepete ürün ekleyiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsLoading = false;
                    return;
                }
                
                Console.WriteLine($"Müşteri: {SelectedCustomer.FirstName} {SelectedCustomer.LastName}");
                Console.WriteLine($"Toplam ürün sayısı: {CurrentSaleItems.Count}");
                Console.WriteLine($"Toplam tutar: {CurrentSaleItems.Sum(item => item.TotalPrice):C2}");
                
                // CurrentSale'ı hazırla
                CurrentSale.CustomerId = SelectedCustomer.Id;
                CurrentSale.CreatedDate = DateTime.Now;
                CurrentSale.PaymentType = PaymentType.Sale;
                CurrentSale.PaymentMethod = PaymentMethod.Cash;
                CurrentSale.PaymentItems = new List<PaymentItem>(CurrentSaleItems);
                CurrentSale.TotalAmount = CurrentSaleItems.Sum(item => item.TotalPrice);
                
                // Kullanıcının belirttiği ödenen tutarı doğrula
                var paidAmount = PaidAmount;
                
                // Eğer kullanıcı ödeme alanını boş bırakırsa (0 veya negatif), borca yaz (hiç ödeme yok)
                if (paidAmount <= 0)
                {
                    paidAmount = 0;
                }
                
                // Toplamdan fazla ödeme yapılmasına izin verme
                if (paidAmount > CurrentSale.TotalAmount)
                {
                    paidAmount = CurrentSale.TotalAmount;
                }
                
                CurrentSale.PaidAmount = paidAmount;
                CurrentSale.RemainingAmount = CurrentSale.TotalAmount - paidAmount;
                if (CurrentSale.RemainingAmount < 0)
                {
                    CurrentSale.RemainingAmount = 0;
                }
                CurrentSale.IsFullyPaid = CurrentSale.RemainingAmount == 0;
                
                // ViewModel tarafındaki kalan tutarı da güncelle
                RemainingAmount = CurrentSale.RemainingAmount;
                
                // PaymentItems için ID'leri kontrol et
                foreach (var item in CurrentSale.PaymentItems)
                {
                    if (item.Id == Guid.Empty)
                        item.Id = Guid.NewGuid();
                    
                    Console.WriteLine($"Satış kalemi: {item.Product?.Name ?? "Bilinmeyen"} - Adet: {item.Quantity} - Fiyat: {item.TotalPrice:C2}");
                }
                
                // Satışı kaydet
                Console.WriteLine("SaleService.AddSaleAsync çağrılıyor...");
                bool result = await _saleService.AddSaleAsync(CurrentSale);
                
                if (result)
                {
                    MessageBox.Show("Satış başarıyla kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsNewSaleDialogOpen = false;
                    await RefreshSalesAsync();
                    
                    // Temizle
                    CurrentSale = null;
                    CurrentSaleItems?.Clear();
                    SelectedCustomer = null;
                    SelectedProduct = null;
                    Quantity = 1;
                    PaidAmount = 0;
                    RemainingAmount = 0;
                    
                    Console.WriteLine("Satış kaydetme işlemi tamamlandı ve veriler temizlendi.");
                }
                else
                {
                    MessageBox.Show("Satış kaydedilemedi! Lütfen tekrar deneyiniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveSale metodunda hata: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                MessageBox.Show($"Satış kaydedilemedi!\n\nHata: {ex.Message}\n\nDetay: {ex.InnerException?.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        Product = SelectedProduct, // Product nesnesini ekle, UI'da gösterim için gerekli
                        Quantity = Quantity,
                        UnitPrice = SelectedProduct.Price,
                        TotalPrice = SelectedProduct.Price * Quantity
                    };

                    CurrentSaleItems.Add(paymentItem);
                    Console.WriteLine($"Yeni ürün eklendi: {SelectedProduct.Name}, Miktar: {paymentItem.Quantity}");
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
                    
                    // Kalan tutarı güncelle
                    RemainingAmount = CurrentSale.TotalAmount - PaidAmount;
                    if (RemainingAmount < 0)
                    {
                        RemainingAmount = 0;
                    }
                    
                    // Debug amaçlı konsola yazdır
                    Console.WriteLine($"Toplam tutar hesaplandı: {total:C2} - {CurrentSaleItems.Count} ürün");
                }
                else if (CurrentSale != null)
                {
                    // Hiç ürün yoksa toplam sıfırla
                    CurrentSale.TotalAmount = 0;
                    OnPropertyChanged(nameof(CurrentSale));
                    RemainingAmount = 0;
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
                        // UI Thread'de Collection'lara erişim
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var sale in sales)
                            {
                                Sales.Add(sale);
                            }
                            // FilteredSales'i Sales ile güvenli bir şekilde senkronize et
                            FilteredSales = new ObservableCollection<Payment>(Sales);
                        });
                        Console.WriteLine($"{sales.Count} adet satış yüklendi.");
                    }
                    else
                    {
                        Console.WriteLine("Hiç satış verisi bulunamadı.");
                    }
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
                        // UI Thread'de Collection'lara erişim
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var product in products)
                            {
                                Products.Add(product);
                            }
                        });
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
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _customers.Clear();
                            Customers.Clear();
                            foreach (var c in customers)
                            {
                                _customers.Add(c);
                                Customers.Add(c);
                            }
                        });
                        Console.WriteLine($"{customers.Count} adet müşteri yüklendi.");
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _customers = new ObservableCollection<Customer>();
                            Customers = new ObservableCollection<Customer>();
                        });
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

        private void OpenPartialPaymentDialog(Payment payment)
        {
            if (payment != null)
            {
                SelectedSale = payment;
                SelectedUnpaidSale = payment;
                PaidAmount = 0;
                RemainingAmount = payment.RemainingAmount;
                IsPartialPaymentDialogOpen = true;
            }
        }

        private void CancelPartialPayment(object obj)
        {
            IsPartialPaymentDialogOpen = false;
        }

        public async void ViewProductHistory(Product product)
        {
            if (product == null) return;

            SelectedHistoryProduct = product;
            await LoadProductHistory();
            IsProductHistoryDialogOpen = true;
        }

        private async Task LoadProductHistory()
        {
            if (SelectedHistoryProduct == null) return;
            
            ProductSaleHistory = new ObservableCollection<Payment>(
                await _saleService.GetProductPaymentHistoryAsync(SelectedHistoryProduct.Id)
            );
        }

        private void CloseProductHistory(object obj)
        {
            IsProductHistoryDialogOpen = false;
        }

        private async void ViewPaymentHistory(Payment payment)
        {
            if (payment != null)
            {
                // Ödeme detaylarını tekrar veritabanından al, çünkü transactions eksik olabilir
                var fullPayment = await _saleService.GetSaleDetailsAsync(payment.Id);
                
                if (fullPayment != null)
                {
                    SelectedSale = fullPayment;
                    // İşlemleri tarihe göre sıralayarak göster
                    var sortedTransactions = (fullPayment.Transactions ?? new List<Transaction>())
                        .OrderByDescending(t => t.TransactionDate)
                        .ToList();
                    PaymentTransactions = new ObservableCollection<Transaction>(sortedTransactions);
                    IsPaymentHistoryDialogOpen = true;
                }
                else
                {
                    MessageBox.Show("Ödeme detayları yüklenemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClosePaymentHistory(object obj)
        {
            IsPaymentHistoryDialogOpen = false;
        }

        private async Task LoadUnpaidSalesAsync()
        {
            try
            {
                IsLoading = true;
                
                var unpaidSales = await _saleService.GetUnpaidSalesAsync();
                UnpaidSales = new ObservableCollection<Payment>(unpaidSales);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödenmemiş satışlar yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void LoadUnpaidSales(object obj)
        {
            await LoadUnpaidSalesAsync();
        }

        // Method to refresh product data from database
        public async Task RefreshProductDataAsync()
        {
            try
            {
                // Reload products from database
                _products = new ObservableCollection<Product>(await _productService.GetAllProductsAsync());
                
                // If there's a search text, apply the filter
                if (!string.IsNullOrWhiteSpace(ProductSearchText))
                {
                    var searchedProducts = await _productService.SearchProductsAsync(ProductSearchText);
                    Products = new ObservableCollection<Product>(searchedProducts);
                }
                else
                {
                    // Otherwise show all products
                    Products = new ObservableCollection<Product>(_products);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün verileri yenilenirken hata oluştu: {ex.Message}");
            }
        }

        private async void MakePartialPayment(object obj)
        {
            try
            {
                if (SelectedUnpaidSale == null || PaidAmount <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir ödeme tutarı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _saleService.AddPartialPaymentAsync(SelectedUnpaidSale.Id, PaidAmount, PaymentMethod.Cash);

                if (result)
                {
                    MessageBox.Show("Ödeme başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Satış listesini ve ödenmemiş satışları yenile
                    await RefreshSalesAsync();
                    await LoadUnpaidSalesAsync();
                    
                    IsPartialPaymentDialogOpen = false;
                    PaidAmount = 0;
                }
                else
                {
                    MessageBox.Show("Ödeme kaydedilirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Test metodu - Database bağlantısını kontrol etmek için
        private async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                Console.WriteLine("=== DATABASE CONNECTION TEST BAŞLADI ===");
                
                // Test 1: Basic connection
                using (var context = new Nethesap.Infrastructure.Data.AppDbContext())
                {
                    bool canConnect = context.Database.CanConnect();
                    Console.WriteLine($"Can Connect: {canConnect}");
                    
                    if (!canConnect)
                    {
                        Console.WriteLine("HATA: Database'e bağlanılamıyor!");
                        return false;
                    }
                }
                
                // Test 2: Service test
                try
                {
                    var customers = await _customerService.GetAllCustomersAsync();
                    Console.WriteLine($"Müşteri servisi çalışıyor. Müşteri sayısı: {customers.Count}");
                    
                    var products = await _productService.GetAllProductsAsync();
                    Console.WriteLine($"Ürün servisi çalışıyor. Ürün sayısı: {products.Count}");
                    
                    Console.WriteLine("=== DATABASE CONNECTION TEST BAŞARILI ===");
                    return true;
                }
                catch (Exception serviceEx)
                {
                    Console.WriteLine($"Servis testi başarısız: {serviceEx.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection test hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
} 