using Nethesap.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Domain.IRepositories
{
    /// <summary>
    /// İşlem (Transaction) repository interface'i
    /// </summary>
    public interface ITransactionRepository : IRepository<Transaction>
    {
        /// <summary>
        /// Müşteriye ait işlemleri getirir
        /// </summary>
        Task<List<Transaction>> GetByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Tarih aralığına göre işlemleri getirir
        /// </summary>
        Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Müşterinin toplam bakiyesini hesaplar
        /// </summary>
        Task<decimal> GetCustomerBalanceAsync(Guid customerId);
    }
} 