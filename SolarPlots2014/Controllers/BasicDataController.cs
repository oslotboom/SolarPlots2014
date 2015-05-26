using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SolarPlots.Classes;

namespace SolarPlots.Controllers
{
    public class BasicDataController : ApiController
    {
        //api/basicdata/latitude=29&longitude=95&timezone=central&date=8-march-2014&increment=5&daylightsaving=true

        public SingleDayBasicData Get(Single latitude, Single longitude, string date, string timeZone,  bool daylightsaving)
        {
            DateTime plotDate = DateTime.Parse(date);

            //Create object which will be returned. It includes the title and plot parameters
            SingleDayBasicData singleDayBasicData = new SingleDayBasicData(Convert.ToDecimal(latitude), Convert.ToDecimal(longitude), timeZone, plotDate, daylightsaving);

            SingleDayBasic singleDayBasic = new SingleDayBasic(latitude, longitude, timeZone, plotDate, daylightsaving);

            singleDayBasicData.DailyStatistics = singleDayBasic;

            return singleDayBasicData;
        }



    }
}
