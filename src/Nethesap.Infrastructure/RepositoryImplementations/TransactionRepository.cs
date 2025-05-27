using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nethesap.Infrastructure.RepositoryImplementations
{
    /// <summary>
    /// İşlem (Transaction) repository implementasyonu
    /// </summary>
    public class TransactionRepository : EfRepository<Transaction>, ITransactionRepository
    {
        /// <summary>
        /// TransactionRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<List<Transaction>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _dbSet
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<decimal> GetCustomerBalanceAsync(Guid customerId)
        {
            return await _dbSet
                .Where(t => t.CustomerId == customerId)
                .SumAsync(t => t.Amount);
        }
    }
} 