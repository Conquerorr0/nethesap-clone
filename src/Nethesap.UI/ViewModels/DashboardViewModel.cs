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
            // Tüm verileri çek
            var allPayments = await _saleService.GetAllSalesAsync();
            
            // Finansal metrikleri hesapla
            if (allPayments != null && allPayments.Any())
            {
                // Satışlar ve İadeler
                var sales = allPayments.Where(s => s.PaymentType == PaymentType.Sale).ToList();
                var refunds = allPayments.Where(s => s.PaymentType == PaymentType.Refund).ToList();
                
                // Toplamlar (Brüt)
                decimal grossSales = sales.Sum(s => s.TotalAmount);
                decimal totalRefunds = refunds.Sum(s => Math.Abs(s.TotalAmount)); // Refund tutarları negatiftir, mutlak değer al
                
                // Net Bakiye: Toplam Satış - Toplam İade
                TotalBalance = grossSales - totalRefunds;
                
                // Toplam Alacak: Ödenen Kısım (Satışlardan ödenen - İade edilen nakit/kart)
                // Not: Basitleştirilmiş varsayım - İade anında ödenmiş (paid) kabul ediyoruz.
                decimal grossPaid = sales.Sum(s => s.PaidAmount);
                decimal refundPaid = refunds.Sum(s => Math.Abs(s.PaidAmount));
                TotalReceivables = grossPaid - refundPaid;
                
                // Toplam Borç: Kalan (Satışların borcu) - (İadelerin 'kalanı' genelde 0'dır ama borç düşüldüyse hesaba katılmalı)
                // İade işlemi borçtan düşüyorsa, transaction bazında bakmak daha doğru olur ama basitçe:
                TotalPayables = sales.Sum(s => s.RemainingAmount); 
            }
            else
            {
                TotalBalance = 0;
                TotalReceivables = 0;
                TotalPayables = 0;
            }

            // Son işlemleri yükle
            var transactions = allPayments.OrderByDescending(p => p.CreatedDate)
                                          .Take(10)
                                          .Select(p => new TransactionItem
                                          {
                                              Description = p.Customer != null ? $"{p.Customer.FirstName} {p.Customer.LastName}" : (p.Description ?? "İsimsiz İşlem"),
                                              Date = p.CreatedDate,
                                              Amount = p.PaymentType == PaymentType.Refund ? -Math.Abs(p.TotalAmount) : p.TotalAmount
                                          });
            
            RecentTransactions = new ObservableCollection<TransactionItem>(transactions);

            // Grafik verilerini ilk periyot için hazırla
            PrepareChartData(SelectedPeriodIndex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dashboard verileri yüklenirken hata: {ex.Message}");
        }
    }

    private async void PrepareChartData(int periodIndex)
    {
        try
        {
            ChartLabels = GetDynamicLabelsForPeriod(periodIndex);
            var (startDate, endDate) = GetDateRangeForPeriod(periodIndex);
            
            // Veritabanından verileri çek (Satış + İade)
            var payments = await _saleService.GetSalesByDateRangeAsync(startDate, endDate);
            
            int dataPointCount = ChartLabels.Length;
            var incomeValues = new ChartValues<double>();
            var expenseValues = new ChartValues<double>();
            
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
                
                // Periyottaki işlemler
                var periodPayments = payments.Where(p => 
                    p.CreatedDate >= periodStart && 
                    p.CreatedDate < periodEnd
                ).ToList();
                
                // Gelir: (Satışlar) - (İadeler)
                double periodSales = (double)periodPayments.Where(p => p.PaymentType == PaymentType.Sale).Sum(s => s.TotalAmount);
                double periodRefunds = (double)periodPayments.Where(p => p.PaymentType == PaymentType.Refund).Sum(s => Math.Abs(s.TotalAmount));
                
                double netIncome = periodSales - periodRefunds;
                if (netIncome < 0) netIncome = 0;
                
                // Gider: Tahmini %60
                double expense = netIncome * 0.6;
                
                incomeValues.Insert(0, netIncome); 
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