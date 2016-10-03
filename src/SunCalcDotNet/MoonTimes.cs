namespace SunCalcDotNet
{
    using System;

    public class MoonTimes
    {
        public MoonTimes(DateTimeOffset? rise, DateTimeOffset? set, double ye)
        {
            this.Rise = rise;
            this.Set = set;

            if (!rise.HasValue && !set.HasValue)
            {
                if (ye > 0)
                {
                    this.AlwaysUp = true;
                }
                else
                {
                    this.AlwaysDown = true;
                }
            }
        }

        public bool AlwaysUp { get; private set; }

        public bool AlwaysDown { get; private set; }

        public DateTimeOffset? Rise { get; private set; }

        public DateTimeOffset? Set { get; private set; }
    }
}