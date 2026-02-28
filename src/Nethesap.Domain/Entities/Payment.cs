using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

//Payment: Ödeme işlemlerini tutan sınıf. Müşteri bilgisi, toplam tutar, ödeme yöntemi ve ödeme 
//tipi (satış/iade) gibi bilgileri içerir. PaymentItems ile ödeme detaylarına bağlantı kurar.
namespace Nethesap.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; } // Ödenen tutar
        public decimal RemainingAmount { get; set; } // Kalan tutar
        public bool IsFullyPaid { get; set; } // Tamamen ödenip ödenmediği
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? DueDate { get; set; } // Ödeme vadesi
        
        public bool IsFullyRefunded { get; set; } // Tamamen iade edilip edilmediği
        
        [InverseProperty("Payment")]
        public virtual ICollection<PaymentItem> PaymentItems { get; set; } = new List<PaymentItem>();
        
        [InverseProperty("Payment")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        BankTransfer
    }

    public enum PaymentType
    {
        Sale,
        Refund,
        PartialPayment
    }
} 