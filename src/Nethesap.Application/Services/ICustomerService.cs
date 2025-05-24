using Nethesap.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Müşteri işlemleri için servis interface'i
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Tüm müşterileri getirir
        /// </summary>
        Task<List<Customer>> GetAllAsync();

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        Task<Customer> GetByIdAsync(Guid id);

        /// <summary>
        /// Yeni müşteri ekler
        /// </summary>
        Task AddAsync(Customer customer);

        /// <summary>
        /// Müşteri bilgilerini günceller
        /// </summary>
        Task UpdateAsync(Customer customer);

        /// <summary>
        /// Müşteriyi siler (soft delete)
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Müşteri bakiyesini getirir
        /// </summary>
        Task<decimal> GetBalanceAsync(Guid customerId);

        /// <summary>
        /// Müşterinin işlem geçmişini getirir
        /// </summary>
        Task<List<Transaction>> GetTransactionHistoryAsync(Guid customerId);
    }
} 