using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nethesap.Domain.Entities;
using Nethesap.UI.Commands;

namespace Nethesap.UI.ViewModels
{
    public class RefundDialogViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RefundItemViewModel> _saleItems;
        private decimal _totalRefundAmount;
        private Payment _originalSale;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? RequestClose;

        public ObservableCollection<RefundItemViewModel> SaleItems
        {
            get => _saleItems;
            set
            {
                _saleItems = value;
                OnPropertyChanged(nameof(SaleItems));
            }
        }

        public decimal TotalRefundAmount
        {
            get => _totalRefundAmount;
            set
            {
                _totalRefundAmount = value;
                OnPropertyChanged(nameof(TotalRefundAmount));
            }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public RefundDialogViewModel(Payment sale)
        {
            _originalSale = sale;
            ConfirmCommand = new RelayCommand(ConfirmRefund);
            CancelCommand = new RelayCommand(CancelRefund);

            LoadItems();
        }

        private void LoadItems()
        {
            SaleItems = new ObservableCollection<RefundItemViewModel>();
            
            if (_originalSale?.PaymentItems != null)
            {
                foreach (var item in _originalSale.PaymentItems)
                {
                    // Calculate available quantity (Purchased - Already Refunded)
                    int available = item.Quantity - item.RefundedQuantity;
                    
                    if (available > 0)
                    {
                        var refundItem = new RefundItemViewModel(item, available);
                        refundItem.PropertyChanged += RefundItem_PropertyChanged;
                        SaleItems.Add(refundItem);
                    }
                }
            }
        }

        private void RefundItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RefundItemViewModel.RefundQuantity) || 
                e.PropertyName == nameof(RefundItemViewModel.IsSelected))
            {
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (var item in SaleItems)
            {
                if (item.IsSelected)
                {
                    total += item.RefundQuantity * item.UnitPrice;
                }
            }
            TotalRefundAmount = total;
        }

        private void ConfirmRefund(object obj)
        {
            if (TotalRefundAmount <= 0)
            {
                MessageBox.Show("Lütfen iade edilecek en az bir ürün seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Doğrulama: Seçili ama miktarı 0 olan var mı?
            var invalidItems = SaleItems.Where(x => x.IsSelected && x.RefundQuantity <= 0).ToList();
            if (invalidItems.Any())
            {
                MessageBox.Show("Seçili ürünlerin iade miktarı 0 olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void CancelRefund(object obj)
        {
            DialogResult = false;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public bool DialogResult { get; private set; }

        public List<PaymentItem> GetRefundItems()
        {
            // Sadece seçili olanları döndür
            // Note: We return new PaymentItem instances or a DTO to the service
            var result = new List<PaymentItem>();
            foreach (var item in SaleItems.Where(x => x.IsSelected))
            {
                result.Add(new PaymentItem
                {
                    Id = item.SourceItem.Id, // Original Item ID
                    ProductId = item.SourceItem.ProductId,
                    Quantity = item.RefundQuantity, // Count to refund
                    UnitPrice = item.UnitPrice,
                    // Diğer alanlar serviste işlenecek
                });
            }
            return result;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RefundItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private int _refundQuantity;
        
        public PaymentItem SourceItem { get; }
        public string ProductName => SourceItem.Product?.Name ?? "Bilinmeyen Ürün";
        public int PurchasedQuantity => SourceItem.Quantity;
        public decimal UnitPrice => SourceItem.UnitPrice;
        public int MaxRefundQuantity { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RefundItemViewModel(PaymentItem item, int maxRefundable)
        {
            SourceItem = item;
            MaxRefundQuantity = maxRefundable;
            RefundQuantity = maxRefundable; // Default to max
            IsSelected = false; // Default unselected
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                // Seçildiğinde miktarı max yap, kaldırıldığında da kalabilir
            }
        }

        public int RefundQuantity
        {
            get => _refundQuantity;
            set
            {
                if (value > MaxRefundQuantity) value = MaxRefundQuantity;
                if (value < 0) value = 0;
                
                _refundQuantity = value;
                OnPropertyChanged(nameof(RefundQuantity));
                
                // Miktar değişirse otomatik seçili yap (0 değilse)
                if (value > 0 && !IsSelected) IsSelected = true;
            }
        }

        public decimal TotalLinePrice => RefundQuantity * UnitPrice;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
