using System;
using System.Collections.Generic;

//Customer: Müşteri bilgilerini tutan sınıf. İsim, telefon, e-posta, adres ve bakiye gibi 
//temel özellikleri içerir.Payments ve Transactions ile ödeme ve işlem detaylarına bağlantı kurar.

namespace Nethesap.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
} 