using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SolarPlots.Classes;

namespace SolarPlots2014.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MobileTest()
        {
            return View();
        }

        public ActionResult SingleDayTabularData(Single latitude, Single longitude, string date, string timeZone, int increment, bool daylightsaving)
        {
            DateTime plotDate = DateTime.Parse(date);
            SingleDayPlot singleDayPlot = new SingleDayPlot(latitude, longitude, timeZone, plotDate, daylightsaving);
            return PartialView(singleDayPlot.GetSeries(increment));
        }

        public ActionResult MultipleDayTabularData(Single latitude, Single longitude, string timeZone, bool daylightsaving, string startDateString, string endDateString)
        {
            DateTime startDate = DateTime.Parse(startDateString);
            DateTime endDate = DateTime.Parse(endDateString);
            MultipleDayPlot multipleDayPlot = new MultipleDayPlot(latitude, longitude, timeZone, daylightsaving, startDate, endDate);
            return PartialView(multipleDayPlot.GetSeries());
        }
    }
}
