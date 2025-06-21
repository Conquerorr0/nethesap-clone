using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

//Product: Ürün bilgilerini tutan sınıf. İsim, açıklama, fiyat, stok miktarı, barkod ve kategori 
//gibi temel özellikleri içerir. PaymentItems ile ödeme detaylarına bağlantı kurar.
namespace Nethesap.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Barcode { get; set; }
        public string Category { get; set; }
        
        [InverseProperty("Product")]
        public virtual ICollection<PaymentItem> PaymentItems { get; set; } = new List<PaymentItem>();
    }
} 