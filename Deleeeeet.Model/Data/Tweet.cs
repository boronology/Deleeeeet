using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deleeeeet.Model.Data
{
    public interface ITweet
    {
        public long Id { get; }
        public string FullText { get; }
        public bool IsReply { get; }
        public bool HasMedia { get; }
        public bool IsRetweet { get; }
        public DateTime CreatedAt { get; }
    }

    internal class Tweet : ITweet
    {
        public long Id { get; }

        public string FullText { get; }

        public bool IsReply { get; }

        public bool HasMedia { get; }

        public bool IsRetweet { get; }

        public DateTime CreatedAt { get; }

        public Tweet(long id, string fullText, bool isReply, bool hasMedia, bool isRetweet, DateTime createdAt)
        {
            this.Id = id; ;
            this.FullText = fullText;
            this.IsReply = isReply;
            this.HasMedia = hasMedia;
            this.IsRetweet = isRetweet;
            this.CreatedAt = createdAt;
        }
    }
}

