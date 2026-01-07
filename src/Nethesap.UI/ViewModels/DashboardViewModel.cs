using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Nethesap.UI.Services;
using Nethesap.Domain.Entities;

namespace Nethesap.UI.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private decimal _totalBalance;
    private decimal _totalReceivables;
    private decimal _totalPayables;
    private int _selectedPeriodIndex = 0; // Varsayılan: Son 1 ay
    private SeriesCollection _chartSeries;
    private string[] _chartLabels;
    private double _chartMaxValue;
    private ObservableCollection<TransactionItem> _recentTransactions;
    private readonly SaleService _saleService;
    private readonly CustomerService _customerService;

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

    public double ChartMaxValue
    {
        get => _chartMaxValue;
        set
        {
            _chartMaxValue = value;
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
        _saleService = new SaleService();
        _customerService = new CustomerService();
        // Gerçek verileri yükle
        LoadDataAsync();
        ChartFormatter = value => value.ToString("C0");
    }

    private async void LoadDataAsync()
    {
        try
        {
            // Tüm satışları çek
            var allSales = await _saleService.GetAllSalesAsync();
            
            // Finansal metrikleri hesapla
            if (allSales != null && allSales.Any())
            {
                // Sadece satış tipindeki kayıtları al (iade hariç)
                var sales = allSales.Where(s => s.PaymentType == PaymentType.Sale).ToList();
                
                // Genel Bakiye: Toplam satışlar (tüm satın alınan ürünlerin toplam değeri)
                TotalBalance = sales.Sum(s => s.TotalAmount);
                
                // Toplam Alacak: Ödenen kısım
                TotalReceivables = sales.Sum(s => s.PaidAmount);
                
                // Toplam Borç: Ödenmeyen kısım (kalan borç)
                TotalPayables = sales.Sum(s => s.RemainingAmount);
            }
            else
            {
                TotalBalance = 0;
                TotalReceivables = 0;
                TotalPayables = 0;
            }

            // Veritabanından son işlemleri al
            // Not: Burada gerçek bir servis kullanılmalı
            var transactions = new List<TransactionItem>();
            
            // Grafik verilerini hazırla (varsayılan: Son 1 ay)
            PrepareChartData(0);
            
            // Verileri UI'a bağla
            RecentTransactions = new ObservableCollection<TransactionItem>(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veri yükleme hatası: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            // Hata durumunda boş koleksiyonlar oluştur
            TotalBalance = 0;
            TotalReceivables = 0;
            TotalPayables = 0;
            RecentTransactions = new ObservableCollection<TransactionItem>();
            ChartSeries = new SeriesCollection();
            ChartLabels = new string[0];
        }
    }

    private async void PrepareChartData(int periodIndex)
    {
        try
        {
            // Dinamik X-ekseni etiketlerini oluştur
            ChartLabels = GetDynamicLabelsForPeriod(periodIndex);
            
            // Tarih aralığını hesapla
            var (startDate, endDate) = GetDateRangeForPeriod(periodIndex);
            
            // Veritabanından satış verilerini çek
            var sales = await _saleService.GetSalesByDateRangeAsync(startDate, endDate);
            
            // Veri noktası sayısını periyoda göre ayarla
            int dataPointCount = ChartLabels.Length;
            
            // Gelir ve gider değerlerini hazırla
            var incomeValues = new ChartValues<double>();
            var expenseValues = new ChartValues<double>();
            
            // Periyoda göre verileri grupla ve topla
            for (int i = 0; i < dataPointCount; i++)
            {
                DateTime periodStart, periodEnd;
                
                switch (periodIndex)
                {
                    case 0: // Son 1 ay - Haftalık
                        periodEnd = endDate.AddDays(-7 * i);
                        periodStart = periodEnd.AddDays(-7);
                        break;
                        
                    case 1: // Son 3 ay - Aylık
                        periodEnd = endDate.AddMonths(-i);
                        periodStart = periodEnd.AddMonths(-1);
                        break;
                        
                    case 2: // Son 6 ay - Aylık
                    default:
                        periodEnd = endDate.AddMonths(-i);
                        periodStart = periodEnd.AddMonths(-1);
                        break;
                }
                
                // Bu periyottaki satışları filtrele
                var periodSales = sales.Where(s => 
                    s.CreatedDate >= periodStart && 
                    s.CreatedDate < periodEnd &&
                    s.PaymentType == PaymentType.Sale
                ).ToList();
                
                // Gelir: Satışların toplamı
                double income = (double)periodSales.Sum(s => s.TotalAmount);
                
                // Gider: Ödenen tutarlar (basitleştirilmiş - gerçek uygulamada ayrı expense tablosu olabilir)
                // Şimdilik satışların %60'ı gider olarak varsayılıyor
                double expense = income * 0.6;
                
                incomeValues.Insert(0, income); // Ters sırada ekliyoruz (en eskiden en yeniye)
                expenseValues.Insert(0, expense);
            }
            
            // Y-ekseni için maksimum değeri hesapla
            var allValues = incomeValues.Concat(expenseValues);
            ChartMaxValue = CalculateOptimalMaxValue(allValues);
            
            // Grafik serilerini oluştur
            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Gelir",
                    Values = incomeValues,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.3,
                    Stroke = System.Windows.Media.Brushes.ForestGreen,
                    Fill = System.Windows.Media.Brushes.Transparent
                },
                new LineSeries
                {
                    Title = "Gider",
                    Values = expenseValues,
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.3,
                    Stroke = System.Windows.Media.Brushes.Crimson,
                    Fill = System.Windows.Media.Brushes.Transparent
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Grafik verileri hazırlanırken hata: {ex.Message}");
            
            // Hata durumunda boş grafik göster
            ChartSeries = new SeriesCollection();
            ChartLabels = new string[0];
            ChartMaxValue = 10000;
        }
    }

    private void LoadDataForPeriod(int periodIndex)
    {
        // Periyot değiştiğinde grafik verilerini güncelle
        PrepareChartData(periodIndex);
    }
    
    /// <summary>
    /// Seçilen periyoda göre tarih aralığını hesaplar
    /// </summary>
    private (DateTime startDate, DateTime endDate) GetDateRangeForPeriod(int periodIndex)
    {
        var endDate = DateTime.Now;
        DateTime startDate;
        
        switch (periodIndex)
        {
            case 0: // Son 1 ay
                startDate = endDate.AddDays(-28);
                break;
            case 1: // Son 3 ay
                startDate = endDate.AddMonths(-3);
                break;
            case 2: // Son 6 ay
            default:
                startDate = endDate.AddMonths(-6);
                break;
        }
        
        return (startDate, endDate);
    }
    
    /// <summary>
    /// Seçilen periyoda göre dinamik X-ekseni etiketlerini oluşturur
    /// </summary>
    private string[] GetDynamicLabelsForPeriod(int periodIndex)
    {
        var today = DateTime.Now;
        var labels = new List<string>();
        
        switch (periodIndex)
        {
            case 0: // Son 1 ay - Haftalık aralıklar
                for (int i = 3; i >= 0; i--)
                {
                    var date = today.AddDays(-7 * i);
                    labels.Add($"{date.Day} {GetShortMonthName(date.Month)}");
                }
                break;
                
            case 1: // Son 3 ay - Aylık
                for (int i = 2; i >= 0; i--)
                {
                    var date = today.AddMonths(-i);
                    labels.Add(GetMonthName(date.Month));
                }
                break;
                
            case 2: // Son 6 ay - Aylık
            default:
                for (int i = 5; i >= 0; i--)
                {
                    var date = today.AddMonths(-i);
                    labels.Add(GetMonthName(date.Month));
                }
                break;
        }
        
        return labels.ToArray();
    }
    
    /// <summary>
    /// Türkçe ay ismi döndürür
    /// </summary>
    private string GetMonthName(int month)
    {
        string[] months = { 
            "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
            "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" 
        };
        return months[month - 1];
    }
    
    /// <summary>
    /// Kısa Türkçe ay ismi döndürür
    /// </summary>
    private string GetShortMonthName(int month)
    {
        string[] months = { 
            "Oca", "Şub", "Mar", "Nis", "May", "Haz",
            "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" 
        };
        return months[month - 1];
    }
    
    /// <summary>
    /// Y-ekseni için optimal maksimum değeri hesaplar
    /// </summary>
    private double CalculateOptimalMaxValue(IEnumerable<double> values)
    {
        if (!values.Any()) return 10000; // Varsayılan
        
        var maxValue = values.Max();
        
        // %20 boşluk ekle (görsel rahatlık için)
        var targetMax = maxValue * 1.2;
        
        // Yuvarlanmış değere çevir
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(targetMax)));
        var rounded = Math.Ceiling(targetMax / magnitude) * magnitude;
        
        return rounded;
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