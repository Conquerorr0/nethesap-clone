using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Müşteri işlemleri için servis implementasyonu
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionRepository _transactionRepository;

        /// <summary>
        /// CustomerService constructor'ı
        /// </summary>
        /// <param name="customerRepository">Müşteri repository'si</param>
        /// <param name="transactionRepository">İşlem repository'si</param>
        public CustomerService(
            ICustomerRepository customerRepository,
            ITransactionRepository transactionRepository)
        {
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
        }

        /// <inheritdoc/>
        public async Task<List<Customer>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return new List<Customer>(customers);
        }

        /// <inheritdoc/>
        public async Task<Customer> GetByIdAsync(Guid id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Customer customer)
        {
            await _customerRepository.AddAsync(customer);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Customer customer)
        {
            await _customerRepository.UpdateAsync(customer);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer != null)
            {
                await _customerRepository.RemoveAsync(customer);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetBalanceAsync(Guid customerId)
        {
            var transactions = await _transactionRepository.FindAsync(t => t.CustomerId == customerId);
            return transactions.Sum(t => t.Amount);
        }

        /// <inheritdoc/>
        public async Task<List<Transaction>> GetTransactionHistoryAsync(Guid customerId)
        {
            var transactions = await _transactionRepository.FindAsync(t => t.CustomerId == customerId);
            return new List<Transaction>(transactions);
        }
    }
} 