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
    public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync()
        {
            return await _dbSet.Where(c => c.Balance != 0).ToListAsync();
        }

        public async Task<Customer> GetCustomerWithTransactionsAsync(Guid customerId)
        {
            return await _dbSet
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<decimal> GetCustomerBalanceAsync(Guid customerId)
        {
            var customer = await _dbSet.FindAsync(customerId);
            return customer?.Balance ?? 0;
        }

        public async Task<IEnumerable<Customer>> GetCustomersByDebtRangeAsync(decimal minDebt, decimal maxDebt)
        {
            return await _dbSet
                .Where(c => c.Balance >= minDebt && c.Balance <= maxDebt)
                .ToListAsync();
        }
    }
} 