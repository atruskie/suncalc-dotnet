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


        private static readonly Position GetPositionResult = new Position()
            {
                Azimuth = 1.752764615783961,
                Altitude = 1.0532037951926414
            };

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

        private const string AddTimesResults = @"{
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
  ""goldenHour"": ""2016-11-16T07:46:00.127Z"",
  ""eveningTest"": ""2016-11-15T19:19:47.372Z"",
  ""morningTest"": ""2016-11-16T07:48:22.468Z""
}";

        private static readonly MoonIllumination MoonIlluminationResults = new MoonIllumination()
            {
                Fraction = 0.9633750476377128,
                Phase = 0.5612951343240598,
                Angle = 1.6879465154894437
            };

        private static readonly MoonPosition MoonPositionResults = new MoonPosition()
            {
                Azimuth = -0.6471761888421219,
                Altitude = -1.358355246220925,
                Distance = 365760.15309179353
            };

        private const string GetMoonTimesResults = @"
{
  ""rise"":""2016-11-16T10:00:49.263Z"",
  ""set"":""2016-11-15T20:11:43.491Z""
}";


        public BasicApiTests()
        {
        }

        [Fact]
        public void GetPositionTest()
        {
            var actual = SunCalc.GetPosition(this.date, this.latitude, this.longitude);

            Assert.Equal(GetPositionResult.Altitude, actual.Altitude);
            Assert.Equal(GetPositionResult.Azimuth, actual.Azimuth);
        }

        [Fact]
        public void GetTimesTest()
        {
            var map = AddTimeZone(JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(GetTimesResults));
           
            var result = SunCalc.GetTimes(this.date, this.latitude, this.longitude);

            foreach (var kvp in map)
            {
                var key = kvp.Key;
                var expected = kvp.Value;

                Assert.Equal(expected, result.Times[key]);
            }
        }

        [Fact]
        public void AddTimeTest()
        {
            SunCalc.AddTime(new TimeMap(5.5, "morningTest",  "eveningTest"));
            
            var map = AddTimeZone(JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(AddTimesResults));

            var result = SunCalc.GetTimes(this.date, this.latitude, this.longitude);

            foreach (var kvp in map)
            {
                var key = kvp.Key;
                var expected = kvp.Value;

                Assert.Equal(expected, result.Times[key]);
            }
        }

        [Fact]
        public void GetMoonIlluminationTest()
        {
            var actual = SunCalc.GetMoonIllumination(this.date);

            Assert.Equal(MoonIlluminationResults.Angle, actual.Angle);
            Assert.Equal(MoonIlluminationResults.Fraction, actual.Fraction);
            Assert.Equal(MoonIlluminationResults.Phase, actual.Phase);
        }

        [Fact]
        public void GetMoonPositionTest()
        {
            var actual = SunCalc.GetMoonPosition(this.date, this.latitude, this.longitude);

            Assert.Equal(MoonPositionResults.Altitude, actual.Altitude);
            Assert.Equal(MoonPositionResults.Azimuth, actual.Azimuth);
            Assert.Equal(MoonPositionResults.Distance, actual.Distance);
        }

        [Fact]
        public void GetMoonTimesTest()
        {
            var actual = SunCalc.GetMoonTimes(this.date, this.latitude, this.longitude);

            var expected = AddTimeZone(JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(GetMoonTimesResults));

            Assert.NotNull(actual.Rise);
            Assert.Equal(expected["rise"], actual.Rise.Value);
            Assert.False(actual.AlwaysDown);

            Assert.NotNull(actual.Set);
            Assert.Equal(expected["set"], actual.Set.Value);
            Assert.False(actual.AlwaysUp);
        }

        private static Dictionary<string, DateTimeOffset> AddTimeZone(Dictionary<string, DateTime> map)
        {
            var addTimesMap = new Dictionary<string, DateTimeOffset>();
            foreach (var time in map)
            {
                addTimesMap.Add(time.Key, new DateTimeOffset(time.Value, TimeSpan.Zero).ToOffset(kilo));
            }
            return addTimesMap;
        }
    }
}
