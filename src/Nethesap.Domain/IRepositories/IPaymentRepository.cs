using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

namespace Nethesap.Domain.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payment>> GetPaymentsWithDetailsAsync(Guid paymentId);
    }
} 