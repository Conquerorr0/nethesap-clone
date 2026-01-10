namespace Nethesap.Domain.Enums
{
    /// <summary>
    /// Satış iade durumu
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// İade yok
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Kısmi iade (bazı ürünler iade edildi)
        /// </summary>
        Partial = 1,
        
        /// <summary>
        /// Tam iade (tüm ürünler iade edildi)
        /// </summary>
        Full = 2
    }
}
