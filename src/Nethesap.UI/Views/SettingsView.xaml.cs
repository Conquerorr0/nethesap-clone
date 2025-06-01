using System.Windows;
using System.Windows.Controls;

namespace Nethesap.UI.Views;

public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        
        // Kategori seçimini dinle
        SettingsCategories.SelectionChanged += SettingsCategories_SelectionChanged;
    }
    
    private void SettingsCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompanySettingsPanel == null || AppearanceSettingsPanel == null || BackupSettingsPanel == null)
            return;
            
        // Tüm panelleri gizle
        CompanySettingsPanel.Visibility = Visibility.Collapsed;
        AppearanceSettingsPanel.Visibility = Visibility.Collapsed;
        BackupSettingsPanel.Visibility = Visibility.Collapsed;
        
        // Seçilen kategoriye göre ilgili paneli göster
        switch (SettingsCategories.SelectedIndex)
        {
            case 0: // Şirket Bilgileri
                CompanySettingsPanel.Visibility = Visibility.Visible;
                break;
            case 1: // Görünüm
                AppearanceSettingsPanel.Visibility = Visibility.Visible;
                break;
            case 2: // Yedekleme
                BackupSettingsPanel.Visibility = Visibility.Visible;
                break;
        }
    }
}

// Design-time data class
public class SettingsDesignData
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
} 