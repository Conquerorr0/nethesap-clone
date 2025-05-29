using System;
using System.Windows;
using System.Windows.Controls;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Set initial selection - MainViewModel will handle the actual view
        if (NavigationList.Items.Count > 0)
        {
            NavigationList.SelectedItem = DashboardItem;
        }
    }

    private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (NavigationList.SelectedItem == DashboardItem)
            {
                viewModel.NavigateToDashboard();
            }
            else if (NavigationList.SelectedItem == CustomersItem)
            {
                viewModel.NavigateToCustomers();
            }
            else if (NavigationList.SelectedItem == ProductsItem)
            {
                viewModel.NavigateToProducts();
            }
            else if (NavigationList.SelectedItem == SalesItem)
            {
                viewModel.NavigateToSales();
            }
            else if (NavigationList.SelectedItem == AccountsItem)
            {
                viewModel.NavigateToAccounts();
            }

            // Close the drawer after selection
            MenuToggleButton.IsChecked = false;
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.NavigateToSettings();
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
}