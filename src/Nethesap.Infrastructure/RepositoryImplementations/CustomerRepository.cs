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

        // ... mevcut kod ...
    }
} 