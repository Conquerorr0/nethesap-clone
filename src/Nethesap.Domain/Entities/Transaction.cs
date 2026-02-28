using System;
using System.ComponentModel.DataAnnotations.Schema;

//Transaction: İşlem bilgilerini tutan sınıf. Müşteri bilgisi, işlem tutarı, işlem tipi (borç/alacak)

namespace Nethesap.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        
        public Guid? PaymentId { get; set; }
        
        [ForeignKey("PaymentId")]
        public virtual Payment? Payment { get; set; }
        
        public decimal TotalDueAmount { get; set; } // Toplam borç tutarı
        public decimal PaidAmount { get; set; } // Ödenen tutar
    }

    public enum TransactionType
    {
        Debt,
        Credit,
        PartialPayment,
        FullPayment,
        Refund
    }
}