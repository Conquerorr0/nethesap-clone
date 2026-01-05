using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ödeme işlemleri için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, müşteri bazlı ödemeler, tarih aralığına göre ödemeler ve toplam tutar hesaplama işlemlerini gerçekleştirir.
/// IPaymentRepository interface'ini implemente ederek ödeme işlemlerine özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class PaymentRepository : EfRepository<Payment>, IPaymentRepository
    {
        /// <summary>
        /// PaymentRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Belirtilen müşteriye ait tüm ödemeleri ödeme detaylarıyla birlikte getirir.
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Müşterinin ödemeleri ve detayları</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Include(p => p.PaymentItems)
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki tüm ödemeleri getirir.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Tarih aralığındaki ödemeler</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(p => p.PaymentItems)
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki toplam ödeme tutarını hesaplar.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Toplam ödeme tutarı</returns>
        public async Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
                .SumAsync(p => p.TotalAmount);
        }

        /// <summary>
        /// Belirtilen ödemeyi tüm detaylarıyla birlikte getirir.
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <returns>Ödeme detayları</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Transactions)
                .Where(p => p.Id == paymentId)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen ürünün geçmiş satışlarını getirir.
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <returns>Ürünün geçmiş satışları</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsByProductAsync(Guid productId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Transactions)
                .Where(p => p.PaymentItems.Any(pi => pi.ProductId == productId))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ödenmemiş veya kısmen ödenmiş satışları getirir.
        /// </summary>
        /// <returns>Ödenmemiş satışlar</returns>
        public async Task<IEnumerable<Payment>> GetUnpaidPaymentsAsync()
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                .Where(p => !p.IsFullyPaid)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Yaklaşan ödemeleri getirir.
        /// </summary>
        /// <param name="daysThreshold">Gün eşiği</param>
        /// <returns>Yaklaşan ödemeler</returns>
        public async Task<IEnumerable<Payment>> GetUpcomingPaymentsAsync(int daysThreshold)
        {
            var today = DateTime.Today;
            var thresholdDate = today.AddDays(daysThreshold);
            
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                .Where(p => !p.IsFullyPaid && p.DueDate.HasValue && p.DueDate.Value <= thresholdDate && p.DueDate.Value >= today)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Müşterinin toplam ödenmemiş tutarını hesaplar.
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Toplam ödenmemiş tutar</returns>
        public async Task<decimal> GetTotalUnpaidAmountByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && !p.IsFullyPaid)
                .SumAsync(p => p.RemainingAmount);
        }
    }
} 