using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Nethesap.Domain.Entities;

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
        private int _lowStockThreshold = 5;

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

        // Commands
        public ICommand AddProductCommand => _addProductCommand ??= new RelayCommand(OpenAddProductDialog);
        public ICommand SaveProductCommand => _saveProductCommand ??= new RelayCommand(SaveProduct);
        public ICommand CancelAddCommand => _cancelAddCommand ??= new RelayCommand(CancelAdd);
        public ICommand EditProductCommand => _editProductCommand ??= new RelayCommand<Product>(OpenEditProductDialog);
        public ICommand SaveEditCommand => _saveEditCommand ??= new RelayCommand(SaveEditedProduct);
        public ICommand CancelEditCommand => _cancelEditCommand ??= new RelayCommand(CancelEdit);
        public ICommand DeleteProductCommand => _deleteProductCommand ??= new RelayCommand<Product>(DeleteProduct);

        // Constructor
        public ProductsViewModel()
        {
            LoadSampleData();
            NewProduct = new Product();
        }

        // Methods
        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProducts = new ObservableCollection<Product>(Products);
            }
            else
            {
                FilteredProducts = new ObservableCollection<Product>(
                    Products.Where(p => 
                        p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                        p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        p.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }
        }

        private void LoadSampleData()
        {
            // In a real app, this would come from a repository or service
            Products = new ObservableCollection<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Description = "Oyun Bilgisayarı - 16GB RAM, 512GB SSD",
                    Price = 25000.00m,
                    StockQuantity = 12,
                    Barcode = "1234567890",
                    Category = "Elektronik"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Telefon",
                    Description = "Akıllı Telefon - 128GB",
                    Price = 15000.00m,
                    StockQuantity = 3,
                    Barcode = "0987654321",
                    Category = "Elektronik"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Klavye",
                    Description = "Mekanik Klavye - RGB Aydınlatmalı",
                    Price = 1200.00m,
                    StockQuantity = 0,
                    Barcode = "1122334455",
                    Category = "Aksesuar"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Kulaklık",
                    Description = "Kablosuz Kulaklık - Gürültü Engelleyici",
                    Price = 2800.00m,
                    StockQuantity = 7,
                    Barcode = "5566778899",
                    Category = "Aksesuar"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Fare",
                    Description = "Kablosuz Gaming Mouse - 16000 DPI",
                    Price = 750.00m,
                    StockQuantity = 4,
                    Barcode = "9988776655",
                    Category = "Aksesuar"
                }
            };

            FilteredProducts = new ObservableCollection<Product>(Products);
        }

        private void OpenAddProductDialog(object obj)
        {
            NewProduct = new Product();
            IsAddDialogOpen = true;
        }

        private void SaveProduct(object obj)
        {
            // In a real app, this would save to a database
            NewProduct.Id = Guid.NewGuid();
            
            Products.Add(NewProduct);
            FilterProducts();
            
            IsAddDialogOpen = false;
            NewProduct = new Product();
        }

        private void CancelAdd(object obj)
        {
            IsAddDialogOpen = false;
            NewProduct = new Product();
        }

        private void OpenEditProductDialog(Product product)
        {
            SelectedProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Barcode = product.Barcode,
                Category = product.Category,
                PaymentItems = product.PaymentItems
            };
            
            IsEditDialogOpen = true;
        }

        private void SaveEditedProduct(object obj)
        {
            // In a real app, this would update the database
            var existingProduct = Products.FirstOrDefault(p => p.Id == SelectedProduct.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = SelectedProduct.Name;
                existingProduct.Description = SelectedProduct.Description;
                existingProduct.Price = SelectedProduct.Price;
                existingProduct.StockQuantity = SelectedProduct.StockQuantity;
                existingProduct.Barcode = SelectedProduct.Barcode;
                existingProduct.Category = SelectedProduct.Category;
            }
            
            FilterProducts();
            IsEditDialogOpen = false;
        }

        private void CancelEdit(object obj)
        {
            IsEditDialogOpen = false;
        }

        private void DeleteProduct(Product product)
        {
            if (MessageBox.Show(
                $"'{product.Name}' ürününü silmek istediğinize emin misiniz?",
                "Ürün Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // In a real app, this would delete from the database
                Products.Remove(product);
                FilterProducts();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 