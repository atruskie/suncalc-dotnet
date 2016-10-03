namespace SunCalcDotNet
{
    public class TimeMap
    {
        public TimeMap(double angle, string morningName, string eveningName)
        {
            this.Angle = angle;
            this.MorningName = morningName;
            this.EveningName = eveningName;
        }

        public double Angle { get; set; }

        public string MorningName { get; set; }

        public string EveningName { get; set; }
    }
}