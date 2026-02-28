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
    public class SaleService : IDisposable
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<PaymentItem> _paymentItemRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly AppDbContext _dbContext;
        private bool _disposed = false;

        /// <summary>
        /// SaleService sınıfının constructor'ı
        /// </summary>
        public SaleService()
        {
            try
            {
                _dbContext = new AppDbContext();
                
                // Veritabanının mevcut olduğundan emin ol
                _dbContext.Database.EnsureCreated();
                
                _paymentRepository = new EfRepository<Payment>(_dbContext);
                _paymentItemRepository = new EfRepository<PaymentItem>(_dbContext);
                _customerRepository = new EfRepository<Customer>(_dbContext);
                _productRepository = new EfRepository<Product>(_dbContext);
                
                Console.WriteLine("SaleService başarıyla başlatıldı");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaleService başlatma hatası: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        private IQueryable<Payment> GetBaseQuery()
        {
            return _paymentRepository.Query()
                .Include(p => p.Customer)
                .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Transactions);
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
            // Transaction dışı kontroller
            try
            {
                if (payment.PaymentItems == null || !payment.PaymentItems.Any())
                {
                    throw new InvalidOperationException("Satış kalemleri boş olamaz.");
                }

                // Veritabanı bağlantısını test et
                // Not: Burada _dbContext yerine yeni oluşturacağımız context'i kullanacağız ama
                // hızlı kontrol için statik bir yöntem veya _dbContext kullanılabilir.
                // Ancak isolation prensibi gereği aşağıda yeni context oluşturuyoruz.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ön kontroller sırasında hata: {ex.Message}");
                throw; 
            }

            // Yeni bir context oluşturarak işlem yapıyoruz (Isolation)
            using var context = new AppDbContext();
            
            if (!await context.Database.CanConnectAsync())
            {
                throw new InvalidOperationException("Veritabanına bağlanılamıyor.");
            }

            // Transaction başlat
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.Now;

                // Yeni Payment oluştur
                var newPayment = new Payment
                {
                    Id = payment.Id != Guid.Empty ? payment.Id : Guid.NewGuid(),
                    CustomerId = payment.CustomerId,
                    TotalAmount = payment.TotalAmount,
                    PaidAmount = payment.PaidAmount,
                    RemainingAmount = payment.RemainingAmount,
                    IsFullyPaid = payment.IsFullyPaid,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = payment.PaymentType,
                    Description = payment.Description,
                    CreatedDate = now,
                    DueDate = payment.DueDate,
                    PaymentItems = new List<PaymentItem>() 
                };

                // Stok düşümü ve Item hazırlığı
                foreach (var item in payment.PaymentItems)
                {
                    if (item.ProductId == Guid.Empty)
                        throw new InvalidOperationException("Ürün bulunamadı (ID boş).");

                    // Ürünü bu context içinde bul
                    var product = await context.Products.FindAsync(item.ProductId);
                    
                    if (product == null)
                        throw new InvalidOperationException($"Ürün bulunamadı (ID: {item.ProductId}).");

                    // Stok kontrolü
                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"'{product.Name}' stoğu yetersiz! (Stok: {product.StockQuantity}, İstenen: {item.Quantity})");
                    }

                    // Stoğu güncelle
                    product.StockQuantity -= item.Quantity;
                    context.Entry(product).State = EntityState.Modified;

                    // PaymentItem oluştur
                    var newItem = new PaymentItem
                    {
                        Id = item.Id != Guid.Empty ? item.Id : Guid.NewGuid(),
                        PaymentId = newPayment.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };
                    
                    newPayment.PaymentItems.Add(newItem);
                }

                // Payment'ı ekle
                context.Payments.Add(newPayment);

                // Borç kaydı (Transaction)
                var saleTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = payment.CustomerId,
                    PaymentId = newPayment.Id,
                    Amount = newPayment.TotalAmount,
                    Type = TransactionType.Debt,
                    Description = payment.Description ?? "Satış işlemi",
                    TransactionDate = now,
                    TotalDueAmount = newPayment.TotalAmount,
                    PaidAmount = newPayment.PaidAmount
                };
                await context.Transactions.AddAsync(saleTransaction);

                // Ödeme kaydı (Transaction)
                if (payment.PaidAmount > 0)
                {
                    var paymentTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = payment.CustomerId,
                        PaymentId = newPayment.Id,
                        Amount = -payment.PaidAmount,
                        Type = payment.IsFullyPaid ? TransactionType.FullPayment : TransactionType.PartialPayment,
                        Description = $"Ödeme: {payment.PaidAmount:C2} ({payment.PaymentMethod})",
                        TransactionDate = now,
                        TotalDueAmount = newPayment.TotalAmount,
                        PaidAmount = payment.PaidAmount
                    };
                    await context.Transactions.AddAsync(paymentTransaction);
                }

                // Kaydet
                await context.SaveChangesAsync();

                // Müşteri bakiyesini güncelle
                var customerTransactions = await context.Transactions
                    .Where(t => t.CustomerId == payment.CustomerId)
                    .ToListAsync();

                decimal totalBalance = customerTransactions.Sum(t => t.Amount);
                
                var customer = await context.Customers.FindAsync(payment.CustomerId);
                if (customer != null)
                {
                    customer.Balance = totalBalance;
                    context.Entry(customer).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }

                // Hepsini onayla
                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Satış hatası: {ex.Message}");
                throw; 
            }
        }

        /// <summary>
        /// Satış iade işlemi (Kısmi/Tam)
        /// </summary>
        /// <param name="originalSaleId">Orijinal satış ID'si</param>
        /// <param name="refundItems">İade edilecek ürünler (Quantity alanında iade edilecek adet olmalı)</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> RefundSaleAsync(Guid originalSaleId, IEnumerable<PaymentItem> refundItems)
        {
            using var context = new AppDbContext(); // Isolate context
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Orijinal satışı ve kalemlerini getir
                var originalSale = await context.Payments
                    .Include(p => p.PaymentItems)
                    .ThenInclude(pi => pi.Product)
                    .FirstOrDefaultAsync(p => p.Id == originalSaleId);

                if (originalSale == null)
                    throw new InvalidOperationException("Satış bulunamadı.");

                // 2. İade satış kaydını oluştur
                var refundSale = new Payment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = originalSale.CustomerId,
                    CreatedDate = DateTime.Now,
                    PaymentMethod = originalSale.PaymentMethod,
                    PaymentType = PaymentType.Refund,
                    Description = $"İade: {originalSale.Description ?? "SATIŞ"} (Ref: {originalSaleId.ToString().Substring(0, 8)})",
                    PaymentItems = new List<PaymentItem>(),
                    IsFullyRefunded = true // Bu kayıt kendisi bir iade kaydı olduğu için listede gösterilmeyebilir veya filtreye takılabilir
                    // Not: İade kaydı "Sale" olmadığı için zaten ana listede PaymentType ile filtrelenir veya görünür.
                    // Ana satışın IsFullyRefunded durumu aşağıda güncellenecek.
                };

                decimal totalRefundAmount = 0;
                bool isFullRefund = true; // Başlangıç varsayımı

                // 3. Seçili ürünleri işle
                foreach (var refundItemReq in refundItems)
                {
                    // Orijinal kalemi bul
                    var originalItem = originalSale.PaymentItems.FirstOrDefault(pi => pi.ProductId == refundItemReq.ProductId && pi.Id == refundItemReq.Id);
                    
                    if (originalItem == null)
                        // Tam eşleşme yoksa ProductId ile dene (farklı item ID olabilir ama aynı ürün)
                         originalItem = originalSale.PaymentItems.FirstOrDefault(pi => pi.ProductId == refundItemReq.ProductId);

                    if (originalItem == null)
                        throw new InvalidOperationException($"İade edilmek istenen ürün satışta bulunamadı (ID: {refundItemReq.ProductId})");

                    // Miktar kontrolü
                    int maxRefundable = originalItem.Quantity - originalItem.RefundedQuantity;
                    if (refundItemReq.Quantity > maxRefundable)
                         throw new InvalidOperationException($"'{originalItem.Product?.Name}' için iade edilebilir miktar aşıldı! (Max: {maxRefundable})");

                    if (refundItemReq.Quantity <= 0) continue;

                    // Orijinal kalemi güncelle
                    originalItem.RefundedQuantity += refundItemReq.Quantity;
                    context.Entry(originalItem).State = EntityState.Modified;

                    // İade kalemi oluştur
                    var newRefundItem = new PaymentItem
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = refundSale.Id,
                        ProductId = refundItemReq.ProductId,
                        Quantity = refundItemReq.Quantity,
                        UnitPrice = originalItem.UnitPrice,
                        TotalPrice = originalItem.UnitPrice * refundItemReq.Quantity,
                        RefundedQuantity = 0
                    };
                    
                    refundSale.PaymentItems.Add(newRefundItem);
                    totalRefundAmount += newRefundItem.TotalPrice;

                    // Stok güncelle (Artır)
                    var product = await context.Products.FindAsync(refundItemReq.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += refundItemReq.Quantity;
                        context.Entry(product).State = EntityState.Modified;
                    }
                }

                if (totalRefundAmount <= 0)
                    throw new InvalidOperationException("İade edilecek ürün seçilmedi.");

                refundSale.TotalAmount = -totalRefundAmount; // Negatif tutar
                refundSale.RemainingAmount = 0;
                refundSale.PaidAmount = refundSale.TotalAmount; // Tamamı ödenmiş (iade edilmiş) sayılır
                refundSale.IsFullyPaid = true;

                // 4. Orijinal Satışın "IsFullyRefunded" durumunu kontrol et
                // Tüm ürünlerin tamamı iade edilmiş mi?
                bool allItemsRefunded = true;
                foreach (var item in originalSale.PaymentItems)
                {
                    if (item.RefundedQuantity < item.Quantity)
                    {
                        allItemsRefunded = false;
                        break;
                    }
                }
                originalSale.IsFullyRefunded = allItemsRefunded;
                context.Entry(originalSale).State = EntityState.Modified;

                // 5. Kayıtları ekle
                context.Payments.Add(refundSale);

                // 6. Finansal Transaction (Para iadesi / Borç silme)
                var refundTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = refundSale.CustomerId,
                    PaymentId = refundSale.Id,
                    Amount = -totalRefundAmount, // Bakiyeyi düşür (borcu azalt) -> Aslında satış (+), iade (-) olmalı.
                    // Fakat sistem "Amount"ları topluyor. Satış BORÇ(Debt) olarak ekleniyorsa (+), İade ALACAK/ÖDEME (-) gibi olmalı.
                    // SaleService.AddSaleAsync'de Debt = +TotalAmount.
                    // Burada -TotalAmount veriyoruz ki bakiye düşsün.
                    // Ama TotalRefundAmount pozitif (örn 100 TL). Biz -100 eklemeliyiz.
                    // refundSale.TotalAmount zaten -100.
                    // Transaction Amount: -100.
                    
                    Type = TransactionType.Refund,
                    Description = refundSale.Description,
                    TransactionDate = DateTime.Now,
                    TotalDueAmount = 0,
                    PaidAmount = 0
                };
                // Ancak dikkat: Debt transaction pozitiftir. Payment transaction negatiftir.
                // Refund transaction da negatif olmalıdır ki borcu silsin.
                refundTransaction.Amount = -Math.Abs(totalRefundAmount); 
                
                await context.Transactions.AddAsync(refundTransaction);

                await context.SaveChangesAsync();

                // 7. Müşteri Bakiyesini Güncelle
                 var customerTransactions = await context.Transactions
                    .Where(t => t.CustomerId == originalSale.CustomerId)
                    .ToListAsync();
                
                decimal totalBalance = customerTransactions.Sum(t => t.Amount);
                var customer = await context.Customers.FindAsync(originalSale.CustomerId);
                if (customer != null)
                {
                    customer.Balance = totalBalance;
                    context.Entry(customer).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"İade hatası: {ex.Message}");
                throw;
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
                    .Include(p => p.Transactions)
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

        /// <summary>
        /// Belirli bir ürünün satış geçmişini getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <returns>Ürünün satış geçmişi</returns>
        public async Task<List<Payment>> GetProductSaleHistoryAsync(Guid productId)
        {
            try
            {
                var paymentRepository = new PaymentRepository(_dbContext);
                var payments = await paymentRepository.GetPaymentsByProductAsync(productId);
                return payments.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün satış geçmişi getirilirken hata oluştu: {ex.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Kısmi ödeme ekler
        /// </summary>
        /// <param name="paymentId">Ödeme ID'si</param>
        /// <param name="amount">Ödeme tutarı</param>
        /// <param name="paymentMethod">Ödeme yöntemi</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> AddPartialPaymentAsync(Guid paymentId, decimal amount, PaymentMethod paymentMethod)
        {
            try
            {
                // 1) Asıl ödeme işlemi - transaction içinde
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Ödemeyi getir
                        var payment = await GetSaleByIdAsync(paymentId);
                        if (payment == null)
                        {
                            Console.WriteLine($"Ödeme bulunamadı: {paymentId}");
                            return false;
                        }

                        // Ödeme tutarını kontrol et
                        if (amount <= 0 || amount > payment.RemainingAmount)
                        {
                            Console.WriteLine($"Geçersiz ödeme tutarı: {amount}");
                            return false;
                        }

                        // Ödeme bilgilerini güncelle
                        payment.PaidAmount += amount;
                        payment.RemainingAmount = payment.TotalAmount - payment.PaidAmount;
                        payment.IsFullyPaid = payment.RemainingAmount <= 0;

                        // Transaction kaydı oluştur (ödeme, borcu azaltmalı)
                        var paymentTransaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = payment.CustomerId,
                            PaymentId = payment.Id,
                            Amount = -amount,
                            Type = payment.IsFullyPaid ? TransactionType.FullPayment : TransactionType.PartialPayment,
                            Description = $"Ödeme: {amount:C2} ({paymentMethod})",
                            TransactionDate = DateTime.Now,
                            TotalDueAmount = payment.TotalAmount,
                            PaidAmount = payment.PaidAmount
                        };

                        // Veritabanına kaydet
                        _dbContext.Payments.Update(payment);
                        await _dbContext.Transactions.AddAsync(paymentTransaction);
                        await _dbContext.SaveChangesAsync();

                        // Transaction'ı onayla
                        await transaction.CommitAsync();
                        Console.WriteLine($"Kısmi ödeme başarıyla eklendi: {amount:C2}");
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Kısmi ödeme eklenirken hata oluştu: {ex.Message}");
                        return false;
                    }
                }

                // 2) Müşteri bakiyesi - en iyi çaba, hata olsa bile ödeme kayıtlı kalsın
                try
                {
                    var transactionRepository = new TransactionRepository(_dbContext);
                    // Tüm transaction'lara göre güncel bakiye
                    var payment = await GetSaleByIdAsync(paymentId);
                    if (payment != null)
                    {
                        var newBalance = await transactionRepository.GetCustomerBalanceAsync(payment.CustomerId);
                        var customer = await _customerRepository.GetByIdAsync(payment.CustomerId);
                        if (customer != null)
                        {
                            customer.Balance = newBalance;
                            await _customerRepository.UpdateAsync(customer);
                        }
                    }
                }
                catch (Exception balanceEx)
                {
                    Console.WriteLine($"Müşteri bakiyesi güncellenirken hata oluştu (ödeme kayıtlı): {balanceEx.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kısmi ödeme işlemi başlatılırken hata oluştu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ödenmemiş satışları getirir
        /// </summary>
        /// <returns>Ödenmemiş satışlar</returns>
        public async Task<List<Payment>> GetUnpaidSalesAsync()
        {
            try
            {
                var paymentRepository = new PaymentRepository(_dbContext);
                var unpaidPayments = await paymentRepository.GetUnpaidPaymentsAsync();
                return unpaidPayments.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ödenmemiş satışlar getirilirken hata oluştu: {ex.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Yaklaşan ödemeleri getirir
        /// </summary>
        /// <param name="daysThreshold">Gün eşiği</param>
        /// <returns>Yaklaşan ödemeler</returns>
        public async Task<List<Payment>> GetUpcomingPaymentsAsync(int daysThreshold = 7)
        {
            try
            {
                var paymentRepository = new PaymentRepository(_dbContext);
                var upcomingPayments = await paymentRepository.GetUpcomingPaymentsAsync(daysThreshold);
                return upcomingPayments.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yaklaşan ödemeler getirilirken hata oluştu: {ex.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Müşterinin toplam borcunu hesaplar
        /// </summary>
        /// <param name="customerId">Müşteri ID'si</param>
        /// <returns>Toplam borç</returns>
        public async Task<decimal> GetCustomerTotalDebtAsync(Guid customerId)
        {
            try
            {
                var paymentRepository = new PaymentRepository(_dbContext);
                return await paymentRepository.GetTotalUnpaidAmountByCustomerAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri borcu hesaplanırken hata oluştu: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Belirli bir ürünün ödeme geçmişini getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <returns>Ürünün ödeme geçmişi</returns>
        public async Task<List<Payment>> GetProductPaymentHistoryAsync(Guid productId)
        {
            try
            {
                var paymentRepository = new PaymentRepository(_dbContext);
                var payments = await paymentRepository.GetPaymentsByProductAsync(productId);
                return payments.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün ödeme geçmişi getirilirken hata oluştu: {ex.Message}");
                return new List<Payment>();
            }
        }

        /// <summary>
        /// Satış eklerken kısmi ödeme desteği
        /// </summary>
        /// <param name="payment">Ödeme bilgileri</param>
        /// <param name="paidAmount">Ödenen tutar</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> AddSaleWithPartialPaymentAsync(Payment payment, decimal paidAmount)
        {
            try
            {
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Ödeme bilgilerini ayarla
                        payment.PaidAmount = paidAmount;
                        payment.RemainingAmount = payment.TotalAmount - paidAmount;
                        payment.IsFullyPaid = payment.RemainingAmount <= 0;

                        // PaymentItems'ların PaymentId'lerini ayarla
                        foreach (var item in payment.PaymentItems)
                        {
                            if (item.Id == Guid.Empty)
                            {
                                item.Id = Guid.NewGuid();
                            }
                            item.PaymentId = payment.Id;
                        }

                        // Satışı ekle (PaymentItems da otomatik eklenecek)
                        _dbContext.Payments.Add(payment);
                        await _dbContext.SaveChangesAsync();

                        // Stok miktarlarını güncelle
                        foreach (var item in payment.PaymentItems)
                        {
                            var product = await _productRepository.GetByIdAsync(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity -= item.Quantity;
                                _dbContext.Products.Update(product);
                            }
                        }
                        
                        // Stok değişikliklerini veritabanına kaydet
                        await _dbContext.SaveChangesAsync();

                        // Transaction kaydı oluştur
                        var paymentTransaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = payment.CustomerId,
                            PaymentId = payment.Id,
                            Amount = paidAmount,
                            Type = payment.IsFullyPaid ? TransactionType.FullPayment : TransactionType.PartialPayment,
                            Description = $"Ödeme: {paidAmount:C2} ({payment.PaymentMethod})",
                            TransactionDate = DateTime.Now,
                            TotalDueAmount = payment.TotalAmount,
                            PaidAmount = paidAmount
                        };

                        // Transaction kaydını ekle
                        await _dbContext.Transactions.AddAsync(paymentTransaction);
                        await _dbContext.SaveChangesAsync();

                        // Transaction'ı onayla
                        await transaction.CommitAsync();
                        Console.WriteLine($"Satış başarıyla eklendi, ödenen: {paidAmount:C2}, kalan: {payment.RemainingAmount:C2}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Satış eklenirken hata oluştu: {ex.Message}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Satış işlemi başlatılırken hata oluştu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Veritabanı bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı başarılı ise true</returns>
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                Console.WriteLine("Veritabanı bağlantısı test ediliyor...");
                bool canConnect = await _dbContext.Database.CanConnectAsync();
                Console.WriteLine($"Database.CanConnectAsync(): {canConnect}");
                
                if (canConnect)
                {
                    // Basit bir query test et
                    var customerCount = await _dbContext.Customers.CountAsync();
                    Console.WriteLine($"Müşteri sayısı: {customerCount}");
                    
                    var productCount = await _dbContext.Products.CountAsync();
                    Console.WriteLine($"Ürün sayısı: {productCount}");
                    
                    var paymentCount = await _dbContext.Payments.CountAsync();
                    Console.WriteLine($"Ödeme sayısı: {paymentCount}");
                    
                    var paymentItemCount = await _dbContext.PaymentItems.CountAsync();
                    Console.WriteLine($"Ödeme kalemleri sayısı: {paymentItemCount}");
                }
                
                return canConnect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı bağlantı test hatası: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 