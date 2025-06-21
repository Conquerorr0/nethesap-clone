using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

// Ödeme işlemleri için özel repository interface'i. Müşteri bazlı ödemeler, tarih aralığına göre ödemeler ve toplam tutar hesaplama gibi ödeme işlemlerini tanımlar.
// IRepository<Payment>'dan kalıtım alarak temel CRUD operasyonlarını da içerir.
namespace Nethesap.Domain.IRepositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payment>> GetPaymentsWithDetailsAsync(Guid paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByProductAsync(Guid productId);
        Task<IEnumerable<Payment>> GetUnpaidPaymentsAsync();
        Task<IEnumerable<Payment>> GetUpcomingPaymentsAsync(int daysThreshold);
        Task<decimal> GetTotalUnpaidAmountByCustomerAsync(Guid customerId);
    }
} 