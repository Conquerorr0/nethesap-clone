using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ödeme kalemleri için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, ödeme detayları, ürün bazlı ödemeler ve tarih aralığına göre ödeme kalemleri işlemlerini gerçekleştirir.
/// IPaymentItemRepository interface'ini implemente ederek ödeme kalemlerine özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class PaymentItemRepository : EfRepository<PaymentItem>, IPaymentItemRepository
    {
        /// <summary>
        /// PaymentItemRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public PaymentItemRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Belirtilen ödemeye ait tüm ödeme kalemlerini ürün bilgileriyle birlikte getirir.
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <returns>Ödeme kalemleri listesi</returns>
        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByPaymentAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Where(pi => pi.PaymentId == paymentId)
                .OrderBy(pi => pi.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen ürüne ait tüm ödeme kalemlerini ödeme bilgileriyle birlikte getirir.
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <returns>Ödeme kalemleri listesi</returns>
        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByProductAsync(Guid productId)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.ProductId == productId)
                .OrderByDescending(pi => pi.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen ürünün toplam satış miktarını hesaplar.
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <returns>Toplam satış miktarı</returns>
        public async Task<decimal> GetTotalQuantityByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId)
                .SumAsync(pi => pi.Quantity);
        }

        /// <summary>
        /// Belirtilen ödeme kalemlerini tüm detaylarıyla birlikte getirir.
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <returns>Detaylı ödeme kalemleri listesi</returns>
        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Include(pi => pi.Payment)
                    .ThenInclude(p => p.Customer)
                .Where(pi => pi.PaymentId == paymentId)
                .OrderBy(pi => pi.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen ödemenin toplam tutarını hesaplar.
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <returns>Toplam tutar</returns>
        public async Task<decimal> GetTotalAmountByPaymentAsync(Guid paymentId)
        {
            return await _dbSet
                .Where(pi => pi.PaymentId == paymentId)
                .SumAsync(pi => pi.TotalPrice);
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki ödeme kalemlerini getirir.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Tarih aralığındaki ödeme kalemleri</returns>
        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.CreatedAt >= startDate && pi.Payment.CreatedAt <= endDate)
                .OrderByDescending(pi => pi.Payment.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki ödeme kalemlerinin toplam tutarını hesaplar.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Toplam tutar</returns>
        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.CreatedAt >= startDate && pi.Payment.CreatedAt <= endDate)
                .SumAsync(pi => pi.TotalPrice);
        }

        /// <summary>
        /// Belirtilen müşteriye ait tüm ödeme kalemlerini getirir.
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Müşterinin ödeme kalemleri</returns>
        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.CustomerId == customerId)
                .OrderByDescending(pi => pi.Payment.CreatedAt)
                .ToListAsync();
        }
    }
} 