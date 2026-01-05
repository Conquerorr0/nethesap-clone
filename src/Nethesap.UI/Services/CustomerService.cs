using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using Nethesap.Infrastructure.RepositoryImplementations;

namespace Nethesap.UI.Services
{
    /// <summary>
    /// Müşteri verilerini yönetmek için kullanılan servis sınıfı
    /// </summary>
    public class CustomerService
    {
        private readonly IRepository<Customer> _customerRepository;

        /// <summary>
        /// CustomerService sınıfının constructor'ı
        /// </summary>
        public CustomerService()
        {
            var dbContext = new AppDbContext();
            _customerRepository = new EfRepository<Customer>(dbContext);
        }

        public event Action<Customer>? CustomerAdded;

        /// <summary>
        /// Tüm müşterileri getirir ve her müşteri için güncel bakiyeyi hesaplar
        /// </summary>
        /// <returns>Müşteri listesi (bakiye bilgileri güncel)</returns>
        public async Task<ObservableCollection<Customer>> GetAllCustomersAsync()
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    // Müşterileri Payments ile birlikte yükle
                    var customers = await dbContext.Set<Customer>()
                        .Include(c => c.Payments)
                        .ToListAsync();

                    // Her müşteri için güncel bakiyeyi hesapla
                    foreach (var customer in customers)
                    {
                        if (customer.Payments != null && customer.Payments.Any())
                        {
                            // Payments'in RemainingAmount'larını toplayarak güncel borcu hesapla
                            customer.Balance = customer.Payments.Sum(p => p.RemainingAmount);
                        }
                        else
                        {
                            customer.Balance = 0;
                        }
                    }

                return new ObservableCollection<Customer>(customers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteriler getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Customer>();
            }
        }

        /// <summary>
        /// İsme göre müşteri arar ve her müşteri için güncel bakiyeyi hesaplar
        /// </summary>
        /// <param name="searchText">Aranacak metin</param>
        /// <returns>Aranan isme göre filtrelenmiş müşteri listesi (bakiye bilgileri güncel)</returns>
        public async Task<ObservableCollection<Customer>> SearchCustomersAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return await GetAllCustomersAsync();
                }

                // EF Core + SQLite için StringComparison parametresi desteklenmediği için
                // ToLower() ile case-insensitive arama yapıyoruz ve null kontrollerini ekliyoruz.
                string searchLower = searchText.ToLower();

                using (var dbContext = new AppDbContext())
                {
                    // Müşterileri Payments ile birlikte yükle ve filtrele
                    var customers = await dbContext.Set<Customer>()
                        .Include(c => c.Payments)
                        .Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(searchLower)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(searchLower)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(searchLower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchLower))
                        )
                        .ToListAsync();

                    // Her müşteri için güncel bakiyeyi hesapla
                    foreach (var customer in customers)
                    {
                        if (customer.Payments != null && customer.Payments.Any())
                        {
                            // Payments'in RemainingAmount'larını toplayarak güncel borcu hesapla
                            customer.Balance = customer.Payments.Sum(p => p.RemainingAmount);
                        }
                        else
                        {
                            customer.Balance = 0;
                        }
                    }

                return new ObservableCollection<Customer>(customers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri araması yapılırken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Customer>();
            }
        }

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        /// <param name="id">Müşteri ID'si</param>
        /// <returns>Bulunan müşteri veya null</returns>
        public async Task<Customer> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                return await _customerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Yeni müşteri ekler
        /// </summary>
        /// <param name="customer">Eklenecek müşteri</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try
            {
                if (customer.Id == Guid.Empty)
                {
                    customer.Id = Guid.NewGuid();
                }

                customer.CreatedAt = DateTime.UtcNow;
                await _customerRepository.AddAsync(customer);
                CustomerAdded?.Invoke(customer); // Event tetikleniyor
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri eklenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }

        /// <summary>
        /// Müşteri günceller
        /// </summary>
        /// <param name="customer">Güncellenecek müşteri</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.UpdatedAt = DateTime.UtcNow;
                await _customerRepository.UpdateAsync(customer);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri güncellenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }

        /// <summary>
        /// Müşteri siler (soft delete)
        /// </summary>
        /// <param name="customer">Silinecek müşteri</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> DeleteCustomerAsync(Customer customer)
        {
            try
            {
                await _customerRepository.RemoveAsync(customer);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri silinirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }
    }
} 