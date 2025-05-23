using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

namespace Nethesap.Domain.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<Product> GetProductByBarcodeAsync(string barcode);
        Task UpdateStockAsync(Guid productId, int quantity);
    }
} 