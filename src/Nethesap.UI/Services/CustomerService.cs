using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// Tüm müşterileri getirir
        /// </summary>
        /// <returns>Müşteri listesi</returns>
        public async Task<ObservableCollection<Customer>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                return new ObservableCollection<Customer>(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteriler getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Customer>();
            }
        }

        /// <summary>
        /// İsme göre müşteri arar
        /// </summary>
        /// <param name="searchText">Aranacak metin</param>
        /// <returns>Aranan isme göre filtrelenmiş müşteri listesi</returns>
        public async Task<ObservableCollection<Customer>> SearchCustomersAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return await GetAllCustomersAsync();
                }

                var customers = await _customerRepository.FindAsync(c =>
                    c.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    c.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                );

                return new ObservableCollection<Customer>(customers);
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