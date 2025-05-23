using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

namespace Nethesap.Domain.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync();
        Task<Customer> GetCustomerWithTransactionsAsync(Guid customerId);
        Task<decimal> GetCustomerBalanceAsync(Guid customerId);
        Task<IEnumerable<Customer>> GetCustomersByDebtRangeAsync(decimal minDebt, decimal maxDebt);
    }
} 