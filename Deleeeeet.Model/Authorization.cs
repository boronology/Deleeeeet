using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using Deleeeeet.Model.Schema;

namespace Deleeeeet.Model
{
    internal class Authorization
    {
        private readonly string appUuid = "47BC88A7-65E6-46C5-8AEF-98834C7676DB";

        public delegate Task<CoreTweet.Tokens> ContinueFunction(string pinCode);

        private string GetDefaultSavePath()
        {
            string appDataPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appUuid);
            Directory.CreateDirectory(appDataPath);
            return Path.GetFullPath(Path.Combine(appDataPath, "twitterTokens.xml"));
        }

        public (Uri, ContinueFunction) AuthorizeOnline()
        {
            var session = CoreTweet.OAuth.Authorize(Secrets.CONSUMER_KEY, Secrets.CONSUMER_KEY_SECRET);
            ContinueFunction func = async (string str) =>
            {
                return  await CoreTweet.OAuth.GetTokensAsync(session, str);
            };
            return (session.AuthorizeUri, func);
        }


        public CoreTweet.Tokens LoadTokens(string filePath)
        {
            if(! File.Exists(filePath))
            {
                return null;
            }
            using var stream = File.OpenRead(filePath);
            return LoadTokens(stream);
        }

        public CoreTweet.Tokens LoadTokensFromDefault()
        {
            return LoadTokens(GetDefaultSavePath());
        }

        public CoreTweet.Tokens LoadTokens(Stream stream)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(TwitterAuthToken));
            var obj = serializer.ReadObject(stream);
            if (obj is TwitterAuthToken deleeeeetToken)
            {
                return CoreTweet.Tokens.Create(
                    Secrets.CONSUMER_KEY,
                    Secrets.CONSUMER_KEY_SECRET,
                    deleeeeetToken.AccessToken,
                    deleeeeetToken.AccessTokenSecret,
                    deleeeeetToken.UserId,
                    deleeeeetToken.ScreenName);
            }
            return null;
        }

        public bool SaveTokensToDefault(Tokens token)
        {
            return SaveTokens(token, GetDefaultSavePath());
        }

        public bool SaveTokens(Tokens tokens, string filePath)
        {
            using var stream = File.Create(filePath);
            return SaveTokens(tokens, stream); 

        }

        public bool SaveTokens(CoreTweet.Tokens token, Stream stream)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(TwitterAuthToken));
            var deleeeeetToken = new TwitterAuthToken
            {
                AccessToken = token.AccessToken,
                AccessTokenSecret = token.AccessTokenSecret,
                UserId = token.UserId,
                ScreenName = token.ScreenName,
            };
            try
            {
                serializer.WriteObject(stream, deleeeeetToken);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
            

        }


    }
}
