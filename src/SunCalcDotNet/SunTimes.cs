namespace SunCalcDotNet
{
    using System;
    using System.Collections.Generic;

    public class SunTimes
    {
        private const string SolarNoonKey = "solarNoon";

        private const string NadirKey = "nadir";

        public DateTimeOffset SolarNoon
        {
            get
            {
                return this.Times[SolarNoonKey];
            }
            set
            {
                this.Times[SolarNoonKey] = value;
            }
        }

        /// <summary>
        /// The point on the celestial sphere directly below an observer.
        /// </summary>
        public DateTimeOffset Nadir
        {
            get
            {
                return this.Times[NadirKey];
            }
            set
            {
                this.Times[NadirKey] = value;
            }
        }

        public Dictionary<string, DateTimeOffset> Times { get; } = new Dictionary<string, DateTimeOffset>();
    }
}