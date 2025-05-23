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
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki ödemeleri getirir.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Tarih aralığındaki ödemeler</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki ödemelerin toplam tutarını hesaplar.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Toplam ödeme tutarı</returns>
        public async Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .SumAsync(p => p.TotalAmount);
        }

        /// <summary>
        /// Belirtilen ödemeyi tüm detaylarıyla (ödeme kalemleri ve ürün bilgileri) getirir.
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <returns>Ödeme detayları listesi</returns>
        public async Task<IEnumerable<Payment>> GetPaymentsWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                .Where(p => p.Id == paymentId)
                .ToListAsync();
        }
    }
} 