using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class CustomerDesignData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public decimal Balance { get; set; }
} 