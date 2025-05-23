using System;
using System.Collections.Generic;

//Payment: Ödeme işlemlerini tutan sınıf. Müşteri bilgisi, toplam tutar, ödeme yöntemi ve ödeme 
//tipi (satış/iade) gibi bilgileri içerir. PaymentItems ile ödeme detaylarına bağlantı kurar.
namespace Nethesap.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentType PaymentType { get; set; }
        public string Description { get; set; }
        public virtual ICollection<PaymentItem> PaymentItems { get; set; }
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
        Refund
    }
} 