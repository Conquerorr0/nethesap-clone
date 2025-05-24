using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class ProductsView : UserControl
{
    public ProductsView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class ProductDesignData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Barcode { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
} 