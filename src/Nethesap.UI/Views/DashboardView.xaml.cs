using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
        
        // Design-time data
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new DashboardDesignData();
        }
    }
}

// Design-time data class
public class DashboardDesignData
{
    public decimal TotalBalance { get; set; } = 125000.00m;
    public decimal TotalReceivables { get; set; } = 75000.00m;
    public decimal TotalPayables { get; set; } = 45000.00m;
    public int SelectedPeriodIndex { get; set; } = 0;

    public SeriesCollection ChartSeries { get; set; }
    public string[] ChartLabels { get; set; }
    public Func<double, string> ChartFormatter { get; set; }

    public ObservableCollection<TransactionItem> RecentTransactions { get; set; }

    public DashboardDesignData()
    {
        // Initialize chart data
        ChartSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Gelir",
                Values = new ChartValues<double> { 65000, 85000, 78000, 92000, 88000, 95000 },
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                LineSmoothness = 0.3
            },
            new LineSeries
            {
                Title = "Gider",
                Values = new ChartValues<double> { 45000, 42000, 55000, 48000, 58000, 62000 },
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                LineSmoothness = 0.3
            }
        };

        ChartLabels = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };
        ChartFormatter = value => value.ToString("C0");

        // Initialize sample transactions
        RecentTransactions = new ObservableCollection<TransactionItem>
        {
            new TransactionItem 
            { 
                Description = "Ahmet Yılmaz'dan ödeme",
                Date = DateTime.Now.AddDays(-1),
                Amount = 5000.00m,
                Icon = PackIconKind.ArrowTopRight,
                IsIncome = true
            },
            new TransactionItem 
            { 
                Description = "Tedarikçi ödemesi",
                Date = DateTime.Now.AddDays(-2),
                Amount = -2500.00m,
                Icon = PackIconKind.ArrowBottomLeft,
                IsIncome = false
            },
            new TransactionItem 
            { 
                Description = "Mehmet Kaya'dan ödeme",
                Date = DateTime.Now.AddDays(-3),
                Amount = 3500.00m,
                Icon = PackIconKind.ArrowTopRight,
                IsIncome = true
            },
            new TransactionItem 
            { 
                Description = "Kira ödemesi",
                Date = DateTime.Now.AddDays(-5),
                Amount = -4500.00m,
                Icon = PackIconKind.ArrowBottomLeft,
                IsIncome = false
            }
        };
    }
}

public class TransactionItem
{
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PackIconKind Icon { get; set; }
    public bool IsIncome { get; set; }
} 