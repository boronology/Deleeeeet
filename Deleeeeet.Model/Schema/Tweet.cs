using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deleeeeet.Model.Schema
{
    public class Tweet
    {
        public long Id { get;  }
        public string FullText { get;  }
        public bool HasMedia { get;  }

        public bool IsReply { get;  }
        public bool IsRetweet { get;  }

        public DateTime CreatedAt { get; }

        public Tweet(long id, string fullText, bool hasMedia, bool isReply, bool isRetweet, DateTime createdAt)
        {
            Id = id;
            FullText = fullText;
            HasMedia = hasMedia;
            IsReply = isReply;
            IsRetweet = isRetweet;
            CreatedAt = createdAt;
        }
    }

}
