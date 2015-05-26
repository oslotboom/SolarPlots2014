using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SolarPlots.Classes;

namespace SolarPlots2014.Controllers
{
    public class SeriesController : ApiController
    {
        // GET api/series
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/series/5
        public string Get(int id)
        {
            return "value";
        }

        // GET api/series/
        //routeTemplate: "api/{controller}/latitude={latitude}&longitude={longitude}&timezone={timezone}&date={date}&increment={increment}&daylightsaving={daylightsaving}"
        //api/series/latitude=29&longitude=95&timezone=central&date=8-march-2014&increment=5&daylightsaving=true
        //was public IEnumerable<DateTimePlotData>
        public SingleDayPlotData  GetSingleDay(Single latitude, Single longitude, string date, string timeZone, int increment, bool daylightsaving)
        {
            DateTime plotDate = DateTime.Parse(date);

            //Create object which will be returned. It includes the title and plot parameters
            SingleDayPlotData plotData = new SingleDayPlotData(Convert.ToDecimal(latitude), Convert.ToDecimal(longitude), timeZone, plotDate, increment, daylightsaving);
            plotData.Title = plotDate.ToString("D") + ", Lat=" + latitude.ToString("F2") + ", Long=" + longitude.ToString("F2") + ", Timezone=" + timeZone + (!daylightsaving ? ", Daylight Saving NOT applied" : "");

            SingleDayPlot singleDayPlot = new SingleDayPlot(latitude, longitude, timeZone, plotDate, daylightsaving);
            plotData.PlotData = singleDayPlot.GetSeries(increment);

            return plotData;
        }

        // GET api/series/
        //routeTemplate: "api/{controller}/latitude={latitude}&longitude={longitude}&timezone={timezone}&daylightsaving={daylightsaving}&startdate={date}&enddate={date}"
        //api/series/latitude=29&longitude=95&timezone=central&daylightsaving=true&startdate=1-january-2014&enddate=31-december-2014
        //was public IEnumerable<DateTimePlotData>
        public MultipleDayPlotData GetMultipleDays(Single latitude, Single longitude, string timeZone, bool daylightsaving, string startDateString, string endDateString)
        {
            DateTime startDate = DateTime.Parse(startDateString);
            DateTime endDate = DateTime.Parse(endDateString);

            //Create object which will be returned. It includes the title and plot parameters
            MultipleDayPlotData plotData = new MultipleDayPlotData(Convert.ToDecimal(latitude), Convert.ToDecimal(longitude), timeZone, daylightsaving, startDate, endDate);
            plotData.Title = startDate.ToString("d-MMM-yyyy") + " to " + endDate.ToString("d-MMM-yyyy") + ", Lat=" + latitude.ToString("F2") + ", Long=" + longitude.ToString("F2") + ", Timezone=" + timeZone + (!daylightsaving ? ", Daylight Saving NOT applied" : "");

            MultipleDayPlot multipleDayPlot = new MultipleDayPlot(latitude, longitude, timeZone, daylightsaving, startDate, endDate);
            plotData.PlotData = multipleDayPlot.GetSeries();

            return plotData;
        }


        // POST api/series
        public void Post([FromBody]string value)
        {
        }

        // PUT api/series/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/series/5
        public void Delete(int id)
        {
        }
    }
}
