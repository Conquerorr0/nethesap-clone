using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ödeme kalemleri için Entity Framework Core tabanlı repository implementasyonu.
/// </summary>
/// <remarks>
/// Bu sınıf, ödeme detayları, ürün bazlı satış analizi ve tarih aralığına göre satış raporları işlemlerini gerçekleştirir.
/// IPaymentItemRepository interface'ini implemente ederek ödeme kalemlerine özel iş mantığını sağlar.
/// </remarks>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class PaymentItemRepository : EfRepository<PaymentItem>, IPaymentItemRepository
    {
        /// <summary>
        /// PaymentItemRepository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public PaymentItemRepository(AppDbContext context) : base(context)
        {
        }

        // ... mevcut kod ...
    }
} 