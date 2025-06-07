using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nethesap.UI.ViewModels
{
    public class AccountsViewModel : INotifyPropertyChanged
    {
        private string _userName;
        private string _fullName;
        private string _email;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public AccountsViewModel()
        {
            // Örnek veriler
            UserName = "admin";
            FullName = "Admin Kullanıcı";
            Email = "admin@nethesap.com";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 