using Nethesap.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Ödeme işlemleri için servis interface'i
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Tüm ödemeleri getirir
        /// </summary>
        Task<List<Payment>> GetAllAsync();

        /// <summary>
        /// ID'ye göre ödeme getirir
        /// </summary>
        Task<Payment> GetByIdAsync(Guid id);

        /// <summary>
        /// Yeni ödeme ekler
        /// </summary>
        Task AddAsync(Payment payment);

        /// <summary>
        /// Ödeme bilgilerini günceller
        /// </summary>
        Task UpdateAsync(Payment payment);

        /// <summary>
        /// Ödemeyi siler (soft delete)
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Müşteriye ait ödemeleri getirir
        /// </summary>
        Task<List<Payment>> GetByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Tarih aralığına göre ödemeleri getirir
        /// </summary>
        Task<List<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödeme detaylarını getirir
        /// </summary>
        Task<List<PaymentItem>> GetPaymentItemsAsync(Guid paymentId);
    }
} 