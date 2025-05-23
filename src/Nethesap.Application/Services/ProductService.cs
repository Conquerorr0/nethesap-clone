using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Ürün işlemleri için servis implementasyonu
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// ProductService constructor'ı
        /// </summary>
        /// <param name="productRepository">Ürün repository'si</param>
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return new List<Product>(products);
        }

        /// <inheritdoc/>
        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                await _productRepository.RemoveAsync(product);
            }
        }

        /// <inheritdoc/>
        public async Task<Product> GetByBarcodeAsync(string barcode)
        {
            var products = await _productRepository.FindAsync(p => p.Barcode == barcode);
            return products.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            var products = await _productRepository.FindAsync(p => p.Category == category);
            return new List<Product>(products);
        }
    }
} 