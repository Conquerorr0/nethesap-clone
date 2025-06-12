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
    public class ProductService
    {
        private readonly IRepository<Product> _productRepository;

        /// <summary>
        /// ProductService sınıfının constructor'ı
        /// </summary>
        public ProductService()
        {
            var dbContext = new AppDbContext();
            _productRepository = new EfRepository<Product>(dbContext);
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

                var products = await _productRepository.FindAsync(p =>
                    p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Barcode.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                );

                return new ObservableCollection<Product>(products);
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
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün güncellenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
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
    }
} 