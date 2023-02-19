using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deleeeeet.Model.Schema;

namespace Deleeeeet.Model
{
    public interface ILoader
    {
        bool VerifyFormat(string filePath);
        bool VerifyFormat(Stream stream);

        Tweet[] LoadTweets(string filePath);
        Tweet[] LoadTweets(Stream stream);

    }
}
