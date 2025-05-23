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
    public class PaymentItemRepository : EfRepository<PaymentItem>, IPaymentItemRepository
    {
        public PaymentItemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByPaymentAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Where(pi => pi.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByProductAsync(Guid productId)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalQuantityByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId)
                .SumAsync(pi => pi.Quantity);
        }

        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Include(pi => pi.Payment)
                    .ThenInclude(p => p.Customer)
                .Where(pi => pi.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByPaymentAsync(Guid paymentId)
        {
            return await _dbSet
                .Where(pi => pi.PaymentId == paymentId)
                .SumAsync(pi => pi.TotalPrice);
        }

        public async Task<IEnumerable<PaymentItem>> GetPaymentItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.CreatedAt >= startDate && pi.Payment.CreatedAt <= endDate)
                .OrderByDescending(pi => pi.Payment.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.CreatedAt >= startDate && pi.Payment.CreatedAt <= endDate)
                .SumAsync(pi => pi.TotalPrice);
        }

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