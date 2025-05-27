using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
}

// Design-time data class
public class SettingsDesignData
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
} 