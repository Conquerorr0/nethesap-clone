using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.Repositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nethesap.Infrastructure.Repositories
{
    public class PaymentRepository : EfRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .SumAsync(p => p.TotalAmount);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Customer)
                .Where(p => p.Id == paymentId)
                .ToListAsync();
        }
    }
} 