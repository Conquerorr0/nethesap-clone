using System;
using Nethesap.Domain.Enums;

namespace Nethesap.UI.Models
{
    /// <summary>
    /// Müşteri işlem geçmişi için timeline item modeli
    /// </summary>
    public class TransactionHistoryItem
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Satış", "Ödeme", "İade"
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public RefundStatus? RefundStatus { get; set; }
        public Guid? RelatedId { get; set; } // Payment veya Transaction ID
        
        /// <summary>
        /// İşlem tipi ikonu (Material Design)
        /// </summary>
        public string TypeIcon => Type switch
        {
            "Satış" => "ShoppingCart",
            "Ödeme" => "CashMultiple",
            "İade" => "CartRemove",
            _ => "Information"
        };
        
        /// <summary>
        /// Tutar rengi (pozitif: yeşil, negatif: kırmızı)
        /// </summary>
        public string AmountColor => Amount >= 0 ? "#2E7D32" : "#D32F2F";
        
        /// <summary>
        /// İade durumu etiketi
        /// </summary>
        public string RefundStatusLabel => RefundStatus switch
        {
            Domain.Enums.RefundStatus.Full => "Tam İade Edildi",
            Domain.Enums.RefundStatus.Partial => "Kısmi İade",
            _ => string.Empty
        };
        
        /// <summary>
        /// Formatlanmış tutar (+ veya - işareti ile)
        /// </summary>
        public string FormattedAmount
        {
            get
            {
                var sign = Amount >= 0 ? "+" : "";
                return $"{sign}{Amount:N2} TL";
            }
        }
    }
}
