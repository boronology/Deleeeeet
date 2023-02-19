using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Deleeeeet.Model.Schema;

namespace Deleeeeet.Model
{
    public class JsonLoader : ILoader
    {
        public bool VerifyFormat(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            if (!ext.Equals(".JS", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            try
            {
                string text = File.ReadAllText(filePath);
                return VerifyFormatInternal(text);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyFormat(Stream stream)
        {
            try
            {
                using var sr = new StreamReader(stream);
                return VerifyFormatInternal(sr.ReadToEnd());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// window.YTD.tweets.part0 = を除去する
        /// </summary>
        /// <param name="tweetsJsText"></param>
        /// <returns></returns>
        private string RemoveJsHeader(string tweetsJsText)
        {
            if (!tweetsJsText.StartsWith("window"))
            {
                return tweetsJsText;
            }
            int eqIndex = tweetsJsText.IndexOf("=");
            if (eqIndex != -1)
            {
                return tweetsJsText.Substring(eqIndex + 1);
            }
            return tweetsJsText;
        }

        private bool VerifyFormatInternal(string jsonText)
        {
            try
            {
                var node = JsonArray.Parse(RemoveJsHeader(jsonText));
                return node.AsArray() != null;
            }
            catch
            {
                return false;
            }
        }

        public Tweet[] LoadTweets(string jsFile)
        {
            if (jsFile == null)
            {
                return null;
            }

            if (!File.Exists(jsFile))
            {
                return null;
            }
            var jsText = File.ReadAllText(jsFile);

            return LoadFromJson(jsText);
        }

        public Tweet[] LoadTweets(Stream stream)
        {
            using var sr = new StreamReader(stream);
            return LoadFromJson(sr.ReadToEnd());
        }

        private Tweet[] LoadFromJson(string jsText)
        {
            var jsonText = RemoveJsHeader(jsText);
            var obj = JsonArray.Parse(jsonText);
            return obj.AsArray().Select(node => ConvertNodeToTweet(node)).Where(x => x != null).ToArray();
        }

        private Tweet ConvertNodeToTweet(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                return null;
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                throw new Exception("node does not have \"tweet\" node");
            }
            try
            {
                long tweetId = GetTweetId(tweetNode);
                bool isReply = GetIsReply(tweetNode);
                bool isRetweet = GetIsRetweet(tweetNode);
                string fullText = GetFullText(tweetNode);
                bool hasMedia = GetHasMedia(tweetNode);
                DateTime createdAt = GetCreatedAt(tweetNode);
                return new Tweet(tweetId, fullText, hasMedia, isReply, isRetweet, createdAt);
            }
            catch
            {
                return null;
            }
        }

        #region load properties
        private long GetTweetId(JsonNode tweetNode)
        {

            var idStrNode = tweetNode["id_str"];
            if (idStrNode == null)
            {
                throw new Exception("id_str is null");
            }
            var strId = idStrNode.AsValue().ToString();
            return long.TryParse(strId, out long longId) ? longId : throw new Exception("id_str must be long type");
        }

        private bool GetIsReply(JsonNode tweetNode)
        {
            //リプライ関連のプロパティがあればリプライとみなす
            if (tweetNode["in_reply_to_screen_name"] != null
                || tweetNode["in_reply_to_status_id_str"] != null
                || tweetNode["in_reply_to_user_id_str"] != null)
            {
                return true;
            }
            return false;
        }


        private bool GetIsRetweet(JsonNode tweetNode)
        {
            //tweet.entities.user_mentionsに要素があればRTとみなす
            var userMentionsNode = tweetNode["entities"]?["user_mentions"];
            if (userMentionsNode == null)
            {
                throw new Exception("node user_mentions not found");
            }
            return userMentionsNode.AsArray().FirstOrDefault() != null;
        }

        private string GetFullText(JsonNode tweetNode)
        {
            var fullTextNode = tweetNode["full_text"];
            if (fullTextNode == null)
            {
                throw new Exception("node full_text not found");
            }
            return fullTextNode.AsValue().ToString();
        }

        private bool GetHasMedia(JsonNode tweetNode)
        {
            //tweet.extended_entities.mediaがあればメディア添付ありとみなす
            var mediaNode = tweetNode["extended_entities"]?["media"];
            if (mediaNode == null)
            {
                return false;
            }
            return mediaNode.AsArray().FirstOrDefault() != null;
        }

        private DateTime GetCreatedAt(JsonNode tweetNode)
        {

            var createdAtNode = tweetNode["created_at"];
            if (createdAtNode == null)
            {
                throw new Exception("created_at not found");
            }
            string strValue = createdAtNode.AsValue().ToString();
            if (DecodeHelper.TryParseCreatedAt(strValue, out DateTime createdAt))
            {
                return createdAt;
            }
            throw new Exception($"invalid datetime format \"{strValue}\"");
        }

        #endregion
    }
}
