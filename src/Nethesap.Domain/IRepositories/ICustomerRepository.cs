using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethesap.Domain.Entities;

// Müşteri yönetimi için özel repository interface'i. Müşteri bakiyesi, işlem geçmişi ve borç/alacak durumu gibi müşteriye özel işlemleri tanımlar.
// IRepository<Customer>'dan kalıtım alarak temel CRUD operasyonlarını da içerir.
namespace Nethesap.Domain.IRepositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync();
        Task<Customer> GetCustomerWithTransactionsAsync(Guid customerId);
        Task<decimal> GetCustomerBalanceAsync(Guid customerId);
        Task<IEnumerable<Customer>> GetCustomersByDebtRangeAsync(decimal minDebt, decimal maxDebt);
    }
} 