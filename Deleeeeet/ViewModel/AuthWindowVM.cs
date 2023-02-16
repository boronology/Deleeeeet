using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Deleeeeet.ViewModel
{
    internal class AuthWindowVM : ViewModelBase
    {
        public string AuthUrl { get; }

        private string _pinCode = "";
        public string PinCode
        {
            get => _pinCode;
            set { _pinCode = value; OnPropertyChanged(); }
        }

        public DelegateCommand OkCommand { get; }

        public AuthWindowVM(Window window, string url)
        {
            AuthUrl = $"ブラウザで表示されたPINコードを真ん中の枠内に入力してください\r\nブラウザが表示されない場合は以下のURLをコピーしてブラウザで開いてください。\r\n\r\n{url}";
            OkCommand = new DelegateCommand((obj) =>
            {
                window.Close();
            }, (obj) =>
            {
                return PinCode != null && PinCode.Length == 7;
            });
        }
    }
}
