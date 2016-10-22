namespace SunCalcDotNet.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class BasicApiTests
    {
        private readonly DateTimeOffset date = new DateTimeOffset(2016, 10 , 16, 13, 37, 00, TimeSpan.FromHours(10));

        private readonly double latitude = -27.477320;

        private readonly double longitude;

        public BasicApiTests()
        {
//            SunCalc.AddTime(new TimeMap(3, "MorningName", "EveningName"));
//
//            this.longitude = 153.027086;
//
//            SunCalc.GetMoonIllumination(this.date);
//
//            SunCalc.GetMoonPosition(this.date, this.latitude, this.longitude);
//
//            SunCalc.GetMoonTimes(this.date, this.latitude, this.longitude, false);
//
//            SunCalc.GetPosition(this.date, this.latitude, this.longitude);
//
//            SunCalc.GetTimes(this.date, this.latitude, this.longitude);

        }

        [Fact]
        public void GetTimesTest()
        {
            //var result = SunCalc.GetTimes(this.date, this.latitude, this.longitude);


            Xunit.Assert.Equal(4, 2 + 2);
        }
    }
}
