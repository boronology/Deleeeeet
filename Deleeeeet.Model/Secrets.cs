using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deleeeeet.Model
{
    internal static class Secrets
    {
        public static readonly string CONSUMER_KEY;
        public static readonly string CONSUMER_KEY_SECRET;
#if LOCALBUILD
        private const string STREAM_NAME = "Deleeeeet.Model.SecretKey.secret.json";
#else
        private const string STREAM_NAME = "Deleeeeet.Model.SecretKey.json";
#endif

        static Secrets()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(STREAM_NAME);
            using var sr = new StreamReader(stream);
            string s = sr.ReadToEnd();
            var tokens = JsonSerializer.Deserialize<Schema.SecretToken>(s);
            if (tokens == null)
            {
                throw new Exception("SecretKey is null");
            }
            if (string.IsNullOrEmpty(tokens.ConsumerKey) || string.IsNullOrEmpty(tokens.ConsumerKeySecret))
            {
                throw new Exception("Invalid SecretKey");
            }
            CONSUMER_KEY = tokens.ConsumerKey;
            CONSUMER_KEY_SECRET = tokens.ConsumerKeySecret;
        }
    }
}