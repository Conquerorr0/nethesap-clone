using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using Nethesap.Infrastructure.RepositoryImplementations;

namespace Nethesap.UI.Services
{
    /// <summary>
    /// Ürün verilerini yönetmek için kullanılan servis sınıfı
    /// </summary>
    public class ProductService : IDisposable
    {
        private readonly IRepository<Product> _productRepository;
        private readonly AppDbContext _dbContext;
        private bool _disposed = false;

        /// <summary>
        /// ProductService sınıfının constructor'ı
        /// </summary>
        public ProductService()
        {
            _dbContext = new AppDbContext();
            _productRepository = new EfRepository<Product>(_dbContext);
        }

        /// <summary>
        /// Tüm ürünleri getirir
        /// </summary>
        /// <returns>Ürün listesi</returns>
        public async Task<ObservableCollection<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürünler getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Product>();
            }
        }

        /// <summary>
        /// İsme göre ürün arar
        /// </summary>
        /// <param name="searchText">Aranacak metin</param>
        /// <returns>Aranan isme göre filtrelenmiş ürün listesi</returns>
        public async Task<ObservableCollection<Product>> SearchProductsAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return await GetAllProductsAsync();
                }

                // Arama metnini küçük harfe çevir
                searchText = searchText.ToLower();

                // Veritabanında arama yap
                var products = await _productRepository.FindAsync(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    p.Barcode.ToLower().Contains(searchText) ||
                    p.Description.ToLower().Contains(searchText) ||
                    p.Category.ToLower().Contains(searchText)
                );

                // Sonuçları sırala (önce tam eşleşmeler, sonra kısmi eşleşmeler)
                var sortedProducts = products.OrderBy(p =>
                {
                    if (p.Name.ToLower() == searchText || p.Barcode.ToLower() == searchText)
                        return 0;
                    if (p.Name.ToLower().StartsWith(searchText))
                        return 1;
                    return 2;
                }).ToList();

                return new ObservableCollection<Product>(sortedProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün araması yapılırken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Product>();
            }
        }

        /// <summary>
        /// ID'ye göre ürün getirir
        /// </summary>
        /// <param name="id">Ürün ID'si</param>
        /// <returns>Bulunan ürün veya null</returns>
        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            try
            {
                return await _productRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Barkod numarasına göre ürün getirir
        /// </summary>
        /// <param name="barcode">Ürün barkodu</param>
        /// <returns>Bulunan ürün veya null</returns>
        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    return null;
                }

                var product = await _productRepository.SingleOrDefaultAsync(p => p.Barcode == barcode);
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Barkod ile ürün aranırken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Yeni ürün ekler
        /// </summary>
        /// <param name="product">Eklenecek ürün</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                if (product.Id == Guid.Empty)
                {
                    product.Id = Guid.NewGuid();
                }

                product.CreatedAt = DateTime.UtcNow;
                await _productRepository.AddAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün eklenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ürün günceller
        /// </summary>
        /// <param name="product">Güncellenecek ürün</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                if (product == null || product.Id == Guid.Empty)
                {
                    Console.WriteLine("Ürün güncelleme hatası: Ürün veya ID geçersiz!");
                    return false;
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
                
                // Değişikliklerin kalıcı olduğundan emin olmak için context'i temizle
                _dbContext.ChangeTracker.Clear();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün güncellenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Ürün siler (soft delete)
        /// </summary>
        /// <param name="product">Silinecek ürün</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> DeleteProductAsync(Product product)
        {
            try
            {
                await _productRepository.RemoveAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün silinirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stok miktarı düşük olan ürünleri getirir
        /// </summary>
        /// <param name="threshold">Düşük stok eşiği</param>
        /// <returns>Stok miktarı düşük olan ürünlerin listesi</returns>
        public async Task<ObservableCollection<Product>> GetLowStockProductsAsync(int threshold = 5)
        {
            try
            {
                var products = await _productRepository.FindAsync(p => p.StockQuantity <= threshold);
                return new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Düşük stoklu ürünler getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Product>();
            }
        }

        /// <summary>
        /// IDisposable implementasyonu - kaynakları temizler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }
    }
} 