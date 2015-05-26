using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SolarPlots.Classes
{
    public class SingleDayPlot : Plot
    {
        public SingleDayPlot(double latitude, double longitude, string timeZone, DateTime currentDay, bool daylightSaving)
            : base(latitude, longitude, timeZone, daylightSaving)
        {

            this.CurrentDay = currentDay;
            SetSunriseAndSunsetLocalTime();
            ComputeDueEastDueWest();
        }

        //GetSeries with no arguments goes from sunrise to sunset at 1 minute increments
        public List<DateTimePlotData> GetSeries()
        {
            //determine the sunrise and sunset
            return GetSeries(Sunrise, Sunset, 1);
        }


        //for this overload data is returned at 1 minute increments
        public List<DateTimePlotData> GetSeries(DateTime startLocalTime, DateTime endLocalTime)
        {
            return GetSeries(startLocalTime, endLocalTime, 1);
        }

        //SingleDayPlot
        //This overload takes the interval as an argument and uses the sunrise and sunset for start and end times
        public List<DateTimePlotData> GetSeries(int Increment)
        {
            return GetSeries(Sunrise, Sunset, Increment);

        }


        //SingleDayPlot
        //These methods go from sunrise to the time specified by the input parameter
        public List<DateTimePlotData> GetSeriesFromSunrise(DateTime endLocalTime)
        {
            return GetSeries(Sunrise, endLocalTime);
        }
        public List<DateTimePlotData> GetSeriesFromSunrise(DateTime endLocalTime, int increment)
        {
            return GetSeries(Sunrise, endLocalTime, increment);
        }

        //These methods go from the time specified by the input parameter to Sunset
        public List<DateTimePlotData> GetSeriesToSunset(DateTime startLocalTime)
        {
            return GetSeries(startLocalTime, Sunset);
        }
        public List<DateTimePlotData> GetSeriesToSunset(DateTime startLocalTime, int increment)
        {
            return GetSeries(startLocalTime, Sunset, increment);
        }


        //SingleDayPlot
        //increment is in minutes
        public List<DateTimePlotData> GetSeries(DateTime startLocalTime, DateTime endLocalTime, int increment)
        {
            List<DateTimePlotData> series = new List<DateTimePlotData>();

            if (startLocalTime < Sunrise)
                throw new Exception("GetSeries: requested start time for series is before sunrise (" + Sunrise.ToLongTimeString() + ")");
            else if (endLocalTime > Sunset)
                throw new Exception("GetSeries: requested end time for series is after sunset (" + Sunset.ToLongTimeString() + ")");
            else if (startLocalTime > endLocalTime)
                throw new Exception("GetSeries: requested end time must be after start time.");
            else if ((startLocalTime.Year != endLocalTime.Year) || (startLocalTime.DayOfYear != endLocalTime.DayOfYear))
                throw new Exception("GetSeries: start and end times must be on the same day");
            else
            {
                //generate the List of series points

                DateTime currentTime = startLocalTime;
                DateTime? currentTimePrevious = null;
                bool solarNoonFound = false;
                bool dueEastFound = false;
                bool dueWestFound = false;
                DateTime timeAtSolarNoon = TimeForPercentOfSolarNoonLST(100);


                decimal Altitude = 0;
                decimal Azimuth = 0;

                GetAngles(currentTime, ref Altitude, ref Azimuth);
                series.Add(new DateTimePlotData(currentTime, Altitude, Azimuth, AltitudePointMessageText(currentTime, Altitude), AzimuthPointMessageText(currentTime, Azimuth))); 

                //dr = dtAngles.NewRow();
                //dr["Date-Time"] = currentTime.ToShortDateString() + " " + currentTime.ToLongTimeString();
                //dr["Date-Time(hours)"] = currentTime.ToShortDateString() + " " + Math.Round(ConvertToHours(currentTime), 3).ToString();
                //dr["Percent of Solar Noon"] = PercentOfSolarNoonForTime(currentTime).ToString();
                //dr["Altitude"] = Math.Round(Altitude, 2).ToString();
                //dr["Azimuth"] = Math.Round(Azimuth, 2).ToString();
                //dtAngles.Rows.Add(dr);

                //now round the currentTime down to the previous minute, and then add the increment
                //This ensures we are on even one-minute intervals if the starting time is not an even minute

                currentTime = new DateTime(currentTime.Year, currentTime.Month,
                    currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                //add increment
                currentTime = currentTime.AddMinutes(increment);

                if (currentTime > endLocalTime)
                    currentTime = endLocalTime;

                while (currentTime <= endLocalTime)
                {
                    GetAngles(currentTime, ref Altitude, ref Azimuth);
                    series.Add(new DateTimePlotData(currentTime, Altitude, Azimuth, AltitudePointMessageText(currentTime, Altitude), AzimuthPointMessageText(currentTime, Azimuth))); 

                    
                    //dr = dtAngles.NewRow();
                    //dr["Date-Time"] = currentTime.ToShortDateString() + " " + currentTime.ToLongTimeString();
                    //dr["Date-Time(hours)"] = currentTime.ToShortDateString() + " " + Math.Round(ConvertToHours(currentTime), 3).ToString();
                    //dr["Percent of Solar Noon"] = PercentOfSolarNoonForTime(currentTime).ToString();
                    //dr["Altitude"] = Math.Round(Altitude, 2).ToString();
                    //dr["Azimuth"] = Math.Round(Azimuth, 2).ToString();
                    //dtAngles.Rows.Add(dr);

                    if (currentTime == endLocalTime)
                        break;

                    //If previous time was solar noon, add the increment to the time that was prior to solar noon to maintain the same increment sequence
                    if (currentTimePrevious.HasValue) { 
                        currentTime = currentTimePrevious.Value.AddMinutes(increment);
                        currentTimePrevious = null;
                    }
                    else
                        currentTime = currentTime.AddMinutes(increment);

                    //Check if the next increment passes through solar noon, due west or due east
                    if (currentTime > endLocalTime)
                        currentTime = endLocalTime;
                    else if (!solarNoonFound && startLocalTime < SolarNoon && currentTime > SolarNoon) {
                        currentTimePrevious = currentTime;
                        currentTime = SolarNoon;
                        solarNoonFound = true;
                    }
                    else if (DueEast.HasValue && !dueEastFound && startLocalTime < DueEast && currentTime > DueEast)
                    {
                        currentTimePrevious = currentTime;
                        currentTime = DueEast.Value;
                        dueEastFound = true;
                    }
                    else if (DueWest.HasValue && !dueWestFound && startLocalTime < DueWest && currentTime > DueWest)
                    {
                        currentTimePrevious = currentTime;
                        currentTime = DueWest.Value;
                        dueWestFound = true;
                    }

                }

                return series;
            }

        }




    } // end SingleDayPlot class
}