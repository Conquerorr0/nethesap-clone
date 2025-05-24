using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class DashboardDesignData
{
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalSales { get; set; }
} 