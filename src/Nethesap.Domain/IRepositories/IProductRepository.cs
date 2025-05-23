using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

// Ürün yönetimi için özel repository interface'i. Stok takibi, kategori bazlı sorgulama ve barkod bazlı ürün bulma gibi ürüne özel işlemleri tanımlar.
// IRepository<Product>'dan kalıtım alarak temel CRUD operasyonlarını da içerir.
namespace Nethesap.Domain.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<Product> GetProductByBarcodeAsync(string barcode);
        Task UpdateStockAsync(Guid productId, int quantity);
    }
} 