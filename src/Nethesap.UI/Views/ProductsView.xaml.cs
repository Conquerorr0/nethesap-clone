using System.ComponentModel;
using System.Windows.Controls;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI.Views;

public partial class ProductsView : System.Windows.Controls.UserControl
{
    public ProductsView()
    {
        InitializeComponent();
        
        if (DesignerProperties.GetIsInDesignMode(this) == false && DataContext == null)
        {
            DataContext = new ProductsViewModel();
        }
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