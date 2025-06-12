using System;
using System.Windows;
using System.Windows.Controls;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI.Views;

public partial class SalesView : System.Windows.Controls.UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }
    
    private void CustomerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is SalesViewModel viewModel)
        {
            try
            {
                // Popup'ı açık tut, SalesViewModel'de de kontrol edilecek
                viewModel.IsCustomerSearchOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Müşteri arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void ProductSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is SalesViewModel viewModel)
        {
            try
            {
                // Popup'ı açık tut, SalesViewModel'de de kontrol edilecek
                viewModel.IsProductSearchOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

// Design-time data class
public class SaleDesignData
{
    public string Customer { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public decimal Amount { get; set; } = 0;
} 