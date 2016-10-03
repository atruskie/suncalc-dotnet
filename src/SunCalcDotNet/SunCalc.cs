// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SunCalc.cs">
//   Copyright (c) 2014, Vladimir Agafonkin
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//    1. Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
//    2. Redistributions in binary form must reproduce the above copyright notice, this list
//       of conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <summary>
//   Defines the SunCalc class a port of https://github.com/mourner/suncalc/blob/master/suncalc.js
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SunCalcDotNet
{
    // shotcuts for easier to read formulas
    using System;
    using System.Linq;

    using static System.Math;

    using System.Threading.Tasks;

    /// <summary>
    /// Calculate sun positions.
    /// <para>
    /// sun calculations are based on http://aa.quae.nl/en/reken/zonpositie.html formulas
    /// </para>
    /// </summary>
    public static class SunCalc
    {
        private const double Rad = PI / 180.0;

        // date/time constants and conversions

        #region conversions

        public const int DayMs = 1000 * 60 * 60 * 24;

        public const int J1970 = 2440588;

        public const int J2000 = 2451545;

        private static double ToJulian(DateTimeOffset date)
        {
            return (double)date.ToUnixTimeMilliseconds() / DayMs - 0.5 + J1970;
        }

        private static DateTimeOffset FromJulian(double j)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)((j + 0.5 - J1970) * DayMs));
        }

        private static double ToDays(DateTimeOffset date)
        {
            return ToJulian(date) - J2000;
        }

        private static DateTimeOffset HoursLater(DateTimeOffset date, double hours)
        {
            return date.AddHours(hours);
        }


        #endregion

        // general calculations for positio

        #region positions

        /// obliquity of the Earth
        public const double e = Rad * 23.4397;

        private static double RightAscension(double l, double b)
        {
            return Atan2(Sin(l) * Cos(e) - Tan(b) * Sin(e), Cos(l));
        }

        private static double Declination(double l, double b)
        {
            return Asin(Sin(b) * Cos(e) + Cos(b) * Sin(e) * Sin(l));
        }

        private static double Azimuth(double H, double phi, double dec)
        {
            return Atan2(Sin(H), Cos(H) * Sin(phi) - Tan(dec) * Cos(phi));
        }

        private static double Altitude(double H, double phi, double dec)
        {
            return Asin(Sin(phi) * Sin(dec) + Cos(phi) * Cos(dec) * Cos(H));
        }

        private static double SiderealTime(double d, double lw)
        {
            return Rad * (280.16 + 360.9856235 * d) - lw;
        }

        private static double AstroRefraction(double h)
        {
            // the following formula works for positive altitudes only
            if (h < 0)
            {
                // if h = -0.08901179 a div/0 would occur.
                h = 0;
            }

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Tan(h + 0.00312536 / (h + 0.08901179));
        }

        #endregion

        #region generalSunCalculations

        private static double SolarMeanAnomaly(double d)
        {
            return Rad * (357.5291 + 0.98560028 * d);
        }

        private static double EclipticLongitude(double M)
        {
            // equation of center
            var C = Rad * (1.9148 * Sin(M) + 0.02 * Sin(2 * M) + 0.0003 * Sin(3 * M));

            // perihelion of the Earth
            var P = Rad * 102.9372;

            return M + C + P + PI;
        }

        private static void SunCoordinates(double d, out double declination, out double rightAscension)
        {
            var M = SolarMeanAnomaly(d);
            var L = EclipticLongitude(M);

            declination = Declination(L, 0);
            rightAscension = RightAscension(L, 0);
        }

        #endregion

        public static object GetPosition(DateTimeOffset date, double latitude, double longitude)
        {
            var lw = Rad * -longitude;
            var phi = Rad * latitude;
            var d = ToDays(date);

            double declination;
            double rightAscension;
            SunCoordinates(d, out declination, out rightAscension);

            var H = SiderealTime(d, lw) - rightAscension;

            return new Position { Azimuth = Azimuth(H, phi, declination), Altitude = Altitude(H, phi, declination) };
        }

        // sun times configuration (angle, morning name, evening name)
        private static TimeMap[] DefaultTimes =
            {
                new TimeMap(-0.833, "sunrise", "sunset"),
                new TimeMap(-0.3, "sunriseEnd", "sunsetStart"),
                new TimeMap(-6, "dawn", "dusk"),
                new TimeMap(-12, "nauticalDawn", "nauticalDusk"),
                new TimeMap(-18, "nightEnd", "night"),
                new TimeMap(6, "goldenHourEnd", "goldenHour")
            };

        public static TimeMap[] Times { get; private set; } = DefaultTimes;

        /// <summary>
        /// Adds a custom time to the times config
        /// </summary>
        /// <param name="map"></param>
        public static void AddTime(TimeMap map)
        {
            // implementer's note: this is a bit dodgy - we might change the structure of the time map later
            Times = Times.Concat(new[] { map }).ToArray();
        }

        #region calculationsForSunTimes

        private static double J0 = 0.0009;

        private static double JulianCycle(double d, double lw)
        {
            return Math.Round(d - J0 - lw / (2 * PI), MidpointRounding.ToEven);
        }

        private static double ApproxTransit(double Ht, double lw, double n)
        {
            return J0 + (Ht + lw) / (2 * PI) + n;
        }

        private static double SolarTransitJ(double ds, double M, double L)
        {
            return J2000 + ds + 0.0053 * Sin(M) - 0.0069 * Sin(2 * L);
        }

        private static double HourAngle(double h, double phi, double d)
        {
            return Acos((Sin(h) - Sin(phi) * Sin(d)) / (Cos(phi) * Cos(d)));
        }

        /// returns set time for the given sun altitude
        private static double GetSetJ(double h, double lw, double phi, double dec, double n, double M, double L)
        {
            double w = HourAngle(h, phi, dec), a = ApproxTransit(w, lw, n);

            return SolarTransitJ(a, M, L);
        }



        // calculates sun times for a given date and latitude/longitude

        public static object GetTimes(DateTimeOffset date, double lat, double lng)
        {

            double lw = Rad * -lng,
                   phi = Rad * lat,
                   d = ToDays(date),
                   n = JulianCycle(d, lw),
                   ds = ApproxTransit(0, lw, n),
                   M = SolarMeanAnomaly(ds),
                   L = EclipticLongitude(M),
                   dec = Declination(L, 0),
                   Jnoon = SolarTransitJ(ds, M, L),
                   Jset,
                   Jrise;

            var result = new SunTimes { SolarNoon = FromJulian(Jnoon), Nadir = FromJulian(Jnoon - 0.5) };

            for (int i = 0; i < Times.Length; i += 1)
            {
                var time = Times[i];

                Jset = GetSetJ(time.Angle * Rad, lw, phi, dec, n, M, L);
                Jrise = Jnoon - (Jset - Jnoon);

                result.Times[time.MorningName] = FromJulian(Jrise);
                result.Times[time.EveningName] = FromJulian(Jset);
            }

            return result;
        }

        #endregion

        #region moonCalculations

        /// moon calculations, based on http://aa.quae.nl/en/reken/hemelpositie.html formulas
        /// <param name="d">geocentric ecliptic coordinates of the moon</param>
        private static void MoonCoordinates(
            double d,
            out double rightAscension,
            out double declination,
            out double distance)
        {
            // ecliptic longitude
            double L = Rad * (218.316 + 13.176396 * d);
            // mean anomaly
            double M = Rad * (134.963 + 13.064993 * d);
            // mean distance
            double F = Rad * (93.272 + 13.229350 * d);

            // longitude
            double l = L + Rad * 6.289 * Sin(M);
            // latitude
            double b = Rad * 5.128 * Sin(F);
            // distance to the moon in km
            double dt = 385001 - 20905 * Cos(M);

            rightAscension = RightAscension(l, b);
            declination = Declination(l, b);
            distance = dt;
        }

        public static MoonPosition GetMoonPosition(DateTimeOffset date, double lat, double lng)
        {
            double lw = Rad * -lng;
            double phi = Rad * lat;
            double d = ToDays(date);

            double rightAscension;
            double declination;
            double distance;
            MoonCoordinates(d, out rightAscension, out declination, out distance);

            double H = SiderealTime(d, lw) - rightAscension;
            double h = Altitude(H, phi, declination);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            double pa = Atan2(Sin(H), Tan(phi) * Cos(declination) - Sin(declination) * Cos(H));

            // altitude correction for refraction
            h = h + AstroRefraction(h);

            return new MoonPosition
                       {
                           Azimuth = Azimuth(H, phi, declination),
                           Altitude = h,
                           Distance = distance,
                           ParallacticAngle = pa
                       };
        }

        /// calculations for illumination parameters of the moon,
        /// based on http://idlastro.gsfc.nasa.gov/ftp/pro/astro/mphase.pro formulas and
        /// Chapter 48 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
        public static object GetMoonIllumination(DateTimeOffset date)
        {

            var d = ToDays(date);

            double sunRightAscension;
            double sunDeclination;
            SunCoordinates(d, out sunDeclination, out sunRightAscension);

            double moonRightAscension;
            double moonDeclination;
            double moonDistance;
            MoonCoordinates(d, out moonRightAscension, out moonDeclination, out moonDistance);

            // distance from Earth to Sun in km
            var sdist = 149598000;

            var phi =
                Acos(
                    Sin(sunDeclination) * Sin(sunDeclination)
                    + Cos(sunDeclination) * Cos(moonDeclination) * Cos(sunRightAscension - moonRightAscension));

            var inc = Atan2(sdist * Sin(phi), moonDistance - sdist * Cos(phi));


            var angle = Atan2(
                Cos(sunDeclination) * Sin(sunRightAscension - moonRightAscension),
                Sin(sunDeclination) * Cos(moonDeclination)
                - Cos(sunDeclination) * Sin(moonDeclination) * Cos(sunRightAscension - moonRightAscension));

            return new MoonIllumination
                       {
                           Fraction = (1 + Cos(inc)) / 2,
                           Phase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / PI,
                           Angle = angle
                       };
        }

        /// calculations for moon rise/set times are based on http://www.stargazing.net/kepler/moonrise.html article
        public static object GetMoonTimes(DateTimeOffset date, double lat, double lng, bool inUTC)
        {
            // implementer's note: the inUTC option seems redundant - consider removing

            DateTimeOffset t = date;
            if (inUTC)
            {
                t = t.UtcDateTime;
            }

            t = t - t.TimeOfDay;


            var hc = 0.133 * Rad;
            var h0 = GetMoonPosition(t, lat, lng).Altitude - hc;
            double h1, h2, a, b, xe, ye = 0, d, x1 = 0, x2 = 0, dx;
            double? rise = null, set = null;

            // go in 2-hour chunks, each time seeing if a 3-point quadratic curve crosses zero (which means rise or set)
            for (var i = 1; i <= 24; i += 2)
            {
                h1 = GetMoonPosition(HoursLater(t, i), lat, lng).Altitude - hc;
                h2 = GetMoonPosition(HoursLater(t, i + 1), lat, lng).Altitude - hc;

                a = (h0 + h2) / 2 - h1;
                b = (h2 - h0) / 2;
                xe = -b / (2 * a);
                ye = (a * xe + b) * xe + h1;
                d = b * b - 4 * a * h1;
                int roots = 0;

                if (d >= 0)
                {
                    dx = Sqrt(d) / (Abs(a) * 2);
                    x1 = xe - dx;
                    x2 = xe + dx;
                    if (Abs(x1) <= 1)
                    {
                        roots++;
                    }
                    if (Abs(x2) <= 1)
                    {
                        roots++;
                    }
                    if (x1 < -1)
                    {
                        x1 = x2;
                    }
                }

                if (roots == 1)
                {
                    if (h0 < 0)
                    {
                        rise = i + x1;
                    }
                    else
                    {
                        set = i + x1;
                    }

                }
                else if (roots == 2)
                {
                    rise = i + (ye < 0 ? x2 : x1);
                    set = i + (ye < 0 ? x1 : x2);
                }

                if (rise != null && set != null)
                {
                    break;
                }

                h0 = h2;
            }

            var result = new MoonTimes(
                rise.HasValue ? HoursLater(t, rise.Value) : (DateTimeOffset?)null,
                set.HasValue ? HoursLater(t, set.Value) : (DateTimeOffset?)null,
                ye);

            return result;
        }

        #endregion
    }
}