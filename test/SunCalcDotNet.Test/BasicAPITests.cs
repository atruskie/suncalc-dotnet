namespace SunCalcDotNet.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Xunit;

    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class BasicApiTests
    {
        private static readonly TimeSpan kilo = TimeSpan.FromHours(10);

        
        // http://suncalc.net/#/-27.4774,153.0271,18/2016.10.10/13:37
        private readonly DateTimeOffset date = new DateTimeOffset(2016, 11 , 16, 13, 37, 00, kilo);

        private readonly double latitude = -27.4774;

        private readonly double longitude = 153.0271;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> map = new Dictionary<string, DateTimeOffset>();
                                                                    

        private const string GetTimesResults = @"{
  ""solarNoon"": ""2016-11-16T01:34:04.920Z"",
  ""nadir"": ""2016-11-15T13:34:04.920Z"",
  ""sunrise"": ""2016-11-15T18:49:24.358Z"",
  ""sunset"": ""2016-11-16T08:18:45.483Z"",
  ""sunriseEnd"": ""2016-11-15T18:51:59.373Z"",
  ""sunsetStart"": ""2016-11-16T08:16:10.468Z"",
  ""dawn"": ""2016-11-15T18:24:03.382Z"",
  ""dusk"": ""2016-11-16T08:44:06.459Z"",
  ""nauticalDawn"": ""2016-11-15T17:53:46.047Z"",
  ""nauticalDusk"": ""2016-11-16T09:14:23.793Z"",
  ""nightEnd"": ""2016-11-15T17:22:12.122Z"",
  ""night"": ""2016-11-16T09:45:57.719Z"",
  ""goldenHourEnd"": ""2016-11-15T19:22:09.713Z"",
  ""goldenHour"": ""2016-11-16T07:46:00.127Z""
}";


        public BasicApiTests()
        {
            var times = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(GetTimesResults);
            foreach (var time in times)
            {
                this.map.Add(time.Key, new DateTimeOffset(time.Value, TimeSpan.Zero).ToOffset(kilo));
            }



            // map.Add("sunrise", new DateTimeOffset());
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
            var result = SunCalc.GetTimes(this.date, this.latitude, this.longitude);

            foreach (var kvp in this.map)
            {
                var key = kvp.Key;
                var expected = kvp.Value;

                Assert.Equal(expected, result.Times[key]);
            }
        }
    }
}
