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
    /// Satış işlemlerini yönetmek için kullanılan servis sınıfı
    /// </summary>
    public class SaleService
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<PaymentItem> _paymentItemRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// SaleService sınıfının constructor'ı
        /// </summary>
        public SaleService()
        {
            _dbContext = new AppDbContext();
            _paymentRepository = new EfRepository<Payment>(_dbContext);
            _paymentItemRepository = new EfRepository<PaymentItem>(_dbContext);
            _customerRepository = new EfRepository<Customer>(_dbContext);
            _productRepository = new EfRepository<Product>(_dbContext);
        }

        private IQueryable<Payment> GetBaseQuery()
        {
            return _paymentRepository.Query()
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product);
        }

        /// <summary>
        /// Tüm satışları getirir
        /// </summary>
        /// <returns>Tüm satışların listesi</returns>
        public async Task<List<Payment>> GetSalesAsync()
        {
            try
            {
                return await GetBaseQuery().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tüm satışlar getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki müşteriye ait satışları getirir
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Müşteriye ait satışların listesi</returns>
        public async Task<List<Payment>> GetSalesByCustomerAndDateRangeAsync(Guid customerId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = GetBaseQuery().Where(p => p.CustomerId == customerId);

                if (startDate.HasValue && endDate.HasValue)
                {
                    var startDateTime = startDate.Value.Date;
                    var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    
                    query = query.Where(p => 
                        p.CreatedDate >= startDateTime && 
                        p.CreatedDate <= endDateTime);
                }
                
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşterinin satışları getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Tüm satışları getirir
        /// </summary>
        /// <returns>Satış listesi</returns>
        public async Task<ObservableCollection<Payment>> GetAllSalesAsync()
        {
            try
            {
                var sales = await GetBaseQuery().ToListAsync();
                return new ObservableCollection<Payment>(sales);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Satışlar getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Payment>();
            }
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki satışları getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Tarih aralığındaki satışların listesi</returns>
        public async Task<List<Payment>> GetSalesByDateRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = GetBaseQuery();
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startDateTime = startDate.Value.Date;
                    var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    
                    query = query.Where(p => 
                        p.CreatedDate >= startDateTime && 
                        p.CreatedDate <= endDateTime);
                }
                
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tarih aralığındaki satışlar getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Belirli bir müşteriye ait satışları getirir
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Müşteriye ait satış listesi</returns>
        public async Task<ObservableCollection<Payment>> GetSalesByCustomerAsync(Guid customerId)
        {
            try
            {
                var sales = await GetBaseQuery()
                    .Where(p => p.CustomerId == customerId)
                    .ToListAsync();
                return new ObservableCollection<Payment>(sales);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteriye göre satışlar getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return new ObservableCollection<Payment>();
            }
        }

        /// <summary>
        /// ID'ye göre satış getirir
        /// </summary>
        /// <param name="id">Satış ID'si</param>
        /// <returns>Bulunan satış veya null</returns>
        public async Task<Payment?> GetSaleByIdAsync(Guid id)
        {
            try
            {
                if (_dbContext == null)
                {
                    Console.WriteLine("HATA: DbContext null, yeniden oluşturuluyor");
                    return null;
                }
                
                return await GetBaseQuery()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Satış getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Yeni satış ekler
        /// </summary>
        /// <param name="payment">Eklenecek satış</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> AddSaleAsync(Payment payment)
        {
            try
            {
                Console.WriteLine($"Satış ekleme işlemi başlatıldı: ID: {payment.Id}");
                
                // Satış detaylarını kontrol et
                if (payment.PaymentItems == null || !payment.PaymentItems.Any())
                {
                    Console.WriteLine("HATA: Satış kalemleri boş!");
                    return false;
                }

                // Her bir ürünü kontrol et ve doğru ID'leri ata
                foreach (var item in payment.PaymentItems)
                {
                    // ID kontrolü
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        Console.WriteLine($"Satış kalemi için yeni ID oluşturuldu: {item.Id}");
                    }
                    
                    // PaymentId atama
                    item.PaymentId = payment.Id;
                    
                    // ProductId kontrolü
                    if (item.ProductId == Guid.Empty)
                    {
                        Console.WriteLine("HATA: Ürün ID'si boş!");
                        return false;
                    }
                    
                    // Ürün kontrolü
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        Console.WriteLine($"HATA: {item.ProductId} ID'li ürün bulunamadı!");
                        return false;
                    }
                    
                    // Stok kontrolü
                    if (product.StockQuantity < item.Quantity)
                    {
                        Console.WriteLine($"HATA: Yetersiz stok! Ürün: {product.Name}, Mevcut: {product.StockQuantity}, İstenen: {item.Quantity}");
                        return false;
                    }
                    
                    // Ürün bilgilerini güncelle
                    item.Product = product;
                    item.UnitPrice = product.Price;
                    item.TotalPrice = product.Price * item.Quantity;
                    
                    Console.WriteLine($"Satış kalemi: Ürün: {product.Name}, Adet: {item.Quantity}, Birim Fiyat: {item.UnitPrice:C2}, Toplam: {item.TotalPrice:C2}");
                }

                payment.CreatedDate = DateTime.Now;
                Console.WriteLine($"Satış tarihi: {payment.CreatedDate}");

                try
                {
                    // Transaction başlat
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // 1. Satışı ekle
                        await _dbContext.Payments.AddAsync(payment);
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine("Satış veritabanına eklendi.");
                        
                        // 2. Stok miktarlarını güncelle
                        foreach (var item in payment.PaymentItems)
                        {
                            var product = await _productRepository.GetByIdAsync(item.ProductId);
                            if (product != null)
                            {
                                // Satış yapıldığında stok azalt
                                product.StockQuantity -= item.Quantity;
                                _dbContext.Products.Update(product);
                                Console.WriteLine($"Ürün stoğu güncellendi: {product.Name}, Yeni stok: {product.StockQuantity}");
                            }
                        }
                        
                        // Değişiklikleri kaydet
                        await _dbContext.SaveChangesAsync();
                        
                        // Transaction'ı onayla
                        await transaction.CommitAsync();
                        Console.WriteLine("Tüm değişiklikler başarıyla kaydedildi.");
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Transaction geri alındı. Hata: {ex.Message}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Veritabanı işlemi sırasında hata: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Satış ekleme hatası: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Satış iade işlemi
        /// </summary>
        /// <param name="originalSaleId">Orijinal satış ID'si</param>
        /// <param name="refundItems">İade edilecek ürünler</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> RefundSaleAsync(Guid originalSaleId, IEnumerable<PaymentItem> refundItems)
        {
            try
            {
                // Orijinal satışı getir
                var originalSale = await GetSaleByIdAsync(originalSaleId);
                if (originalSale == null)
                {
                    return false;
                }

                // İade satışını oluştur
                var refundSale = new Payment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = originalSale.CustomerId,
                    CreatedDate = DateTime.Now,
                    PaymentMethod = originalSale.PaymentMethod,
                    PaymentType = PaymentType.Refund,
                    Description = $"İade: {originalSale.Description}",
                    PaymentItems = new Collection<PaymentItem>()
                };

                decimal totalRefundAmount = 0;

                // İade edilecek ürünleri ekle
                foreach (var item in refundItems)
                {
                    var refundItem = new PaymentItem
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = refundSale.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.UnitPrice * item.Quantity
                    };

                    refundSale.PaymentItems.Add(refundItem);
                    totalRefundAmount += refundItem.TotalPrice;
                }

                refundSale.TotalAmount = -totalRefundAmount; // İade tutarı negatif olarak kaydedilir

                // Satışı ekle
                await _paymentRepository.AddAsync(refundSale);

                // Stok miktarlarını güncelle (iade edildiği için artır)
                foreach (var item in refundItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İade işlemi sırasında hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }

        /// <summary>
        /// Satış raporunu oluşturur
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Satış raporu metni</returns>
        public async Task<string> GenerateSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DateTime start = startDate ?? DateTime.Now.AddMonths(-1);
                DateTime end = endDate ?? DateTime.Now;
                
                var sales = await GetSalesByDateRangeAsync(start, end);
                
                var reportBuilder = new System.Text.StringBuilder();
                reportBuilder.AppendLine($"SATIŞ RAPORU ({start:dd.MM.yyyy} - {end:dd.MM.yyyy})");
                reportBuilder.AppendLine("==================================");
                reportBuilder.AppendLine();
                
                // Toplam satış tutarı
                decimal totalSalesAmount = sales.Where(s => s.PaymentType == PaymentType.Sale).Sum(s => s.TotalAmount);
                decimal totalRefundsAmount = sales.Where(s => s.PaymentType == PaymentType.Refund).Sum(s => s.TotalAmount);
                decimal netAmount = totalSalesAmount + totalRefundsAmount; // İadeler negatif olduğu için toplama yapılır
                
                reportBuilder.AppendLine($"Toplam Satış: {totalSalesAmount:C2}");
                reportBuilder.AppendLine($"Toplam İade: {Math.Abs(totalRefundsAmount):C2}");
                reportBuilder.AppendLine($"Net Satış: {netAmount:C2}");
                reportBuilder.AppendLine();
                
                // Ödeme yöntemine göre satışlar
                reportBuilder.AppendLine("ÖDEME YÖNTEMİNE GÖRE SATIŞLAR");
                reportBuilder.AppendLine("----------------------------");
                
                var salesByPaymentMethod = sales
                    .Where(s => s.PaymentType == PaymentType.Sale)
                    .GroupBy(s => s.PaymentMethod);
                
                foreach (var group in salesByPaymentMethod)
                {
                    string paymentMethodName = "Bilinmeyen";
                    switch (group.Key)
                    {
                        case PaymentMethod.Cash:
                            paymentMethodName = "Nakit";
                            break;
                        case PaymentMethod.CreditCard:
                            paymentMethodName = "Kredi Kartı";
                            break;
                        case PaymentMethod.BankTransfer:
                            paymentMethodName = "Havale";
                            break;
                    }
                    
                    reportBuilder.AppendLine($"{paymentMethodName}: {group.Count()} adet, {group.Sum(s => s.TotalAmount):C2}");
                }
                
                return reportBuilder.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rapor oluşturulurken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return $"Rapor oluşturulurken hata oluştu: {ex.Message}";
            }
        }

        /// <summary>
        /// Satış detaylarını getirir (PaymentItems ve Product bilgilerini de içerir)
        /// </summary>
        /// <param name="saleId">Satış ID'si</param>
        /// <returns>Detayları ile birlikte satış nesnesi</returns>
        public async Task<Payment> GetSaleDetailsAsync(Guid saleId)
        {
            try
            {
                var sale = await _paymentRepository.Query()
                    .Include(p => p.Customer)
                    .Include(p => p.PaymentItems)
                        .ThenInclude(pi => pi.Product)
                    .FirstOrDefaultAsync(p => p.Id == saleId);

                return sale;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Satış detayları getirilirken hata oluştu: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }
    }
} 