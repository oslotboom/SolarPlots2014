using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace SolarPlots.Classes
{
    //MultipleDayPlot includes methods to generate data over a period of multiple days
    public class MultipleDayPlot : Plot
    {

        public MultipleDayPlot(double latitude, double longitude, string timeZone, bool daylightSaving, DateTime startDay, DateTime endDay)
            : base(latitude, longitude, timeZone, daylightSaving)
        {
            //verify that the start and end days meet certain criteria

            StringBuilder ExceptionMessage = new StringBuilder("MultipleDayPlot object constructor arguments unacceptable: ", 400);
            if (!(startDay.Date < endDay.Date))
            {
                ExceptionMessage.Append("The start date must be before the end date. ");
                throw new Exception("MultipleDayPlot constructor: " + ExceptionMessage);
            }
            if (endDay.Subtract(startDay).Days > 730)
            {
                ExceptionMessage.Append("Two years is the maximum date separation. ");
                throw new Exception("MultipleDayPlot constructor: " + ExceptionMessage);
            }

            //set the class variables _StartDay and _EndDay to represent the day only (at 12 AM)
            _StartDay = startDay.Date;
            _EndDay = endDay.Date;

        }

        //Declare class varables
        protected DateTime _StartDay;
        protected DateTime _EndDay;


        //GetSeries
        public List<DatePlotData> GetSeries()
        {
            DateTime currDay = _StartDay;
            List<DatePlotData> series = new List<DatePlotData>();

            while (currDay <= _EndDay)
            {

                this.CurrentDay = currDay;
                SetSunriseAndSunsetLocalTime();
                ComputeDueEastDueWest();

                DatePlotData datePlotData = new DatePlotData();
                datePlotData.Date = _currentDay;
                datePlotData.Sunrise = new DateTime(2014, 1, 1, Sunrise.Hour, Sunrise.Minute, Sunrise.Second); //.ToString("M/d/yyyy h:mm:ss tt"); //Convert.ToDecimal((Sunrise.ToOADate() - Math.Floor(Sunrise.ToOADate()))*24); //new TimeSpan(this.Sunrise.Hour, this.Sunrise.Minute, this.Sunrise.Second);
                datePlotData.SunriseAzimuth = GetSunriseAzimuth();
                datePlotData.Sunset = new DateTime(2014, 1, 1, Sunset.Hour, Sunset.Minute, Sunset.Second);
                datePlotData.SunsetAzimuth = GetSunsetAzimuth();
                datePlotData.SolarNoon = new DateTime(2014, 1, 1, SolarNoon.Hour, SolarNoon.Minute, SolarNoon.Second);
                datePlotData.SolarNoonAltitude = Math.Round(GetSolarNoonAltitude(),2);
                datePlotData.DaylightHours = Math.Round(Convert.ToDecimal((Sunset - Sunrise).TotalHours),2);
                datePlotData.DateInformation = String.Empty;


                series.Add(datePlotData);
                currDay = currDay.AddDays(1);
            }

            return series;
        }






    }  // end MultipleDayPlot
}