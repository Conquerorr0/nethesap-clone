using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ödeme işlemleri için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, müşteri bazlı ödemeler, tarih aralığına göre ödemeler ve toplam tutar hesaplama işlemlerini gerçekleştirir.
/// IPaymentRepository interface'ini implemente ederek ödeme işlemlerine özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class PaymentRepository : EfRepository<Payment>, IPaymentRepository
    {
        /// <summary>
        /// PaymentRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        // ... mevcut kod ...
    }
} 