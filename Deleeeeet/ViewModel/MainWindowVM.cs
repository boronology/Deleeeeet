using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CoreTweet.Rest;
using Deleeeeet.View;
using Microsoft.Win32;

namespace Deleeeeet.ViewModel
{
    internal class MainWindowVM : ViewModelBase
    {
        public bool IsAllChecked
        {
            get
            {
                return _tweets.All(x => x.IsSelected);
            }
            set
            {
                _surpressUpdate = true;
                foreach (var t in _tweets)
                {
                    t.IsSelected = value;
                }
                _surpressUpdate = false;
                UpdateCount();
                OnPropertyChanged();
            }
        }

        #region collection
        public ICollectionView Tweets => _source.View;
        private readonly CollectionViewSource _source;
        private readonly ObservableCollection<TweetRow> _tweets;

        #endregion

        #region stats
        public int TotalCount => _tweets.Count;
        public int CheckedCount => _tweets.Where(x => x.IsSelected).Count();
        public int FilteredCount => _tweets.Where(x => OmitFilter(x)).Count();
        public int ToDeleteCount
        {
            get
            {
                return _tweets.Where(x => x.IsSelected && OmitFilter(x)).Count();
            }
        }
        private bool _surpressUpdate = false;
        private void UpdateCount()
        {
            if (_surpressUpdate)
            {
                return;
            }
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CheckedCount));
            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(ToDeleteCount));
            OnPropertyChanged(nameof(IsAllChecked));
            Tweets.Filter = (obj) =>
            {
                return obj is TweetRow row && OmitFilter(row);
            };
        }
        #endregion

        #region filter
        private bool _isOmitRetweetChecked;
        public bool IsOmitRetweetChecked
        {
            get => _isOmitRetweetChecked;
            set { _isOmitRetweetChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private bool _isOmitReplyChecked;
        public bool IsOmitReplyChecked
        {
            get => _isOmitReplyChecked;
            set { _isOmitReplyChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private bool _isOmitMediaChecked;
        public bool IsOmitMediaChecked
        {
            get => _isOmitMediaChecked;
            set { _isOmitMediaChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private bool _isDateFilterChecked;
        public bool IsDateFilterChecked
        {
            get => _isDateFilterChecked;
            set { _isDateFilterChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private bool _isUseNewerTimelimitChecked;
        public bool IsUseNewerTimelimitChecked
        {
            get => _isUseNewerTimelimitChecked;
            set { _isUseNewerTimelimitChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private bool _isUseOlderTimelimitChecked;
        public bool IsUseOlderTimelimitChecked
        {
            get => _isUseOlderTimelimitChecked;
            set { _isUseOlderTimelimitChecked = value; OnPropertyChanged(); UpdateCount(); }
        }
        private DateTime? _newerTimelimit;
        public DateTime? NewerTimelimit
        {
            get => _newerTimelimit;
            set { _newerTimelimit = value; OnPropertyChanged(); UpdateCount(); }
        }
        private DateTime? _olderTimelimit;
        public DateTime? OlderTimelimit
        {
            get => _olderTimelimit;
            set { _olderTimelimit = value; OnPropertyChanged(); UpdateCount(); }
        }

        private bool OmitFilter(TweetRow arg)
        {
            List<Predicate<TweetRow>> filters = new List<Predicate<TweetRow>>();
            if (_isOmitRetweetChecked)
            {
                filters.Add((arg) =>
                {
                    return !arg.IsRetweet;
                });
            }
            if (_isOmitReplyChecked)
            {
                filters.Add((arg) =>
                {
                    return !arg.IsReply;
                });
            }
            if (_isOmitMediaChecked)
            {
                filters.Add((arg) =>
                {
                    return !arg.HasMedia;
                });
            }
            if (_isDateFilterChecked)
            {
                if (_isUseNewerTimelimitChecked && _newerTimelimit != null)
                {
                    filters.Add((arg) =>
                    {
                        return arg.CreatedAt < _newerTimelimit;
                    });
                }
                if (_isUseOlderTimelimitChecked && _olderTimelimit != null)
                {
                    filters.Add((arg) =>
                    {
                        return arg.CreatedAt > _olderTimelimit;
                    });
                }
            }

            return filters.All((cond) => cond(arg));
        }

        #endregion
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand LoadCommand { get; }

        private readonly Model.ActionModel _model;
        public MainWindowVM(Model.ActionModel model)
        {
            _model = model;
            _tweets = new ObservableCollection<TweetRow>();
            _source = new CollectionViewSource { Source = _tweets };
            InitEvent();

            DeleteCommand = new DelegateCommand((obj) =>
            {
                var targets = this.Tweets.OfType<TweetRow>().Where(x => x.IsSelected).Select(x => x.Tweet);
                if (!targets.Any())
                {
                    MessageBox.Show("対象なし");
                    return;
                }

                var win = new DeleteWindow();
                var vm = new DeleteWindowVM(win, _model, targets);
                win.DataContext = vm;
                win.ShowDialog();
            }, (obj) =>
            {
                return _tweets.Count > 0;
            });
            LoadCommand = new DelegateCommand(async (obj) =>
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "twitter archive|*.js;*.zip",
                    Multiselect = false
                };
                var result = ofd.ShowDialog();
                if (result != true || ofd.FileName == null)
                {
                    return;
                }
                await _model.LoadTweetJs(ofd.FileName);
            });
        }

        private void InitEvent()
        {
            _model.OnGetAuthUri += OnGetAuthUri;
            _model.OnFailAuthorize += _model_OnFailAuthorize;
            _model.OnAuthorized += _model_OnAuthorized;
            _model.OnQueryUseSavedToken += _model_OnQueryUseSavedToken;

            _model.OnLoadTweetJsStarted += _model_OnLoadTweetJsStarted;
            _model.OnVerifyTweetJsFailed += _model_OnVerifyTweetJsFailed;
            _model.OnLoadTweetJsFailed += _model_OnLoadTweetJsFailed;
            _model.OnTweetJsLoaded += _model_OnTweetJsLoaded;
        }

        private void _model_OnVerifyTweetJsFailed()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("ファイルの形式が正しくありません");
            });
        }

        public async void Win_Loaded(object sender, RoutedEventArgs e)
        {
            await _model.Authorize();
        }

        private void _model_OnLoadTweetJsFailed()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("読み込みに失敗しました。おそらくファイルが破損しています。再度アーカイブをダウンロードして試してみてください。");
            });
        }

        private void _model_OnQueryUseSavedToken(Model.Event.QueryUseSavedTokenEventArgs eventArgs)
        {
            var result = MessageBox.Show("保存された認証情報を使いますか？", "", MessageBoxButton.YesNo);
            eventArgs.UseSavedToken = result == MessageBoxResult.Yes;
        }

        private void _model_OnTweetJsLoaded(Model.Data.ITweet[] obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var tweet in obj)
                {
                    _tweets.Add(new TweetRow(tweet, UpdateCount));
                }
                UpdateCount();
            });
        }

        private void _model_OnLoadTweetJsStarted()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this._tweets.Clear();
            });
        }


        private void _model_OnAuthorized()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show("認証成功!");
            });
        }

        private void _model_OnFailAuthorize(string obj)
        {
#if DEBUG
            System.Windows.MessageBox.Show(obj);
#else
            System.Windows.MessageBox.Show("認証失敗");
#endif
            App.Current.Shutdown();
        }

        private void OnGetAuthUri(Uri uri, Model.Event.RequirePinCodeEventArgs args)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = uri.AbsoluteUri,
                });
            }
            catch { }

            var win = new View.AuthWindow();
            var vm = new ViewModel.AuthWindowVM(win, uri.AbsoluteUri);
            win.DataContext = vm;
            win.ShowDialog();
            args.PinCode = vm.PinCode;

        }
    }

}
