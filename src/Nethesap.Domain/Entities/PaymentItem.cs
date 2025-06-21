using System;
using System.ComponentModel.DataAnnotations.Schema;

//PaymentItem: Ödeme detaylarını tutan sınıf. Hangi üründen kaç adet alındığı, birim fiyatı
//ve toplam fiyatı gibi bilgileri içerir. Payment ve Product ile ilişki kurar.
namespace Nethesap.Domain.Entities
{
    public class PaymentItem : BaseEntity
    {
        public Guid PaymentId { get; set; }
        
        [ForeignKey("PaymentId")]
        public virtual Payment? Payment { get; set; }
        
        public Guid ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
} 