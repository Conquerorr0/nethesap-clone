using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethesap.Application.Services
{
    /// <summary>
    /// Ödeme işlemleri için servis implementasyonu
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentItemRepository _paymentItemRepository;

        /// <summary>
        /// PaymentService constructor'ı
        /// </summary>
        /// <param name="paymentRepository">Ödeme repository'si</param>
        /// <param name="paymentItemRepository">Ödeme kalemi repository'si</param>
        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentItemRepository paymentItemRepository)
        {
            _paymentRepository = paymentRepository;
            _paymentItemRepository = paymentItemRepository;
        }

        /// <inheritdoc/>
        public async Task<List<Payment>> GetAllAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return new List<Payment>(payments);
        }

        /// <inheritdoc/>
        public async Task<Payment> GetByIdAsync(Guid id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Payment payment)
        {
            await _paymentRepository.AddAsync(payment);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Payment payment)
        {
            await _paymentRepository.UpdateAsync(payment);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment != null)
            {
                await _paymentRepository.RemoveAsync(payment);
            }
        }

        /// <inheritdoc/>
        public async Task<List<Payment>> GetByCustomerIdAsync(Guid customerId)
        {
            var payments = await _paymentRepository.FindAsync(p => p.CustomerId == customerId);
            return new List<Payment>(payments);
        }

        /// <inheritdoc/>
        public async Task<List<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _paymentRepository.FindAsync(p => 
                p.CreatedAt >= startDate && p.CreatedAt <= endDate);
            return new List<Payment>(payments);
        }

        /// <inheritdoc/>
        public async Task<List<PaymentItem>> GetPaymentItemsAsync(Guid paymentId)
        {
            var items = await _paymentItemRepository.FindAsync(pi => pi.PaymentId == paymentId);
            return new List<PaymentItem>(items);
        }
    }
} 