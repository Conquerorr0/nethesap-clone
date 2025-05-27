using Nethesap.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Ürün işlemleri için servis interface'i
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Tüm ürünleri getirir
        /// </summary>
        Task<List<Product>> GetAllAsync();

        /// <summary>
        /// ID'ye göre ürün getirir
        /// </summary>
        Task<Product> GetByIdAsync(Guid id);

        /// <summary>
        /// Yeni ürün ekler
        /// </summary>
        Task AddAsync(Product product);

        /// <summary>
        /// Ürün bilgilerini günceller
        /// </summary>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Ürünü siler (soft delete)
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Barkoda göre ürün getirir
        /// </summary>
        Task<Product> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Kategoriye göre ürünleri getirir
        /// </summary>
        Task<List<Product>> GetByCategoryAsync(string category);
    }
} 