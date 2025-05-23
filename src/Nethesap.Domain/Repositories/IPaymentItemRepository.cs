using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

namespace Nethesap.Domain.Repositories
{
    public interface IPaymentItemRepository : IRepository<PaymentItem>
    {
        Task<IEnumerable<PaymentItem>> GetPaymentItemsByPaymentAsync(Guid paymentId);
        Task<IEnumerable<PaymentItem>> GetPaymentItemsByProductAsync(Guid productId);
        Task<decimal> GetTotalQuantityByProductAsync(Guid productId);
        Task<IEnumerable<PaymentItem>> GetPaymentItemsWithDetailsAsync(Guid paymentId);
        Task<decimal> GetTotalAmountByPaymentAsync(Guid paymentId);
        Task<IEnumerable<PaymentItem>> GetPaymentItemsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentItem>> GetPaymentItemsByCustomerAsync(Guid customerId);
    }
} 