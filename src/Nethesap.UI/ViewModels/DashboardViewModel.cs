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
        // Gerçek verileri yükle
        LoadDataAsync();
        ChartFormatter = value => value.ToString("C0");
    }

    private async void LoadDataAsync()
    {
        try
        {
            // Varsayılan değerler
            TotalBalance = 0;
            TotalReceivables = 0;
            TotalPayables = 0;

            // Veritabanından son işlemleri al
            // Not: Burada gerçek bir servis kullanılmalı
            var transactions = new List<TransactionItem>();
            
            // Grafik verilerini hazırla
            PrepareChartData(2); // Son 6 ay
            
            // Verileri UI'a bağla
            RecentTransactions = new ObservableCollection<TransactionItem>(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veri yükleme hatası: {ex.Message}");
            // Hata durumunda boş koleksiyonlar oluştur
            RecentTransactions = new ObservableCollection<TransactionItem>();
            ChartSeries = new SeriesCollection();
            ChartLabels = new string[0];
        }
    }

    private void PrepareChartData(int periodIndex)
    {
        // Gerçek uygulamada burada seçilen döneme göre verileri yükleyeceğiz
        // Şimdilik sadece varsayılan değerler
        ChartSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Gelir",
                Values = new ChartValues<double> { 0, 0, 0, 0, 0, 0 },
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                LineSmoothness = 0.3,
                Stroke = System.Windows.Media.Brushes.ForestGreen,
                Fill = System.Windows.Media.Brushes.Transparent
            },
            new LineSeries
            {
                Title = "Gider",
                Values = new ChartValues<double> { 0, 0, 0, 0, 0, 0 },
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                LineSmoothness = 0.3,
                Stroke = System.Windows.Media.Brushes.Crimson,
                Fill = System.Windows.Media.Brushes.Transparent
            }
        };

        switch (periodIndex)
        {
            case 0: // Son 1 ay
                ChartLabels = new[] { "1.Hafta", "2.Hafta", "3.Hafta", "4.Hafta" };
                break;
            case 1: // Son 3 ay
                ChartLabels = new[] { "Nisan", "Mayıs", "Haziran" };
                break;
            case 2: // Son 6 ay
            default:
                ChartLabels = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };
                break;
        }
    }

    private void LoadDataForPeriod(int periodIndex)
    {
        // Periyot değiştiğinde grafik verilerini güncelle
        PrepareChartData(periodIndex);
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