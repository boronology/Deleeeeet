using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{

    public class HelperTest
    {
        [Theory]
        [InlineData("Wed Feb 01 23:24:45 +0000 2023", 2023, 2, 1, 23, 24, 45)]
        [InlineData("Wed Oct 19 11:24:43 +0000 2022", 2022, 10, 19, 11, 24, 43)]
        [InlineData("Sun Jul 03 07:32:32 +0000 2022", 2022, 7, 3, 7, 32, 32)]
        [InlineData("Thu Dec 08 14:51:28 +0000 2022", 2022, 12, 8, 14, 51, 28)]
        public void ParseCreatedAtTest(string testData, int year, int month, int day, int hour, int minute, int second)
        {
            bool result = Deleeeeet.Model.DecodeHelper.TryParseCreatedAt(testData, out var createdAt);
            Assert.True(result);
            Assert.Equal(year, createdAt.Year);
            Assert.Equal(month, createdAt.Month);
            Assert.Equal(day, createdAt.Day);
            Assert.Equal(hour, createdAt.Hour);
            Assert.Equal(minute, createdAt.Minute);
            Assert.Equal(second, createdAt.Second);
        }

    }
}
