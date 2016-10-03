namespace SunCalcDotNet
{
    using System;
    using System.Collections.Generic;

    public class SunTimes
    {
        public DateTimeOffset SolarNoon { get; set; }

        /// <summary>
        /// The point on the celestial sphere directly below an observer.
        /// </summary>
        public DateTimeOffset Nadir { get; set; }

        public Dictionary<string, DateTimeOffset> Times { get; } = new Dictionary<string, DateTimeOffset>();
    }
}