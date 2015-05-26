
var singleDayPlotData = [];

/************************** Event functions ***************************/
function getClick(event) {
    var clickedElement = event.target;
    //alert(clickedElement);
    //console.log(clickedElement);
    var latLong = clickedElement.id.split("~");
    var timezone = clickedElement.parentNode.id;
    var city = latLong[0];
    if (!(city.indexOf(",") > -1))
        city = "";

    ViewModel.Latitude(latLong[1]).Longitude(latLong[2]).City(city).SelectedTimezoneOption(timezone);

    //pt.x = event.clientX; pt.y = event.clientY;
    //alert("x: " + event.clientX + "  y:" + event.clientY)
    return;
}

function addClick() {
    var embedObj = document.getElementById("svgUSA");
    var pt = embedObj.getSVGDocument().createSVGPoint();
    embedObj.getSVGDocument().onclick = function (event) {
        return getClick(event);
    };
    return;
}

/************************** END Event functions ***************************/

/************************** Set up Knockout ***************************/
//Data model
function AppViewModel() {
    var self = this;
    self.City = ko.observable("North Dallas, TX");
    self.Latitude = ko.observable('32.96').extend({ required: { message: 'Latitude is required' }, number: { message: 'Latitude must be a numeric value' } });
    self.Longitude = ko.observable('96.84').extend({ required: { message: 'Longitude is required' }, number: { message: 'Longitude must be a numeric value' } });;
    self.TimezoneOptions = ["Eastern", "Central", "Mountain", "Pacific"]
    self.SelectedTimezoneOption = ko.observable("Central").extend({ required: { message: 'A time zone must be selected' } });
    self.SingleDayDate = ko.observable('Today').extend({
        validation: {
            validator: function (val) {
                //if (Object.prototype.toString.call(d) !== "[object Date]")
                //    return false;
                //return !isNaN(d.getTime());
                //var isDate = (Object.prototype.toString.call(val) !== "[object Date]") && !isNaN(val.getTime());

                var parseResult = Date.parse(val);
                return (val.toLowerCase() == "today" || !isNaN(parseResult)); // && isNaN(parseResult)

            },
            message: 'Single day date must be a valid date or "Today"'
        }
    });
    self.SingleDayFrom = ko.observable('Sunrise');
    self.SingleDayTo = ko.observable('Sunset');
    self.SingleDayTabularTimeIncrement = ko.observable('5').extend({ required: { message: 'Time increment is required for tabular data' }, number: { message: 'Tabular data time increment must be a numeric value' } });;
    self.SingleDayNoDSTCheckbox = ko.observable(false),
    self.RadioSelectedOptionValue = ko.observable('AltitudeAzimuth'),
    self.MultipleTimespanSelectedOptionValue = ko.observable('MultipleYear');
    self.IsMultipleDateValidationRequired = ko.computed(function () {
        // Knockout tracks dependencies automatically. It knows that fullName depends on firstName and lastName, because these get called when evaluating fullName.
        return (self.MultipleTimespanSelectedOptionValue == "MultipleSet");
    });
    self.MultipleDayFrom = ko.observable().extend({
        required: {
            onlyIf: function () { return (self.MultipleTimespanSelectedOptionValue() == "MultipleSet") },
            message: "Start Date for multiple day plot must be a valid date"
        },
        date: {
            message: "Start Date for multiple day plot must be a valid date",
            onlyIf: function () { return (self.MultipleTimespanSelectedOptionValue() == "MultipleSet") }
        }
    });
    self.MultipleDayTo = ko.observable().extend({
        validation: {
            validator: function (val, someOtherVal) {
                if (Date.parse(self.MultipleDayFrom()) && Date.parse(self.MultipleDayTo())) {
                    var sDate = new Date(self.MultipleDayFrom());
                    var eDate = new Date(self.MultipleDayTo());
                    var diff = eDate - sDate;
                    return diff > 24 * 60 * 60 * 1000;
                }
                else
                    return true;
            },
            message: 'End date must be after start date'
        },
        date: {
            message: 'End Date for multiple day plot must be a valid date',
            onlyIf: function () { return (self.MultipleTimespanSelectedOptionValue() == "MultipleSet") }
        },
        required: {
            onlyIf: function () { return (self.MultipleTimespanSelectedOptionValue() == "MultipleSet") },
            message: 'End Date for multiple day plot must be a valid date and after the start'
        }

    });
    self.MultiplePlot = ko.observable('SunsetTime');
    self.ErrorListHeading = ko.observable();



    self.fullName = ko.computed(function () {
        // Knockout tracks dependencies automatically. It knows that fullName depends on firstName and lastName, because these get called when evaluating fullName.
        return self.SingleDayFrom() + " " + self.SingleDayTo();

    });


    self.SingleDayPlotData = ko.observableArray(singleDayPlotData);

    //self.bindSingleDayData = function () {
    //    self.SingleDayPlotData([]);
    //    self.SingleDayPlotData(GetSolarDataJson());
    //};

    self.plotshowClick = function () {
        var chart = $('#plotContainer').highcharts();
        if (chart) {
            var seriesAltitude = chart.series[0];
            var seriesAzimuth = chart.series[1];
            if (ViewModel.RadioSelectedOptionValue() == "Altitude") {
                seriesAzimuth.hide();
                if (!seriesAltitude.visible)
                    seriesAltitude.show();
            }
                //chartOptions.series[1].visible = false;
            else if (ViewModel.RadioSelectedOptionValue() == "Azimuth") {
                seriesAltitude.hide();
                if (!seriesAzimuth.visible)
                    seriesAzimuth.show();
            }

            else if (ViewModel.RadioSelectedOptionValue() == "AltitudeAzimuth") {
                seriesAltitude.show();
                seriesAzimuth.show();
            }
        }

        return true;
    };

    self.multiplePlotClick = function () {
        var chart = $('#plotContainer').highcharts();
        if (chart) {
            if (chart.series.length > 2) {
                for (var i = 0; i < chart.series.length; i++)
                    chart.series[i].hide();
                switch (ViewModel.MultiplePlot()) {
                    case "SunriseTime":
                        chart.series[0].show();
                        chart.yAxis[0].update({ type: 'datetime', title: { text: 'Sunrise  Time' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                        };
                        break;
                    case "SunriseAzimuth":
                        chart.series[1].show();
                        //chart.yAxis[0].axisTitle.attr({
                        //    text: 'Sunrise Azimuth'
                        //});
                        //chart.yAxis[0].axisTitle.attr({
                        //    text: 'Sunrise Time'
                        //});
                        chart.yAxis[0].update({ type: 'linear', title: { text: 'Sunrise  Azimuth  Angle' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                        };
                        break;
                    case "SunsetTime":
                        chart.series[2].show();
                        chart.yAxis[0].update({ type: 'datetime', title: { text: 'Sunset  Time' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                        };
                        break;
                    case "SunsetAzimuth":
                        chart.series[3].show();
                        chart.yAxis[0].update({ type: 'linear', title: { text: 'Sunset  Azimuth  Angle' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                        };
                        break;
                    case "SolarNoonTime":
                        chart.series[4].show();
                        //chart.yAxis[0].setTitle({
                        //    text: "Solar Noon Time"
                        //});
                        chart.yAxis[0].update({ type: 'datetime', title: { text: 'Solar  Noon  Time' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                        };
                        break;
                    case "SolarNoonAltitude":
                        chart.series[5].show();
                        chart.yAxis[0].update({ type: 'linear', title: { text: 'Solar  Noon  Altitude' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                        };
                        break;
                    case "LengthOfDay":
                        chart.series[6].show();
                        chart.yAxis[0].update({ type: 'linear', title: { text: 'Length  of  Day,  Hours' } });
                        chart.tooltip.options.formatter = function () {
                            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/> ' + Math.floor(this.y) + ' hours ' + Math.round((this.y - Math.floor(this.y)) * 60) + ' minutes';
                        };
                        break;
                }
            }
        }
        else {
            //The radio was clicked before a chart was created. Update the chartOptionsMultipleDays object 
            switch (ViewModel.MultiplePlot()) {
                case "SunriseTime":
                    chartOptionsMultipleDays.yAxis[0].type = 'datetime';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Sunrise  Time';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                    };
                    break;
                case "SunriseAzimuth":
                    chartOptionsMultipleDays.yAxis[0].type = 'linear';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Sunrise  Azimuth  Angle';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                    };
                    break;
                case "SunsetTime":
                    chartOptionsMultipleDays.yAxis[0].type = 'datetime';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Sunset  Time';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                    };
                    break;
                case "SunsetAzimuth":
                    chartOptionsMultipleDays.yAxis[0].type = 'linear';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Sunset  Azimuth  Angle,  degrees';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                    };
                    break;
                case "SolarNoonTime":
                    chartOptionsMultipleDays.yAxis[0].type = 'datetime';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Solar  Noon  Time';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
                    };
                    break;
                case "SolarNoonAltitude":
                    chartOptionsMultipleDays.yAxis[0].type = 'linear';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Solar  Noon  Altitude,  degrees';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>Angle: ' + this.y;
                    };
                    break;
                case "LengthOfDay":
                    chartOptionsMultipleDays.yAxis[0].type = 'linear';
                    chartOptionsMultipleDays.yAxis[0].title.text = 'Length  of  Day,  Hours';
                    chartOptionsMultipleDays.tooltip.formatter = function () {
                        return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/> ' + Math.floor(this.y) + ' hours ' + Math.round((this.y - Math.floor(this.y)) * 60) + ' minutes';
                    };
                    break;
            }
        }
        return true;
    };

    self.errorList = ko.observableArray([]);

    self.addError = function (item) {
        self.errorList.push({ errorText: item });
    };

    self.clearErrors = function () {
        self.errorList.removeAll();
    }

    self.SelectMultipleSet = function () {
        self.MultipleTimespanSelectedOptionValue('MultipleSet');
    }

};

var ViewModel = new AppViewModel();
/**************************END  Set up Knockout ***************************/

/************************** Set up Knockout validation ***************************/

ViewModel.errorsSingleDay = ko.validation.group([ViewModel.Latitude, ViewModel.Longitude, ViewModel.SelectedTimezoneOption, ViewModel.SingleDayDate]);
ViewModel.errorsSingleDayTabular = ko.validation.group([ViewModel.Latitude, ViewModel.Longitude, ViewModel.SelectedTimezoneOption, ViewModel.SingleDayDate, ViewModel.SingleDayTabularTimeIncrement]);
ViewModel.errorsMultipleDay = ko.validation.group([ViewModel.Latitude, ViewModel.Longitude, ViewModel.SelectedTimezoneOption, ViewModel.MultipleDayFrom, ViewModel.MultipleDayTo]);

ko.validation.init({
    insertMessages: false,
    decorateElement: true,  //new version property is called decorateInputElement
    errorElementClass: 'input-validation-error',
    messageTemplate: 'knockout-validation-errors'
});
/**************************END  Set up Knockout validation ***************************/

//activate Knockout
ko.applyBindings(ViewModel); // This makes Knockout get to work


/************************** Utility functions  ****************************************/
function GetDataServer() {
    var _dataServer = "";
    if (window.location.toString().toLowerCase().indexOf("localhost") > -1)
        _dataServer = "http://" + window.location.host;  //"http://localhost/dfwfreeways";
    else
        _dataServer = "http://" + window.location.host;
    return _dataServer;
}

function GetSingleDayDate() {
    var dateString = ViewModel.SingleDayDate();
    if (dateString.toLowerCase() == "today") {
        var day = new Date();
        dateString = day.getDate() + "-" + MonthName(day.getMonth()) + "-" + day.getFullYear();
    }
    return dateString;
}

function AdjustDateForTimezone(dateString) {
    //Date must end with 'Z' to indicate UTC time
    var dateMilliseconds = Date.parse(dateString);
    var UTCdate = new Date(dateMilliseconds);
    return UTCdate.setMinutes(UTCdate.getMinutes() + UTCdate.getTimezoneOffset());
}

function GetMultipleDayDates() {
    var startDate;
    var endDate;
    var today = new Date(); // current date and time
    switch (ViewModel.MultipleTimespanSelectedOptionValue()) {
        case "MultipleYear":
            startDate = new Date(today.getFullYear(), 0, 1);
            endDate = new Date(today.getFullYear(), 11, 31);
            break;
        case "MultipleMonth":
            startDate = new Date(today.getFullYear(), today.getMonth(), 1);
            endDate = new Date(today.getFullYear(), today.getMonth(), 31);
            break;
        case "MultipleSet":
            startDate = new Date(Date.parse(ViewModel.MultipleDayFrom()));
            endDate = new Date(Date.parse(ViewModel.MultipleDayTo()));
            break;

    };
    var startEndDates = { startDate: startDate.getDate() + "-" + MonthName(startDate.getMonth()) + "-" + startDate.getFullYear(), endDate: endDate.getDate() + "-" + MonthName(endDate.getMonth()) + "-" + endDate.getFullYear() };
    return startEndDates;
}

function AjaxErrorAlert(jqXHR, textStatus, errorThrown) {
    //alert("Sorry, an error occurred.\n\n Please retry your request.");
    //alert('error textStatus: ' + textStatus + ' |errorThrown: ' + errorThrown + ' |jqXHR: ' + jqXHR.responseText);
    alert('error textStatus: ' + textStatus + ' |errorThrown: ' + errorThrown);
};


function MonthName(monthInt) {
    var msg = "";
    switch (monthInt) {
        case 0:
            msg = "January";
            break;
        case 1:
            msg = "February";
            break;
        case 2:
            msg = "March";
            break;
        case 3:
            msg = "April";
            break;
        case 4:
            msg = "May";
            break;
        case 5:
            msg = "June";
            break;
        case 6:
            msg = "July";
            break;
        case 7:
            msg = "August";
            break;
        case 8:
            msg = "September";
            break;
        case 9:
            msg = "October";
            break;
        case 10:
            msg = "November";
            break;
        case 11:
            msg = "December";
            break;
    }
    return msg;
}
/************************** END Utility functions  ****************************************/

/************************** Data Fetching functions  ****************************************/
function GetSolarDataJson(plotType, callback) {
    //Clear any currently displayed errors
    ViewModel.ErrorListHeading("");
    ViewModel.clearErrors();

    var dataUrl = GetDataServer();
    if (plotType.toLowerCase() == "singleday") {
        if (ViewModel.errorsSingleDay().length > 0) {
            $.each(
                ViewModel.errorsSingleDay(),
                function (intIndex, objValue) {
                    ViewModel.addError(objValue)
                });
            ViewModel.ErrorListHeading("Please correct the following data field errors");
            return;
        }
        dataUrl += "/api/series?latitude=" + encodeURIComponent(ViewModel.Latitude()) + "&longitude=" + encodeURIComponent(ViewModel.Longitude()) + "&timezone=" + ViewModel.SelectedTimezoneOption() + "&date=" + GetSingleDayDate() + "&increment=" + ViewModel.SingleDayTabularTimeIncrement() + "&daylightsaving=" + !ViewModel.SingleDayNoDSTCheckbox();
    }
    else if (plotType.toLowerCase() == "multipleday") {
        if (ViewModel.errorsMultipleDay().length > 0) {
            $.each(
                ViewModel.errorsMultipleDay(),
                function (intIndex, objValue) {
                    ViewModel.addError(objValue)
                });
            ViewModel.ErrorListHeading("Please correct the following data field errors");
            return;
        }
        dataUrl += "/api/series?latitude=" + encodeURIComponent(ViewModel.Latitude()) + "&longitude=" + encodeURIComponent(ViewModel.Longitude()) + "&timezone=" + ViewModel.SelectedTimezoneOption() + "&daylightsaving=" + !ViewModel.SingleDayNoDSTCheckbox() + "&startDateString=" + GetMultipleDayDates().startDate + "&endDateString=" + GetMultipleDayDates().endDate;

    }
    $.ajax({
        url: dataUrl,
        dataType: "json",
        type: "get",
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            callback(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            AjaxErrorAlert(jqXHR, textStatus, errorThrown);
        },
        beforeSend: function () {
            $('#loader').show();
        },
        complete: function () {
            $('#loader').hide();
        }
    });
}



function SingleDayPlotCallback(data) {
    var mappeddataAltitude = [];
    var mappeddataAzimuth = [];
    $("#tabularDataContainer").hide();
    $("#plotContainer").show();

    $.each(data.PlotData, function (i) {
        //This works
        //mappeddataAltitude.push([Date.parse(data[i].PointDateTime), data[i].Altitude])
        //mappeddataAzimuth.push([Date.parse(data[i].PointDateTime), data[i].Azimuth])
        //END This works

        if (data.PlotData[i].AltitudePointInformation) {
            var pt = {
                x: AdjustDateForTimezone(data.PlotData[i].PointDateTime + "Z"),
                y: data.PlotData[i].Altitude,
                note: data.PlotData[i].AltitudePointInformation,


                marker: {
                    enabled: true,
                }
            };
            if (i != 0 && i != (data.PlotData.length - 1))
                pt.dataLabels = {
                    verticalAlign: 'bottom', //opposite of the logical expected setting
                    enabled: true,
                    formatter: function () {
                        return this.point.note;
                    },
                    style: {
                        fontSize: '13px',
                        color: 'black',
                        fontWeight: 'normal'
                    }
                }
            else
                pt.dataLabels = {
                    enabled: true,
                    verticalAlign: 'top', //opposite of the logical expected setting
                    formatter: function () {
                        return this.point.note;
                    },
                    style: {
                        fontSize: '13px',
                        color: 'black',
                        fontWeight: 'normal'
                    }
                }
        }
        else {
            var pt = { x: AdjustDateForTimezone(data.PlotData[i].PointDateTime + "Z"), y: data.PlotData[i].Altitude, note: 'test' };
        }
        mappeddataAltitude.push(pt);

        if (data.PlotData[i].AzimuthPointInformation) {
            var pt = {
                x: AdjustDateForTimezone(data.PlotData[i].PointDateTime+'Z'),
                y: data.PlotData[i].Azimuth,
                note: data.PlotData[i].AzimuthPointInformation,

                marker: {
                    enabled: true,
                }
            };
            if (i != 0 && i == (data.length - 1))
                pt.dataLabels = {
                    verticalAlign: 'bottom', //opposite of the logical expected setting
                    enabled: true,
                    formatter: function () {
                        return this.point.note;
                    },
                    style: {
                        fontSize: '13px',
                        color: 'black',
                        fontWeight: 'normal'
                    }
                }
            else if (i != 0 && i < (data.length / 2 - 1))
                //This is to place the due east on the right side of the point
                pt.dataLabels = {
                    verticalAlign: 'top', //opposite of the logical expected setting
                    align: 'left',
                    enabled: true,
                    formatter: function () {
                        return this.point.note;
                    },
                    style: {
                        fontSize: '13px',
                        color: 'black',
                        fontWeight: 'normal'
                    }
                }
            else
                pt.dataLabels = {
                    enabled: true,
                    verticalAlign: 'top', //opposite of the logical expected setting
                    formatter: function () {
                        return this.point.note;
                    },
                    style: {
                        fontSize: '13px',
                        color: 'black',
                        fontWeight: 'normal'
                    }
                }
        }
        else {
            var pt = { x: AdjustDateForTimezone(data.PlotData[i].PointDateTime+'Z'), y: data.PlotData[i].Azimuth, note: 'test' };
        }
        mappeddataAzimuth.push(pt);

        //mappeddataAltitude.push("{note: 'test', x:" + Date.parse(data[i].PointDateTime) + ", y:" + data[i].Altitude + "}");
        //mappeddataAltitude.push("{note: 'test', x:" + Date.parse(data[i].PointDateTime) + ",y:" + data[i].Altitude + "}");
        //mappeddataAltitude.push("{note: 'test', x:Date.parse('" + data[i].PointDateTime + "'),y:" + data[i].Altitude + "}");
        //mappeddataAzimuth.push("{name: 'test', x:" + Date.parse(data[i].PointDateTime) + ",y:" + data[i].Azimuth + "}");
        //coords = [Date.parse(data[i].PointDateTime), data[i].Altitude];
        //point = { data: coords };
        //mappeddataAltitude.push(point);
    });

    chartOptionsSingleDay.series[0].data = mappeddataAltitude;
    chartOptionsSingleDay.series[0].yAxis = 0;
    chartOptionsSingleDay.series[1].data = mappeddataAzimuth;
    chartOptionsSingleDay.series[1].yAxis = 1;
    chartOptionsSingleDay.title.text = data.Title;

    if (ViewModel.RadioSelectedOptionValue() == "Altitude")
        chartOptionsSingleDay.series[1].visible = false;
    else if (ViewModel.RadioSelectedOptionValue() == "Azimuth")
        chartOptionsSingleDay.series[0].visible = false;

    var chart = new Highcharts.Chart(chartOptionsSingleDay);

}

function MultipleDayPlotCallback(data) {
    $("#tabularDataContainer").hide();
    $("#plotContainer").show();

    var mappeddataSunriseTime = [];
    var mappeddataSunriseAzimuth = [];
    var mappeddataSunsetTime = [];
    var mappeddataSunsetAzimuth = [];
    var mappeddataSolarNoonTime = [];
    var mappeddataSolarNoonAltitude = [];
    var mappeddataHoursDaylight = [];


    $.each(data.PlotData, function (i) {
        //Need logic to add note if it exists
        var ptSunriseTime = { x: Date.parse(data.PlotData[i].Date), y: CreateDate(data.PlotData[i].Sunrise), note: '' }; // This seems to be needed to prevent Chrome from plotting using UTC date
        //var ptSunriseTime = { x: Date.parse(data.PlotData[i].Date), y: Date.parse(data.PlotData[i].Sunrise), note: '' };  
        var ptSunriseAzimuth = { x: Date.parse(data.PlotData[i].Date), y: parseFloat(data.PlotData[i].SunriseAzimuth), note: '' };
        //var ptSunsetTime = { x: Date.parse(data.PlotData[i].Date), y: Date.parse(data.PlotData[i].Sunset), note: '' };
        var ptSunsetTime = { x: Date.parse(data.PlotData[i].Date), y: CreateDate(data.PlotData[i].Sunset), note: '' }; // This seems to be needed to prevent Chrome from plotting using UTC date
        var ptSunsetAzimuth = { x: Date.parse(data.PlotData[i].Date), y: parseFloat(data.PlotData[i].SunsetAzimuth), note: '' };
        //var ptSolarNoonTime = { x: Date.parse(data.PlotData[i].Date), y: Date.parse(data.PlotData[i].SolarNoon), note: '' };
        var ptSolarNoonTime = { x: Date.parse(data.PlotData[i].Date), y: CreateDate(data.PlotData[i].SolarNoon), note: '' };  // This seems to be needed to prevent Chrome from plotting using UTC date
        var ptSolarNoonAltitude = { x: Date.parse(data.PlotData[i].Date), y: parseFloat(data.PlotData[i].SolarNoonAltitude), note: '' };
        var ptHoursDaylight = { x: Date.parse(data.PlotData[i].Date), y: parseFloat(data.PlotData[i].DaylightHours), note: '' };

        mappeddataSunriseTime.push(ptSunriseTime);
        mappeddataSunriseAzimuth.push(ptSunriseAzimuth);
        mappeddataSunsetTime.push(ptSunsetTime);
        mappeddataSunsetAzimuth.push(ptSunsetAzimuth);
        mappeddataSolarNoonTime.push(ptSolarNoonTime);
        mappeddataSolarNoonAltitude.push(ptSolarNoonAltitude);
        mappeddataHoursDaylight.push(ptHoursDaylight);


    });


    chartOptionsMultipleDays.series[0].data = mappeddataSunriseTime;
    chartOptionsMultipleDays.series[1].data = mappeddataSunriseAzimuth;
    chartOptionsMultipleDays.series[2].data = mappeddataSunsetTime;
    chartOptionsMultipleDays.series[3].data = mappeddataSunsetAzimuth;
    chartOptionsMultipleDays.series[4].data = mappeddataSolarNoonTime;
    chartOptionsMultipleDays.series[5].data = mappeddataSolarNoonAltitude;
    chartOptionsMultipleDays.series[6].data = mappeddataHoursDaylight;
    chartOptionsMultipleDays.title.text = data.Title;

    for (var i = 0; i < chartOptionsMultipleDays.series.length; i++) {
        chartOptionsMultipleDays.series[i].visible = false;
    }

    switch (ViewModel.MultiplePlot()) {
        case "SunriseTime":
            chartOptionsMultipleDays.series[0].visible = true;
            break;
        case "SunriseAzimuth":
            chartOptionsMultipleDays.series[1].visible = true;
            //chart.yAxis[0].update({ type: 'linear', title: { text: 'Sunrise  Azimuth  Angle' } });
            break;
        case "SunsetTime":
            chartOptionsMultipleDays.series[2].visible = true;
            break;
        case "SunsetAzimuth":
            chartOptionsMultipleDays.series[3].visible = true;
            break;
        case "SolarNoonTime":
            chartOptionsMultipleDays.series[4].visible = true;
            break;
        case "SolarNoonAltitude":
            chartOptionsMultipleDays.series[5].visible = true;
            break;
        case "LengthOfDay":
            chartOptionsMultipleDays.series[6].visible = true;
            break;
    }

    var chart = new Highcharts.Chart(chartOptionsMultipleDays);

}

function CreateDate(dateString) {
    var a = dateString.split(/[^0-9]/);
    var d = new Date(a[0], a[1] - 1, a[2], a[3], a[4], a[5]);
    return d.getTime();
}

function LoadTabularData(dataset) {

    var dataObject;
    ViewModel.ErrorListHeading("");
    ViewModel.clearErrors();
    if (dataset == "singleday") {
        if (ViewModel.errorsSingleDayTabular().length > 0) {
            $.each(
                ViewModel.errorsSingleDayTabular(),
                function (intIndex, objValue) {
                    ViewModel.addError(objValue)
                });
            ViewModel.ErrorListHeading("Please correct the following data field errors");
            return;
        }
        var dataUrl = GetDataServer() + "/default/SingleDayTabularData"; // + ViewModel.Latitude() + "&longitude=" + ViewModel.SingleDayLongitude() + "&timezone=central&date=" + GetSingleDayDate() + "&increment=" + ViewModel.SingleDayTabularTimeIncrement() + "&daylightsaving=" + !ViewModel.SingleDayNoDSTCheckbox();
        dataObject = { latitude: ViewModel.Latitude(), longitude: ViewModel.Longitude(), timezone: ViewModel.SelectedTimezoneOption(), date: GetSingleDayDate(), increment: ViewModel.SingleDayTabularTimeIncrement(), daylightsaving: !ViewModel.SingleDayNoDSTCheckbox() };
    }
    else {
        var dataUrl = GetDataServer() + "/default/MultipleDayTabularData"; // + ViewModel.Latitude() + "&longitude=" + ViewModel.SingleDayLongitude() + "&timezone=central&date=" + GetSingleDayDate() + "&increment=" + ViewModel.SingleDayTabularTimeIncrement() + "&daylightsaving=" + !ViewModel.SingleDayNoDSTCheckbox();
        dataObject = { latitude: ViewModel.Latitude(), longitude: ViewModel.Longitude(), timezone: ViewModel.SelectedTimezoneOption(), daylightsaving: !ViewModel.SingleDayNoDSTCheckbox(), startDateString: GetMultipleDayDates().startDate, endDateString: GetMultipleDayDates().endDate };
    }

    var request = $.ajax({
        url: dataUrl,
        type: "POST",
        data: dataObject,
        dataType: "html"
    });
    request.done(function (msg) {
        $("#tabularDataContainer").show();
        $("#plotContainer").hide();
        $("#tabularDataContainer").html(msg);
    });
    request.fail(function (jqXHR, textStatus, errorThrown) {
        AjaxErrorAlert(jqXHR, textStatus, errorThrown);
    });
}

/************************** END Data Fetching functions  ****************************************/

/********************************jQuery document ready************************************************************************************/
$(function () {
    //wireup for showing/hiding map
    $("#showHideMap").click(function () {
        $("#mapContainer").slideToggle("fast", function () {
            if ($("#showHideMap").text() == "Hide clickable map")
                $("#showHideMap").text("Show clickable map");
            else
                $("#showHideMap").text("Hide clickable map");
        });
    });

    //addClick();
    var embed = document.getElementById("clickableMap");
    embed.addEventListener('load', function () {
        var svg = embed.getSVGDocument();
        // Operate upon the SVG DOM here
        svg.onclick = function (event) {
            return getClick(event);
        };

    });

    $("#singleDayPicker, #multipleDayPickerStart, #multipleDayPickerEnd").datepicker(
        {
            dateFormat: "m/d/yy",  //"dd-MM-yy"
            showOtherMonths: true,
            selectOtherMonths: true,
            showOn: "button",
            buttonImage: "Images/calendar.gif",
            buttonImageOnly: true
        }
    );
    Highcharts.setOptions({
        global: {
            useUTC: false
        }
    });
});  //end document load jquery
//********************************END jQuery document ready************************************************************************************

/************************** Highcharts configuration  ****************************************/
var chartOptionsSingleDay = {
    chart: {
        type: 'line',
        renderTo: 'plotContainer'
    },
    title: {
        text: 'Single Day Plot'
    },
    xAxis: {
        type: 'datetime'
        , dateTimeLabelFormats: {
            hour: '%l:%M %p'
        }
        //, min: 1396768020576,
    },
    yAxis: [{
        title: {
            text: 'Altitude,  degrees',
            style: {
                fontSize: '18px',
                color: 'black',
                fontWeight: 'normal'
            }
        }
    }, {
        title: {
            text: 'Azimuth,  degees',
            style: {
                fontSize: '18px',
                color: 'black',
                fontWeight: 'normal'
            }
        },
        opposite: true,
        tickInterval: 45,
        tickPositioner: function () {
            var positions = [];
            var extremes = this.getExtremes();
            var tick = Math.floor(extremes.dataMin / 45.0) * 45;
            var increment = 45;
            var tickMax = Math.ceil(extremes.dataMax / 45.0) * 45;


            for (; tick <= tickMax; tick += increment) {
                positions.push(tick);
            }
            return positions;
        }
    }],
    series: [{
        name: 'Altitude',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2,
                }
            }
        },

        data: []
    },
    {
        name: 'Azimuth',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    }

    ]
};

var chartOptionsMultipleDays = {
    chart: {
        type: 'line',
        renderTo: 'plotContainer'
    },
    title: {
        text: 'Multiple Day Plot'
    },
    legend: {
        enabled: false
    },
    tooltip: {
        formatter: function () {
            return Highcharts.dateFormat('%A, %b %e, %Y', this.x) + '<br/>' + Highcharts.dateFormat('%l:%M %p', this.y);
        }
    },
    xAxis: {
        type: 'datetime'
        , dateTimeLabelFormats: {
            month: '%e-%b-%y',
            hour: '%l:%M %p'
        }
        , labels: {
            style: {
                fontSize: '13px'
            }
        }
        , gridLineColor: '#DDDDDD'
        , gridLineWidth: 0.5

    },
    yAxis: [{
        type: 'datetime'
        , dateTimeLabelFormats: {
            hour: '%l:%M %p',
            minute: '%l:%M %p'
        },
        title: {
            text: 'Sunset Time'
            , style: {
                fontSize: '18px',
                color: 'black',
                fontWeight: 'normal'
            }
        }
        , labels: {
            style: {
                fontSize: '13px'
            }
        }
    }
    //, {
    //    title: {
    //        text: 'Sunrise Azimuth'
    //    }
    //}, {
    //    title: {
    //        text: 'Sunset Time'
    //    }
    //}, {
    //    title: {
    //        text: 'Sunset Azimuth'
    //    }
    //}, {
    //    title: {
    //        text: 'Solar Noon Time'
    //    }
    //}, {
    //    title: {
    //        text: 'Solar Noon Altitude'
    //    }
    //}, {
    //    title: {
    //        text: 'Length of Day, Hours'
    //    }
    //}

    ],
    series: [{
        name: 'SunriseTime',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2,
                }
            }
        },

        data: []
    },
    {
        name: 'SunriseAzimuth',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    },
    {
        name: 'Sunset Time',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    },
    {
        name: 'Sunset Azimuth',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    },
    {
        name: 'Solar Noon Time',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    },
    {
        name: 'Solar Noon Altitude',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    },
    {
        name: 'Hours of Daylight',
        marker: {
            enabled: false,
            states: {
                hover: {
                    fillColor: 'white',
                    lineColor: 'red',
                    lineWidth: 2
                }
            }
        },

        data: []
    }

    ]
};

/************************** END  Highcharts configuration  ****************************************/
