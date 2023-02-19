using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deleeeeet.Model;

namespace TestProject1
{
    public class JsonLoaderTest
    {
        [Fact]
        public void JsonLoaderVerifyTest()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\testdata\testdata.js");
            var jsonLoader = new JsonLoader();
            Assert.True(jsonLoader.VerifyFormat(path));
        }

        [Fact]
        public void JsonLoaderVerifyFailTest() 
        {
            const string invalidText  = "{ key : \"value\" }";
            var jsonLoader = new JsonLoader();
            Assert.False(jsonLoader.VerifyFormat(new MemoryStream(Encoding.UTF8.GetBytes(invalidText))));
        }

        [Fact]
        public void JsonLoaderVerifyMinimumTest()
        {
            const string minimumFormat = "[]";
            var jsonLoader = new JsonLoader();
            Assert.True(jsonLoader.VerifyFormat(new MemoryStream(Encoding.UTF8.GetBytes(minimumFormat))));
        }
    }
}
