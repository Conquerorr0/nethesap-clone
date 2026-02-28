using System;
using System.Windows;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI.Views
{
    public partial class RefundDialog : Window
    {
        public RefundDialog()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (DataContext is RefundDialogViewModel vm)
            {
                vm.RequestClose += (s, args) => 
                {
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                };
            }
        }
    }
}
