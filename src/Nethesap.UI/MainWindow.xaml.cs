using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Nethesap.UI.Views;

namespace Nethesap.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set initial view and selection
        MainRegion.Content = new DashboardView();
        if (NavigationList.Items.Count > 0)
        {
            NavigationList.SelectedItem = NavigationList.Items[0];
        }
    }

    private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Debug.WriteLine($"NavigationList is null: {NavigationList == null}");
            if (NavigationList != null)
            {
                Debug.WriteLine($"SelectedItem is null: {NavigationList.SelectedItem == null}");
            }

            if (NavigationList?.SelectedItem is ListBoxItem selectedItem)
            {
                Debug.WriteLine($"Content is null: {selectedItem.Content == null}");
                Debug.WriteLine($"Content type: {selectedItem.Content?.GetType().FullName}");

                if (selectedItem.Content is StackPanel stackPanel)
                {
                    Debug.WriteLine($"StackPanel children count: {stackPanel.Children.Count}");
                    if (stackPanel.Children.Count > 1)
                    {
                        Debug.WriteLine($"Second child type: {stackPanel.Children[1]?.GetType().FullName}");
                        if (stackPanel.Children[1] is TextBlock textBlock)
                        {
                            Debug.WriteLine($"TextBlock text: {textBlock.Text}");
                            switch (textBlock.Text)
                            {
                                case "Dashboard":
                                    MainRegion.Content = new DashboardView();
                                    break;
                                case "Müşteriler":
                                    MainRegion.Content = new CustomersView();
                                    break;
                                case "Ürünler":
                                    MainRegion.Content = new ProductsView();
                                    break;
                                case "Satışlar":
                                    MainRegion.Content = new SalesView();
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in NavigationList_SelectionChanged: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}