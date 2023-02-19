using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Deleeeeet.Model.Schema;

namespace Deleeeeet.Model
{
    public class ZipLoader : ILoader
    {
        private readonly ILoader _jsonLoader;
        internal ZipLoader(ILoader jsonLoader)
        {
            _jsonLoader = jsonLoader;
        }

        public Tweet[] LoadTweets(string filePath)
        {
            var archive = ZipFile.OpenRead(filePath);
            var entry = GetTweetsJsEntry(archive);
            return LoadTweetsInternal(archive);
        }

        public Tweet[] LoadTweets(Stream stream)
        {
            var archive = new ZipArchive(stream);
            return LoadTweetsInternal(archive);
        }

        private Tweet[] LoadTweetsInternal(ZipArchive archive)
        {
            var entry = GetTweetsJsEntry(archive);
            if (entry == null)
            {
                return null;
            }
            using var stream = entry.Open();
            return _jsonLoader.LoadTweets(stream);
        }

        public bool VerifyFormat(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            if (!ext.Equals(".ZIP", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            try
            {
                var archive = ZipFile.OpenRead(filePath);
                return VerifyFormatInternal(archive);
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
                var archive = new ZipArchive(stream);
                return VerifyFormatInternal(archive);
            }
            catch
            {
                return false;
            }

        }

        private ZipArchiveEntry? GetTweetsJsEntry(ZipArchive archive)
        {
            return archive.Entries.FirstOrDefault(x => x.FullName == "data/tweets.js");
        }

        private bool VerifyFormatInternal(ZipArchive archive)
        {
            var jsEntry = GetTweetsJsEntry(archive);
            return jsEntry != null;
        }
    }
}
