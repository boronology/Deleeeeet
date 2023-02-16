using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace Deleeeeet.Model.Schema
{
    [DataContract]
    internal class TwitterAuthToken
    {
        [DataMember]
        public string AccessToken { get; set; }
        [DataMember]
        public string AccessTokenSecret { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember] 
        public string ScreenName { get; set; }
        


    }
}
