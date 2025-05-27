using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Müşteri yönetimi için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, müşteri bakiyesi, işlem geçmişi ve borç/alacak durumu işlemlerini gerçekleştirir.
/// ICustomerRepository interface'ini implemente ederek müşteriye özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        /// <summary>
        /// CustomerRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Bakiyesi sıfırdan farklı olan müşterileri getirir.
        /// </summary>
        /// <returns>Bakiyesi olan müşteriler listesi</returns>
        public async Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync()
        {
            return await _dbSet
                .Where(c => c.Balance != 0)
                .OrderByDescending(c => Math.Abs(c.Balance))
                .ToListAsync();
        }

        /// <summary>
        /// Müşteriyi işlem geçmişiyle birlikte getirir.
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>İşlem geçmişi ile birlikte müşteri bilgisi</returns>
        public async Task<Customer> GetCustomerWithTransactionsAsync(Guid customerId)
        {
            return await _dbSet
                .Include(c => c.Transactions)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        /// <summary>
        /// Müşterinin güncel bakiyesini getirir.
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Müşteri bakiyesi</returns>
        public async Task<decimal> GetCustomerBalanceAsync(Guid customerId)
        {
            var customer = await _dbSet.FindAsync(customerId);
            return customer?.Balance ?? 0;
        }

        /// <summary>
        /// Belirtilen borç aralığındaki müşterileri getirir.
        /// </summary>
        /// <param name="minDebt">Minimum borç miktarı</param>
        /// <param name="maxDebt">Maximum borç miktarı</param>
        /// <returns>Borç aralığındaki müşteriler</returns>
        public async Task<IEnumerable<Customer>> GetCustomersByDebtRangeAsync(decimal minDebt, decimal maxDebt)
        {
            return await _dbSet
                .Where(c => c.Balance >= minDebt && c.Balance <= maxDebt)
                .OrderByDescending(c => c.Balance)
                .ToListAsync();
        }
    }
} 