using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Deleeeeet.Model.Data;

namespace Deleeeeet.ViewModel
{
    internal class DeleteWindowVM : ViewModelBase
    {
        private readonly Model.ActionModel _model;
        private readonly Window _window;
        private bool _deleting;
        public bool Deleting
        {
            get => _deleting;
            set { _deleting = value; OnPropertyChanged(); }
        }
        public ObservableCollection<DeleteRow> DeleteRows { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        private int _total = 1;
        public int Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged();}
        }


        public DeleteWindowVM(Window window, Model.ActionModel model, IEnumerable<ITweet> tweets) 
        {
            _window = window;
            _model= model;
            DeleteRows = new ObservableCollection<DeleteRow>(tweets.Select(x => new DeleteRow(x)));

            DeleteCommand = new DelegateCommand(async (arg) =>
            {
                var result = MessageBox.Show("一度削除するともとには戻せません。本当に削除しますか？", "", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
                await _model.DeleteTweets(tweets.Select(x => x.Id).ToArray(), true);
            }, (arg) => !Deleting);
            CancelCommand = new DelegateCommand((arg) =>
            {
                _model.CancelDelete();
            }, (arg) => Deleting);

            RegisterEvent();
            _window.Closed += UnregisterEvent;
        }

        private void UnregisterEvent(object? sender, EventArgs e)
        {
            _model.OnDeleteStarted -= _model_OnDeleteStarted;
            _model.OnDeleted -= _model_OnDeleted;
            _model.OnDeleteEnded -= _model_OnDeleteEnded;
            _model.OnProgress -= _model_OnProgress;
            _model.OnException -= _model_OnException;
        }

        private void RegisterEvent()
        {
            _model.OnDeleteStarted += _model_OnDeleteStarted;
            _model.OnDeleted += _model_OnDeleted;
            _model.OnDeleteEnded += _model_OnDeleteEnded;
            _model.OnProgress += _model_OnProgress;
            _model.OnException += _model_OnException;
        }


        private void _model_OnException(Exception arg2)
        {
            MessageBox.Show($"例外が発生したため中止\r\n{arg2}");
        }

        private void _model_OnProgress(int count, int total)
        {
            Progress = count;
            Total = total;
        }

        private void _model_OnDeleteEnded(int deleted, int failed, int notfound, long[] ids)
        {
            MessageBox.Show($"削除済み{notfound}件\r\n削除{deleted}件\r\n失敗{failed}件");
            Deleting = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void _model_OnDeleted(long id, bool deleted)
        {
            if (deleted)
            {
                var item = DeleteRows.FirstOrDefault(x => x.Id== id);
                if (item != null)
                {
                    item.IsDeleted = true;
                }
            }
        }

        private void _model_OnDeleteStarted()
        {
            Deleting = true;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    class DeleteRow : ViewModelBase
    {
        public long Id => _tweet.Id;
        public string FullText => _tweet.FullText;

        private bool _isDeleted;
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; OnPropertyChanged(); }
        }

        private readonly ITweet _tweet;

        public DeleteRow(ITweet tweet)
        {
            IsDeleted= false;
            _tweet = tweet;
        }
    }
}
