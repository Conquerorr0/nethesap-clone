using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class CustomersView : System.Windows.Controls.UserControl
{
    public CustomersView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class CustomerDesignData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
} 