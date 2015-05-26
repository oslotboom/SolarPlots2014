$("#divTest").text("Changed value");

//Set up Knockout
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
    self.SingleDay = ko.observable('Day');
    self.SingleDayNoDSTCheckbox = ko.observable(false),

    self.ErrorListHeading = ko.observable();
    self.locationClick = function () { alert('test 1345'); };
    self.errorList = ko.observableArray([]);
    self.addError = function (item) {
        self.errorList.push({ errorText: item });
    };
    self.clearErrors = function () {
        self.errorList.removeAll();
    }

    //Display values
    self.Sunrise = ko.observable();
    self.Sunset = ko.observable();
    self.SolarNoon = ko.observable();
    self.SolarNoonAltitude = ko.observable();
    self.LengthOfDayHours = ko.observable();
    self.SunriseAzimuth = ko.observable();
    self.SunsetAzimuth = ko.observable();
    self.AzimuthDueWest = ko.observable();
    self.AzimuthDueEast = ko.observable();

}
$("#divTest").text("Second Changed value");
//activate knockout
try {
    var ViewModel = new AppViewModel();
    $("#divTest").text("Second Second Changed value");
    ko.applyBindings(ViewModel); // This makes Knockout get to work
    $("#divTest").text("Third Changed value");
}
catch (err) {
    var text = "Error description: " + err.message + "\n\n";
    text += "Click OK to continue.\n\n";
    alert(text);
}

/******************** Utility functions *********************/
function GetSingleDayDate() {
    var dateString = ViewModel.SingleDayDate();
    if (dateString.toLowerCase() == "today") {
        var day = new Date();
        dateString = day.getDate() + "-" + MonthName(day.getMonth()) + "-" + day.getFullYear();
    }
    return dateString;
}


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
function AjaxErrorAlert(jqXHR, textStatus, errorThrown) {
    //alert("Sorry, an error occurred.\n\n Please retry your request.");
    //alert('error textStatus: ' + textStatus + ' |errorThrown: ' + errorThrown + ' |jqXHR: ' + jqXHR.responseText);
    alert('error textStatus: ' + textStatus + ' |errorThrown: ' + errorThrown);
};

function LocationOnClick(e) {
    var sender = (e && e.target) || (window.event && window.event.srcElement);
    ViewModel.Latitude(sender.dataset["latitude"]).Longitude(sender.dataset["longitude"]).City(sender.dataset["city"]).SelectedTimezoneOption(sender.dataset["timezone"]);
}

/******************** END Utility functions *********************/


$("#divTest").text("Fourth Changed value");

    $('#city-select li').unbind("click").on('click', function () {
        //alert("click via jqeury document ready");
        ViewModel.Latitude($(this).data("latitude")).Longitude($(this).data("longitude")).City($(this).data("city")).SelectedTimezoneOption($(this).data("timezone"));
        $("#city-select").hide();
        $("#btn-major-cities").text("Show large cities");
        if ($('#results').is(":visible"))
            $('#results').fadeTo(500, 0.4);

    });


    function FetchData() {
        //Clear any currently displayed errors
        ViewModel.ErrorListHeading("");
        ViewModel.clearErrors();

        var dataUrl = "http://" + window.location.host + "/api/basicdata?latitude=" +
              encodeURIComponent(ViewModel.Latitude()) + "&longitude=" + encodeURIComponent(ViewModel.Longitude()) + "&timezone=" + ViewModel.SelectedTimezoneOption() + "&date=" + GetSingleDayDate() + "&daylightsaving=" + !ViewModel.SingleDayNoDSTCheckbox();

        $.ajax({
            url: dataUrl,
            dataType: "json",
            type: "get",
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                ViewModel.Sunrise(data.DailyStatistics.Sunrise).Sunset(data.DailyStatistics.Sunset).SolarNoon(data.DailyStatistics.SolarNoon).SolarNoonAltitude(data.DailyStatistics.SolarNoonAltitude + "\xB0").LengthOfDayHours(data.DailyStatistics.LengthOfDayHours);
                ViewModel.SunriseAzimuth(data.DailyStatistics.SunriseAzimuth + "\xB0").SunsetAzimuth(data.DailyStatistics.SunsetAzimuth + "\xB0").AzimuthDueWest(data.DailyStatistics.AzimuthDueWest).AzimuthDueEast(data.DailyStatistics.AzimuthDueEast);
                //$("#results").show();
                $("#results").fadeTo("fast",1);
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


$(function () {
    //Initialize bootstrap datetime picker
    $('#datetimepicker1').datetimepicker({
        pickTime: false,
    });
    var today = new Date();

    var dateString = (today.getMonth() + 1).toString() + "/" + today.getDate().toString() + "/" + today.getFullYear().toString();   //today.toLocaleString();
    ViewModel.SingleDayDate(dateString);
    $('#datetimepicker1').data("DateTimePicker").setDate(dateString);

    $("#city-select").hide();
    $("#btn-major-cities").click(function () {
        $("#city-select").toggle(100, function () {
            if ($("#btn-major-cities").text() == "Show large cities")
                $("#btn-major-cities").text("Hide large cities");
            else
                $("#btn-major-cities").text("Show large cities");
        })
    });

    $("#btn-compute").click(function () { FetchData() })

    //initially hide the results block
    $("#results").hide();

    $("#Latitude,#Longitude,#TimeZone,#IgnoreDaylightSaving,#datetimepicker1").change(function () {
        if ($('#results').is(":visible") )
            $('#results').fadeTo(500, 0.4);
    });

    $("#datetimepicker1").on("dp.change", function (e) {
        ViewModel.SingleDayDate(((e.date._i.M) + 1).toString() + "/" + e.date._i.d + "/" + e.date._i.y);
    });
});