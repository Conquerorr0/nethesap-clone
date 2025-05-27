using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;

namespace Nethesap.UI.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private decimal _totalBalance;
    private decimal _totalReceivables;
    private decimal _totalPayables;
    private int _selectedPeriodIndex;
    private SeriesCollection _chartSeries;
    private string[] _chartLabels;
    private ObservableCollection<TransactionItem> _recentTransactions;

    public decimal TotalBalance
    {
        get => _totalBalance;
        set
        {
            _totalBalance = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalReceivables
    {
        get => _totalReceivables;
        set
        {
            _totalReceivables = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalPayables
    {
        get => _totalPayables;
        set
        {
            _totalPayables = value;
            OnPropertyChanged();
        }
    }

    public int SelectedPeriodIndex
    {
        get => _selectedPeriodIndex;
        set
        {
            _selectedPeriodIndex = value;
            OnPropertyChanged();
            LoadDataForPeriod(value);
        }
    }

    public SeriesCollection ChartSeries
    {
        get => _chartSeries;
        set
        {
            _chartSeries = value;
            OnPropertyChanged();
        }
    }

    public string[] ChartLabels
    {
        get => _chartLabels;
        set
        {
            _chartLabels = value;
            OnPropertyChanged();
        }
    }

    public Func<double, string> ChartFormatter { get; set; }

    public ObservableCollection<TransactionItem> RecentTransactions
    {
        get => _recentTransactions;
        set
        {
            _recentTransactions = value;
            OnPropertyChanged();
        }
    }

    public DashboardViewModel()
    {
        LoadSampleData();
        ChartFormatter = value => value.ToString("C0");
    }

    private void LoadSampleData()
    {
        // Örnek bakiye verileri
        TotalBalance = 125000.00m;
        TotalReceivables = 75000.00m;
        TotalPayables = 45000.00m;

        // Örnek grafik verileri
        ChartSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Gelir",
                Values = new ChartValues<double> { 65000, 85000, 78000, 92000, 88000, 95000 },
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                LineSmoothness = 0.3,
                Stroke = System.Windows.Media.Brushes.ForestGreen,
                Fill = System.Windows.Media.Brushes.Transparent
            },
            new LineSeries
            {
                Title = "Gider",
                Values = new ChartValues<double> { 45000, 42000, 55000, 48000, 58000, 62000 },
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                LineSmoothness = 0.3,
                Stroke = System.Windows.Media.Brushes.Crimson,
                Fill = System.Windows.Media.Brushes.Transparent
            }
        };

        ChartLabels = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };

        // Örnek işlem verileri
        RecentTransactions = new ObservableCollection<TransactionItem>
        {
            new TransactionItem 
            { 
                Description = "Ahmet Yılmaz'dan ödeme",
                Date = DateTime.Now.AddDays(-1),
                Amount = 5000.00m
            },
            new TransactionItem 
            { 
                Description = "Tedarikçi ödemesi - Elektronik malzemeler",
                Date = DateTime.Now.AddDays(-2),
                Amount = -2500.00m
            },
            new TransactionItem 
            { 
                Description = "Mehmet Kaya'dan ödeme - Proje taksiti",
                Date = DateTime.Now.AddDays(-3),
                Amount = 3500.00m
            },
            new TransactionItem 
            { 
                Description = "Kira ödemesi - Haziran 2024",
                Date = DateTime.Now.AddDays(-5),
                Amount = -4500.00m
            },
            new TransactionItem 
            { 
                Description = "Yazılım lisans geliri",
                Date = DateTime.Now.AddDays(-7),
                Amount = 8500.00m
            },
            new TransactionItem 
            { 
                Description = "Personel maaş ödemeleri",
                Date = DateTime.Now.AddDays(-7),
                Amount = -12500.00m
            }
        };
    }

    private void LoadDataForPeriod(int periodIndex)
    {
        // Gerçek uygulamada burada seçilen döneme göre verileri yükleyeceğiz
        // Şimdilik sadece örnek veriler
        switch (periodIndex)
        {
            case 0: // Son 1 ay
                ChartLabels = new[] { "1.Hafta", "2.Hafta", "3.Hafta", "4.Hafta" };
                ChartSeries[0].Values = new ChartValues<double> { 25000, 28000, 32000, 35000 };
                ChartSeries[1].Values = new ChartValues<double> { 18000, 22000, 20000, 25000 };
                break;
            case 1: // Son 3 ay
                ChartLabels = new[] { "Nisan", "Mayıs", "Haziran" };
                ChartSeries[0].Values = new ChartValues<double> { 75000, 82000, 95000 };
                ChartSeries[1].Values = new ChartValues<double> { 55000, 58000, 62000 };
                break;
            case 2: // Son 6 ay
                ChartLabels = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };
                ChartSeries[0].Values = new ChartValues<double> { 65000, 85000, 78000, 92000, 88000, 95000 };
                ChartSeries[1].Values = new ChartValues<double> { 45000, 42000, 55000, 48000, 58000, 62000 };
                break;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TransactionItem : INotifyPropertyChanged
{
    private string _description;
    private DateTime _date;
    private decimal _amount;
    private PackIconKind _icon;
    private bool _isIncome;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged();
        }
    }

    public decimal Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            _isIncome = value >= 0;
            _icon = value >= 0 ? PackIconKind.ArrowTopRight : PackIconKind.ArrowBottomLeft;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsIncome));
            OnPropertyChanged(nameof(Icon));
        }
    }

    public PackIconKind Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public bool IsIncome
    {
        get => _isIncome;
        set
        {
            _isIncome = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 