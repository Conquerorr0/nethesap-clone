using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class AccountsView : UserControl
{
    public AccountsView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class AccountDesignData
{
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
} 