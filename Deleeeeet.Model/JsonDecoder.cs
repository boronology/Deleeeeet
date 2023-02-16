using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Deleeeeet.Model.Schema;

namespace Deleeeeet.Model
{

    public class JsonDecoder
    {
        public JsonDecoder() { }

        public Tweet[] LoadFromFile(string jsonFile)
        {
            if (jsonFile == null)
            {
                return null;
            }

            if (!File.Exists(jsonFile))
            {
                return null;
            }
            var jsonText = File.ReadAllText(jsonFile);
            
            if (jsonText.StartsWith("window"))
            {
                int eqIndex = jsonText.IndexOf("=");
                if (eqIndex != -1)
                {
                    jsonText = jsonText.Substring(eqIndex+ 1);
                }
            }
            return LoadFromJson(jsonText);
        }

        public Tweet[] LoadFromJson(string jsonText)
        {
            var obj = JsonArray.Parse(jsonText);
            return obj.AsArray().Select(node => ConvertNodeToTweet(node)).Where(x => x != null).ToArray();
        }

        private Tweet ConvertNodeToTweet(JsonNode node)
        {
            if (node == null)
            {
                return null;
            }
            
            try
            {
                return new Tweet(GetTweetId(node), GetFullText(node), HasMedia(node), GetIsReply(node), GetIsRetweet(node), GetCreatedAt(node));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private long GetTweetId(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }

            var idStrNode = tweetNode["id_str"];
            if (idStrNode == null)
            {
                var d = string.Join(",", tweetNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("id_str is null " + d);
            }
            var strId = idStrNode.AsValue().ToString();
            return long.TryParse(strId, out long longId) ? longId : 0;
        }

        private bool GetIsReply(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }

            //リプライ関連のプロパティがあればリプライとみなす
            if (tweetNode["in_reply_to_screen_name"] != null
                || tweetNode["in_reply_to_status_id_str"] != null
                || tweetNode["in_reply_to_user_id_str"] != null)
            {
                return true;
            }
            return false;
        }

        private bool GetIsRetweet(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }

            //tweet.entities.user_mentionsに要素があればRTとみなす
            var userMentionsNode = tweetNode["entities"]?["user_mentions"];
            if (userMentionsNode == null)
            {
                throw new Exception("node user_mentions not found");
            }
             return userMentionsNode.AsArray().FirstOrDefault() != null;
        }

        private string GetFullText(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }

            var fullTextNode = tweetNode["full_text"];
            if (fullTextNode == null)
            {
                throw new Exception("node full_text not found");
            }
            return fullTextNode.AsValue().ToString();
        }

        private bool HasMedia(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }
            //tweet.extended_entities.mediaがあればメディア添付ありとみなす
            var mediaNode = tweetNode["extended_entities"]?["media"];
            if (mediaNode == null)
            {
                return false;
            }
            return mediaNode.AsArray().FirstOrDefault() != null;
        }

        private DateTime GetCreatedAt(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException("jsonNode is null");
            }
            var tweetNode = jsonNode["tweet"];
            if (tweetNode == null)
            {
                var d = string.Join(",", jsonNode.AsObject().Select(x => x.Key).ToArray());
                throw new Exception("tweet is null " + d);
            }
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
    }
}
