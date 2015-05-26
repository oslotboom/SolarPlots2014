using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SolarPlots.Classes
{
    public class SingleDayPlotData
    {
        public SingleDayPlotData(Decimal latitude, Decimal longitude, string timezone, DateTime plotDate, int incrementMinutes, bool daylightsaving)
        {
            PlotParameters = new SingleDayPlotParameters(latitude, longitude, timezone, plotDate, incrementMinutes, daylightsaving);
        }

        public string Title { get; set; }
        public SingleDayPlotParameters PlotParameters;
        public List<DateTimePlotData> PlotData { get; set; }

    }

    public class SingleDayPlotParameters
    {
        public SingleDayPlotParameters(Decimal latitude, Decimal longitude, string timezone, DateTime plotDate, int incrementMinutes, bool daylightsaving)
        {
            Latitude = latitude;
            Longitude = longitude;
            Timezone = timezone;
            PlotDate = plotDate;
            IncrementMinutes = incrementMinutes;
            Daylightsaving = daylightsaving;
        }

        public Decimal Latitude { get; set; }
        public Decimal Longitude { get; set; }
        public string Timezone { get; set; }
        public DateTime PlotDate { get; set; }
        public int IncrementMinutes { get; set; }
        public bool Daylightsaving { get; set; }

    }

    public class DateTimePlotData
    {
        public DateTimePlotData(DateTime pointDateTime, Decimal altitude, Decimal azimuth, string altitudePointInformation, string azimuthPointInformation) 
        {
            PointDateTime = pointDateTime;
            Altitude = altitude;
            Azimuth = azimuth;
            AltitudePointInformation = altitudePointInformation;
            AzimuthPointInformation = azimuthPointInformation;        
        }

        public DateTime PointDateTime { get; set; }
        public Decimal Altitude { get; set; }
        public Decimal Azimuth { get; set; }
        public string AltitudePointInformation { get; set; }
        public string AzimuthPointInformation { get; set; }

    }

    public class DatePlotData
    {
        public DatePlotData()  {}

        public DateTime Date { get; set; }
        public DateTime Sunrise { get; set;}
        public DateTime Sunset { get; set; }
        public Decimal SunriseAzimuth { get; set; }
        public Decimal SunsetAzimuth { get; set; }
        public DateTime SolarNoon { get; set; }
        public Decimal SolarNoonAltitude { get; set; }
        public Decimal DaylightHours { get; set; }
        public string DateInformation { get; set; }

    }

    public class MultipleDayPlotData
    {
        public MultipleDayPlotData(Decimal latitude, Decimal longitude, string timezone, bool daylightsaving, DateTime startDate, DateTime endDate)
        {
            PlotParameters = new MultipleDayPlotParameters(latitude, longitude, timezone, daylightsaving, startDate, endDate);
        }

        public string Title { get; set; }
        public MultipleDayPlotParameters PlotParameters;
        public List<DatePlotData> PlotData { get; set; }

    }

    public class MultipleDayPlotParameters
    {
        public MultipleDayPlotParameters(Decimal latitude, Decimal longitude, string timezone, bool daylightsaving, DateTime startDate, DateTime endDate)
        {
            Latitude = latitude;
            Longitude = longitude;
            Timezone = timezone;
            Daylightsaving = daylightsaving;
            StartDate = startDate;
            EndDate = endDate;
        }

        public Decimal Latitude { get; set; }
        public Decimal Longitude { get; set; }
        public string Timezone { get; set; }
        public bool Daylightsaving { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SingleDayBasicData
    {
        public SingleDayBasicData(Decimal latitude, Decimal longitude, string timezone, DateTime plotDate, bool daylightsaving)
        {
            Parameters = new SingleDayPlotParameters(latitude, longitude, timezone, plotDate, 0, daylightsaving);
        }

        public SingleDayPlotParameters Parameters;
        public SingleDayBasic DailyStatistics; 
    }

}