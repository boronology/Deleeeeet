using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deleeeeet.ViewModel
{    class TweetRow : ViewModelBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); _onSelectionChanged(); }
        }

        public string Id { get; }
        public long RawId => Tweet.Id;
        public string FullText => Tweet.FullText;

        public bool IsReply => Tweet.IsReply;
        public bool HasMedia => Tweet.HasMedia;
        public bool IsRetweet => Tweet.IsRetweet;
        public DateTime CreatedAt => Tweet.CreatedAt;
        private readonly Action _onSelectionChanged;

        public Model.Data.ITweet Tweet { get; }
        public TweetRow(Deleeeeet.Model.Data.ITweet tweet, Action onSelectionChanged)
        {
            _isSelected = true;
            Tweet = tweet;
            Id = tweet.Id.ToString();
            _onSelectionChanged = onSelectionChanged;
        }
    }
}
