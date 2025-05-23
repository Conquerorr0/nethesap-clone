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

        // ... mevcut kod ...
    }
} 