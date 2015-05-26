using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SolarPlots.Classes
{
    public class SingleDayBasic
    {
        public SingleDayBasic(double latitude, double longitude, string timeZone, DateTime currentDay, bool daylightSaving)
        {

            Plot plot = new Plot(latitude, longitude, timeZone, daylightSaving);

            plot.CurrentDay = currentDay;
            plot.SetSunriseAndSunsetLocalTime();
            plot.ComputeDueEastDueWest();

            Sunrise = plot.Sunrise.ToShortTimeString();
            Sunset = plot.Sunset.ToShortTimeString();
            SolarNoon = plot.SolarNoon.ToShortTimeString();
            SolarNoonAltitude = Math.Round(plot.GetSolarNoonAltitude(),2).ToString();
            SunriseAzimuth = plot.GetSunriseAzimuth();
            SunsetAzimuth = plot.GetSunsetAzimuth();
            LengthOfDayHours = Math.Round(Convert.ToDecimal(plot.DayLengthHours),2);
            AzimuthDueEast = plot.DueEast.HasValue ? plot.DueEast.Value.ToShortTimeString() : "n/a";
            AzimuthDueWest = plot.DueWest.HasValue ? plot.DueWest.Value.ToShortTimeString() : "n/a";

        }

        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string SolarNoon { get; set; }
        public string SolarNoonAltitude { get; set; }
        public decimal SunriseAzimuth { get; set; }
        public decimal SunsetAzimuth { get; set; }
        public decimal LengthOfDayHours { get; set; }
        public string AzimuthDueWest { get; set; }
        public string AzimuthDueEast { get; set; }

    }
}