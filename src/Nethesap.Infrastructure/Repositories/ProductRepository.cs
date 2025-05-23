using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.Repositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nethesap.Infrastructure.Repositories
{
    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _dbSet.Where(p => p.StockQuantity <= threshold).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _dbSet.Where(p => p.Category == category).ToListAsync();
        }

        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        public async Task UpdateStockAsync(Guid productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantity;
                _dbSet.Update(product);
            }
        }
    }
} 