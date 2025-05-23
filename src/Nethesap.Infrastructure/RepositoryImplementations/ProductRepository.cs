using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ürün yönetimi için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, stok takibi, kategori bazlı sorgulama ve barkod bazlı ürün bulma işlemlerini gerçekleştirir.
/// IProductRepository interface'ini implemente ederek ürüne özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        /// <summary>
        /// ProductRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Stok miktarı belirtilen eşik değerinin altında olan ürünleri getirir.
        /// </summary>
        /// <param name="threshold">Eşik değeri</param>
        /// <returns>Düşük stoklu ürünler listesi</returns>
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _dbSet
                .Where(p => p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen kategorideki ürünleri getirir.
        /// </summary>
        /// <param name="category">Kategori adı</param>
        /// <returns>Kategorideki ürünler listesi</returns>
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(p => p.Category == category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Barkod numarasına göre ürün getirir.
        /// </summary>
        /// <param name="barcode">Barkod numarası</param>
        /// <returns>Ürün bilgisi</returns>
        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        /// <summary>
        /// Ürün stok miktarını günceller.
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <param name="quantity">Eklenecek/Çıkarılacak miktar</param>
        public async Task UpdateStockAsync(Guid productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantity;
                await _context.SaveChangesAsync();
            }
        }
    }
} 