using System;

//Transaction: İşlem bilgilerini tutan sınıf. Müşteri bilgisi, işlem tutarı, işlem tipi (borç/alacak)

namespace Nethesap.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public enum TransactionType
    {
        Debt,
        Credit
    }
} 