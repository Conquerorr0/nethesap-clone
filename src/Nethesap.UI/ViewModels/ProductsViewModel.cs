using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;
using Nethesap.UI.Commands;
using Nethesap.UI.Services;

namespace Nethesap.UI.ViewModels
{
    public class ProductsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _filteredProducts;
        private string _searchText;
        private bool _isAddDialogOpen;
        private bool _isEditDialogOpen;
        private Product _newProduct;
        private Product _selectedProduct;
        private ICommand _addProductCommand;
        private ICommand _saveProductCommand;
        private ICommand _cancelAddCommand;
        private ICommand _editProductCommand;
        private ICommand _saveEditCommand;
        private ICommand _cancelEditCommand;
        private ICommand _deleteProductCommand;
        private ICommand _viewProductHistoryCommand;
        private int _lowStockThreshold = 5;
        private bool _isLoading;
        private readonly ProductService _productService;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
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
                FilterProducts();
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

        public bool IsEditDialogOpen
        {
            get => _isEditDialogOpen;
            set
            {
                _isEditDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public Product NewProduct
        {
            get => _newProduct;
            set
            {
                _newProduct = value;
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
            }
        }

        public int LowStockThreshold
        {
            get => _lowStockThreshold;
            set
            {
                _lowStockThreshold = value;
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
        public ICommand AddProductCommand => _addProductCommand ??= new RelayCommand(OpenAddProductDialog);
        public ICommand SaveProductCommand => _saveProductCommand ??= new RelayCommand(SaveProduct);
        public ICommand CancelAddCommand => _cancelAddCommand ??= new RelayCommand(CancelAdd);
        public ICommand EditProductCommand => _editProductCommand ??= new RelayCommand<Product>(OpenEditProductDialog);
        public ICommand SaveEditCommand => _saveEditCommand ??= new RelayCommand(SaveEditedProduct);
        public ICommand CancelEditCommand => _cancelEditCommand ??= new RelayCommand(CancelEdit);
        public ICommand DeleteProductCommand => _deleteProductCommand ??= new RelayCommand<Product>(DeleteProduct);
        public ICommand ViewProductHistoryCommand => _viewProductHistoryCommand ??= new RelayCommand<Product>(ViewProductHistory);

        // Constructor
        public ProductsViewModel()
        {
            _productService = new ProductService();
            NewProduct = new Product();
            LoadProducts();
        }

        // Methods
        private async void FilterProducts()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    FilteredProducts = new ObservableCollection<Product>(Products);
                }
                else
                {
                    // Veritabanından arama yap
                    var products = await _productService.SearchProductsAsync(SearchText);
                    FilteredProducts = products;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün filtreleme hatası: {ex.Message}");
            }
        }

        private async void LoadProducts()
        {
            try
            {
                IsLoading = true;
                Products = await _productService.GetAllProductsAsync();
                FilteredProducts = new ObservableCollection<Product>(Products);

                // Stok miktarı düşük ürünleri kontrol et
                var lowStockProducts = await _productService.GetLowStockProductsAsync(LowStockThreshold);
                if (lowStockProducts.Any())
                {
                    var lowStockCount = lowStockProducts.Count;
                    Console.WriteLine($"Dikkat: {lowStockCount} ürünün stok miktarı düşük!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürünleri yükleme hatası: {ex.Message}");
                Products = new ObservableCollection<Product>();
                FilteredProducts = new ObservableCollection<Product>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenAddProductDialog(object obj)
        {
            NewProduct = new Product();
            IsAddDialogOpen = true;
        }

        private async void SaveProduct(object obj)
        {
            try
            {
                IsLoading = true;

                if (NewProduct == null) return;

                // Ürünü veritabanına ekle
                bool success = await _productService.AddProductAsync(NewProduct);

                if (success)
                {
                    // UI'ı güncelle
                    Products.Add(NewProduct);
                    FilteredProducts.Add(NewProduct);
                    IsAddDialogOpen = false;
                    NewProduct = new Product();
                }
                else
                {
                    // Hata durumunu kullanıcıya bildir
                    Console.WriteLine("Ürün eklenemedi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün kaydetme hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelAdd(object obj)
        {
            IsAddDialogOpen = false;
            NewProduct = new Product();
        }

        private void OpenEditProductDialog(Product product)
        {
            if (product == null) return;

            // Orijinal ürünün bir kopyasını oluştur (doğrudan referansı değiştirmemek için)
            SelectedProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Barcode = product.Barcode,
                Category = product.Category
            };

            IsEditDialogOpen = true;
        }

        private async void SaveEditedProduct(object obj)
        {
            try
            {
                IsLoading = true;

                if (SelectedProduct == null) return;

                // Ürünü veritabanında güncelle
                bool success = await _productService.UpdateProductAsync(SelectedProduct);

                if (success)
                {
                    // UI'daki ürünü güncelle
                    var existingProduct = Products.FirstOrDefault(p => p.Id == SelectedProduct.Id);
                    if (existingProduct != null)
                    {
                        int index = Products.IndexOf(existingProduct);
                        Products[index] = SelectedProduct;
                    }

                    // Filtrelenmiş listeyi güncelle
                    FilterProducts();

                    IsEditDialogOpen = false;
                }
                else
                {
                    // Hata durumunu kullanıcıya bildir
                    Console.WriteLine("Ürün güncellenemedi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün güncelleme hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit(object obj)
        {
            IsEditDialogOpen = false;
        }

        private async void DeleteProduct(Product product)
        {
            try
            {
                IsLoading = true;

                if (product == null) return;

                // Kullanıcıya silme işlemini onaylat
                MessageBoxResult result = MessageBox.Show(
                    $"{product.Name} ürününü silmek istediğinize emin misiniz?",
                    "Silme Onayı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Ürünü veritabanından sil
                    bool success = await _productService.DeleteProductAsync(product);

                    if (success)
                    {
                        // UI'dan ürünü kaldır
                        Products.Remove(product);
                        FilterProducts();
                    }
                    else
                    {
                        // Hata durumunu kullanıcıya bildir
                        Console.WriteLine("Ürün silinemedi!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün silme hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewProductHistory(Product product)
        {
            if (product != null)
            {
                var salesViewModel = new SalesViewModel();
                salesViewModel.ViewProductHistory(product);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 