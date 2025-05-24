using System;
using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class SaleDesignData
{
    public string Customer { get; set; }
    public string Product { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
} 