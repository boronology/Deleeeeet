using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class LoadJsonTest
    {
        [Fact]
        public void LoadTestDataTest()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\testdata\testdata.js");
            Assert.True(File.Exists(path));

            var model = new Deleeeeet.Model.JsonLoader();
            var data = model.LoadTweets(path);
            Assert.NotNull(data);
            Assert.Equal(3, data.Length);
            Assert.Equal(1582692792211382272L, data[0].Id);
            Assert.False(data[0].IsRetweet);
            Assert.Equal("続いてはノートに1TBのストレージは必要かで頭を悩ませている。必要なら外付けSSDでいいのでは？", data[0].FullText);
            Assert.False(data[0].HasMedia);
            Assert.Equal(11, data[0].CreatedAt.Hour);

            Assert.Equal(1571833549074100229L, data[1].Id);
            Assert.Equal("5000兆連休と5000阿僧祇円ほしい", data[1].FullText);

            Assert.Equal(1558810721500405760L, data[2].Id);
            Assert.True(data[2].HasMedia);

        }
    }
}
