using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deleeeeet.Model
{
    public class DecodeHelper
    {
        public static bool TryParseCreatedAt(string data, out DateTime createdAt)
        {
            return DateTime.TryParseExact(data, "ddd MMM dd HH:mm:ss zzz yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out createdAt);
        }
    }
}
