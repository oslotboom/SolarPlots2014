using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace SolarPlots.Classes
{
    public class Plot
    {
        public Plot(double latitude, double longitude, string timeZone, bool daylightSaving)
        {
            StringBuilder ExceptionMessage = new StringBuilder("Plot object constructor arguments unacceptable: ", 400);
            if (!LatitudeAcceptable(latitude) || !LongitudeAcceptable(longitude) || GetLSTMeridianDegrees(timeZone) == -1)
            {
                if (!LatitudeAcceptable(latitude))
                    ExceptionMessage.Append("Latitude must be between 20 and 50. ");
                if (!LongitudeAcceptable(longitude))
                    ExceptionMessage.Append("Latitude must be between 65 and 130. ");
                if (GetLSTMeridianDegrees(timeZone) == -1)
                {
                    ExceptionMessage.Append("Time zone '" + timeZone + "' is not valid entry. ");
                    ExceptionMessage.Append("Valid values are Eastern, Central, Mountain, and Pacific.");
                }
                throw new Exception("Plot constructor: " + ExceptionMessage);

            }
            else
            {
                _Latitude = latitude;
                _Longitude = longitude;
                _TimeZone = timeZone.ToLower();
                DaylightSaving = daylightSaving;

                //_Sunrise = null;
                //_Sunset = null;
                //note that _currentDay defaults to 1/1/1
            }
        }




        //Declare Delegate
        public delegate double ComputeLSTFromH(double H);

        //Declare class varables
        protected double _Latitude;
        protected double _Longitude;
        protected string _TimeZone;
        //N is the sequence number of the day of the year, with January 1 = 1
        protected int _N = 0;
        protected DateTime _currentDay;

        //_SunriseSunsetCalculated is a boolean set to true when the _Sunrise and _Sunset class variables are set
        protected DateTime? _SunriseLST = null;
        protected DateTime? _SunsetLST = null;
        protected DateTime? _SolarNoon = null;

        //************************************************************************************
        //Properties
        public double Latitude
        {
            get
            {
                return _Latitude;
            }
            set
            {
                _Latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return _Longitude;
            }
            set
            {
                _Longitude = value;
            }
        }

        public string TimeZone
        {
            get
            {
                return _TimeZone;
            }
            set
            {
                _TimeZone = value;
            }
        }

        private int N
        {
            get
            {
                return _N;
            }
            set
            {
                if (Nacceptable(value))
                    _N = value;
                else
                    throw new ArgumentOutOfRangeException("n", "Class variable N must be between 1 and 366");
            }
        }

        public DateTime SunriseLST
        {
            get
            {
                return _SunriseLST.Value;
            }

        }
        public DateTime Sunrise
        {
            get
            {
                if (IsDaylightSavingsTime())
                    return _SunriseLST.Value.AddHours(1);
                else
                    return _SunriseLST.Value;
            }

        }

        public double SunriseDouble { get; set; }

        public DateTime SunsetLST
        {
            get
            {
                return _SunsetLST.Value;
            }
        }

        public DateTime Sunset
        {
            get
            {
                if (IsDaylightSavingsTime())
                    return _SunsetLST.Value.AddHours(1);
                else
                    return _SunsetLST.Value;
            }
        }

        public double SunsetDouble { get; set; }

        public double DayLengthHours
        {
            get 
            {
                return SunsetDouble - SunriseDouble;
            }
        }

        public DateTime? DueEast { get; set; }

        public DateTime? DueWest { get; set; }

        public bool DaylightSaving { get; set; }


        //note that _currentDay should include the time when it is desired to make angle calcutions
        public DateTime CurrentDay
        {
            get
            {
                return _currentDay;
            }
            set
            {
                if (value.Year < 1990)
                    throw new Exception("Year must be 1990 or later");
                _currentDay = value;
                _N = _currentDay.DayOfYear;
                //set _Sunrise and _Sunset to null since any previous values are invalid with the new currentDay
                _SunriseLST = null;
                _SunsetLST = null;
            }
        }

        //Note that this property does not set the time. It goes to the default for a day.
        public string currentDayString
        {
            get
            {
                return _currentDay.ToLongDateString();
            }
            set
            {
                try
                {
                    _currentDay = DateTime.Parse(value);
                    _N = _currentDay.DayOfYear;
                }
                catch
                {
                    throw new Exception("Could not convert string into a date");
                }

            }
        }

        public DateTime SolarNoon
        {
            get
            {
                return SolarNoonLocalTime();
            }
        }

        //End Properties section
        //***************************************************************************

        //***************************************************************************
        //Nacceptable
        //returns true if N is a value between 1 and 366
        private Boolean Nacceptable()
        {
            return Nacceptable(_N);
        }

        private Boolean Nacceptable(int n)
        //Nacceptable returns true if N is acceptable, false otherwise
        {
            return !(n < 1 || n > 366);
        }



        //END Nacceptable


        //***************************************************************************
        //DECLINATION
        //decimal degrees
        //CalculateDeclination returns degrees/double
        public double GetDeclinationDegrees()
        {
            if (Nacceptable(_N))
            {
                return CalculateDeclination(_N);
            }
            else
                throw new ArgumentOutOfRangeException("_N", "Class variable N must be set and must be between 1 and 366");
        }

        //		//decimal radians
        //		//CalculateDeclination returns degrees/double
        //		public decimal GetDeclinationRadians() 
        //		{
        //			if (Nacceptable(_N)) 
        //			{
        //				return Convert.ToDecimal(DegreesToRadians(CalculateDeclination(_N)));
        //			}
        //			else
        //				throw new ArgumentOutOfRangeException("_N","Class variable N must be set and must be between 1 and 366");  
        //		}

        //double radians
        //CalculateDeclination returns degrees/double
        public double GetDeclinationRadians()
        {

            if (Nacceptable(_N))
            {
                return DegreesToRadians(CalculateDeclination(_N));
            }
            else
                throw new ArgumentOutOfRangeException("_N", "Class variable N must be set and must be between 1 and 366");
        }

        private double CalculateDeclination(int n)
        {
            return 23.45 * Math.Sin(2 * Math.PI * (284 + n) / 365);
        }

        //		public static decimal GetDeclinationDegrees(short n) 
        //		{
        //			if (Nacceptable(n)) 
        //			{
        //				return CalculateDeclination(n);
        //			}
        //			else
        //				throw new ArgumentOutOfRangeException("n","Argument n must be set and must be between 1 and 366");  
        //		}



        //		public static double GetDeclinationRadians(short n)
        //		{
        //			return CalculateDeclination(n);
        //
        //		}


        //END DECLINATION CALCULATION methods
        //***************************************************************************



        //***************************************************************************
        //LSTMeridian functions

        public double GetLSTMeridian()
        {
            return 0;
        }

        // returns baseline meridian value if found; returns -1 if not found
        public int GetLSTMeridianDegrees()
        {

            return GetLSTMeridianDegrees(_TimeZone);

        }

        public int GetLSTMeridianDegrees(String timeZone)
        {

            Int32 LSTMeridianDegrees;
            switch (timeZone.ToLower())
            {
                case "eastern":
                    LSTMeridianDegrees = 75;
                    break;
                case "central":
                    LSTMeridianDegrees = 90;
                    break;
                case "mountain":
                    LSTMeridianDegrees = 105;
                    break;
                case "pacific":
                    LSTMeridianDegrees = 120;
                    break;
                default:
                    return -1;

            }
            return LSTMeridianDegrees;

        }
        //END LSTMeridian functions
        //***************************************************************************


        //***************************************************************************
        //Sunrise and Sunset Methods methods




        //		public DateTime GetSunrise(short n)
        //		{
        //			if (Nacceptable(n)) 
        //			{
        //				_N = n;
        //				//convert value returned by ComputeSunrise to decimal format
        //				return DateTime.Now;
        //			}
        //			else
        //				throw new ArgumentOutOfRangeException("n","Class variable N must be set and must be between 1 and 365");  
        //		}

        //public double GetSunriseLST(int n)
        //{
        //    if (Nacceptable(n))
        //    {
        //        _N = n;
        //        ComputeLSTFromH computeLSTFromH = ComputeLSTFromHSunrise;
        //        return ComputeSunrise(computeLSTFromH);
        //    }
        //    else
        //        throw new ArgumentOutOfRangeException("n", "Class variable N must be set and must be between 1 and 365");
        //}


        //public void GetSunriseLocalTime(ref double timeDouble, ref DateTime? timeDateTime)
        //{
        //    //double AMoccurrenceLST = 0;
        //    //double PMoccurrenceLST = 0;
        //    //GetTimeForAltitudeLST(0, out AMoccurrenceLST, out PMoccurrenceLST);
        //    //timeDateTime = ConvertToDateTime(AMoccurrenceLST);


        //    GetSunriseLST(ref timeDouble, ref timeDateTime);



        //    if (IsDaylightSavingsTime())
        //    {
        //        timeDouble += 1;
        //        timeDateTime = timeDateTime.Value.AddHours(1);
        //    }
        //}

        //public void GetSunriseLST(ref double timeDouble, ref DateTime? timeDateTime)
        //{
        //    double SunriseH;  // = sunrise Hours 

        //    if (Nacceptable(_N))
        //    {
        //        SunriseH = GetSunriseLST(_N);

        //        timeDateTime = ConvertToDateTime(SunriseH);

        //        timeDouble = SunriseH;
        //    }
        //    else
        //        throw new ArgumentOutOfRangeException("n", "Class property currentDay must be set");

        //}
        //public void GetSunsetLocalTime(ref double timeDouble, ref DateTime? timeDateTime)
        //{
        //    GetSunsetLST(ref timeDouble, ref timeDateTime);
        //    if (IsDaylightSavingsTime())
        //    {
        //        timeDouble += 1;
        //        timeDateTime = timeDateTime.Value.AddHours(1);
        //    }
        //}


        //public void GetSunsetLST(ref double timeDouble, ref DateTime? timeDateTime)
        //{
        //    double SunsetH;  // = sunrise Hours angle

        //    if (Nacceptable(_N))
        //    {
        //        SunsetH = GetSunsetLST(_N);

        //        timeDateTime = ConvertToDateTime(SunsetH);
        //        timeDouble = SunsetH;
        //    }
        //    else
        //        throw new ArgumentOutOfRangeException("n", "GetSunsetLST: Class property currentDay must be set");

        //}


        //public double GetSunsetLST(int n)
        //{
        //    if (Nacceptable(n))
        //    {
        //        _N = n;
        //        return ComputeSunset();
        //    }
        //    else
        //        throw new ArgumentOutOfRangeException("n", "Class variable N must be set and must be between 1 and 365");
        //}

        //was protected
        public void SetSunriseAndSunsetLocalTime()
        {
            ComputeLSTFromH computeLSTFromHSunrise = ComputeLSTFromHSunrise;
            SunriseDouble = ComputeSunriseSunset(computeLSTFromHSunrise);
            _SunriseLST = ConvertToDateTime(SunriseDouble);

            ComputeLSTFromH computeLSTFromHSunset = ComputeLSTFromHSunset;
            SunsetDouble = ComputeSunriseSunset(computeLSTFromHSunset);
            _SunsetLST = ConvertToDateTime(SunsetDouble);


        }


        //ComputeSunrise returns the LST in double format (hours)
        private double ComputeSunriseSunset(ComputeLSTFromH computeLSTFromH)
        {
            ////AST = apparent solar time
            //double AST;
            ////LST is local standard time
            //double LST;
            //AST = 12 - HforZeroAltitudeDegrees() / 15;
            //if (GetLSTMeridianDegrees() == -1)
            //    throw new Exception("ComputeSunrise: time zone is not set to a recognized value.");
            //LST = AST - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
            //return LST;


            ////Loop until it converges
            //double LST;
            //LST = ComputeLSTFromH(HforDegrees(0));
            //double altitude = 0;
            //double azimuth = 0;
            //GetAngles(ConvertToDateTime(LST), ref altitude, ref azimuth);  

            //double delta = Math.Abs(0.0-altitude);
            //int count = 0;
            //while (delta >.0001 && count<20 )
            //{
            //    LST = ComputeLSTFromH(HforDegrees(-altitude));
            //    GetAngles(ConvertToDateTime(LST), ref altitude, ref azimuth);
            //    delta = Math.Abs(0.0 - altitude);
            //    count++;
            //} 
            //return LST;

            double refractionCorrection = RefractionCorrection(0);
            double LST;
            LST = computeLSTFromH(HforDegreesUncorrected(-refractionCorrection));
            double altitude = 0;
            double azimuth = 0;
            GetAnglesLST(ConvertToDateTime(LST), ref altitude, ref azimuth);  

            double delta = Math.Abs(0.0 - altitude);
            int count = 0;
            while (delta > .00005 && count < 20)
            {
                refractionCorrection += altitude;
                LST = computeLSTFromH(HforDegreesUncorrected(-refractionCorrection));
                GetAnglesLST(ConvertToDateTime(LST), ref altitude, ref azimuth);
                delta = Math.Abs(0.0 - altitude);
                count++;
            }
            return LST;

        }

        public decimal GetSunriseAzimuth()
        {
            decimal altitude = 0;
            decimal azimuth = 0;
            GetAngles(Sunrise, ref altitude, ref azimuth);
            return azimuth;
        }
        public decimal GetSunsetAzimuth()
        {
            decimal altitude = 0;
            decimal azimuth = 0;
            GetAngles(Sunset, ref altitude, ref azimuth);
            return azimuth;
        }

        public decimal GetSolarNoonAltitude()
        {
            double altitude = 0;
            double azimuth = 0;
            GetAnglesLST(SolarNoonLST(), ref altitude, ref azimuth);
            return Convert.ToDecimal(altitude);
        }

        //private double ComputeSunset()
        //{
        //    //AST = apparent solar time
        //    double AST;
        //    //LST is local standard time
        //    double LST;
        //    if (GetLSTMeridianDegrees() == -1)
        //        throw new Exception("ComputeSunset: time zone is not set to a recognized value.");
        //    AST = 12 + HforZeroAltitudeDegrees() / 15;
        //    LST = AST - GetEquationOfTime() / 60 - 4.0 * (GetLSTMeridianDegrees() / 60.0 - _Longitude / 60.0);
        //    return LST;
        //}


        private double ComputeLSTFromHSunrise(double H)
        {
            //AST = apparent solar time
            double AST;
            //LST is local standard time
            double LST;
            AST = 12 - H / 15;
            if (GetLSTMeridianDegrees() == -1)
                throw new Exception("ComputeSunrise: time zone is not set to a recognized value.");
            LST = AST - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
            return LST;
        }

        private double ComputeLSTFromHSunset(double H)
        {
            //AST = apparent solar time
            double AST;
            //LST is local standard time
            double LST;
            AST = 12 + H / 15;
            if (GetLSTMeridianDegrees() == -1)
                throw new Exception("ComputeSunrise: time zone is not set to a recognized value.");
            LST = AST - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
            return LST;
        }


        ////HforZeroAltitudeDegrees returns hours angle (H) in degrees for zero altitude
        ////This value is used for computing both sunrise and sunset
        //private double HforZeroAltitudeDegrees()
        //{
        //    try
        //    {
        //        double Huncorrected, Hcorrected;

        //        //Huncorrected is returned in degrees, with 15 degrees corresponding to 1 hour
        //        Huncorrected = RadiansToDegrees(Math.Acos(-Math.Tan(DegreesToRadians(_Latitude)) * Math.Tan(GetDeclinationRadians())));

        //        //now correct H for atmospheric refraction
        //        //HrefractionCorrectionSunriseSunset returns in minutes of time.
        //        //Since converting minutes to hours means dividing by 60 and converting to degree
        //        //requires multiplying by 15, we multiply the return value by 15/60 = 0.25

        //        Hcorrected = Huncorrected + (HrefractionCorrectionSunriseSunset(Huncorrected) * 0.25);

        //        return Hcorrected;
        //    }


        //    catch (Exception e)
        //    {
        //        throw new Exception("HforZeroAltitudeDegrees: " + e.Message);
        //    }

        //}

        //HforZeroAltitudeDegrees returns hours angle (H) in degrees for zero altitude
        //This value is used for computing both sunrise and sunset
        private double HforDegreesUncorrected(double degrees)
        {
            try
            {
                double Huncorrected, Hcorrected;

                //Huncorrected is returned in degrees, with 15 degrees corresponding to 1 hour
                Huncorrected = RadiansToDegrees(Math.Acos(Math.Sin(DegreesToRadians(degrees) / Math.Cos(DegreesToRadians(_Latitude)) / Math.Cos(GetDeclinationRadians())  - Math.Tan(DegreesToRadians(_Latitude)) * Math.Tan(GetDeclinationRadians()))));

                return Huncorrected;

                //now correct H for atmospheric refraction
                //HrefractionCorrectionSunriseSunset returns in minutes of time.
                //Since converting minutes to hours means dividing by 60 and converting to degree
                //requires multiplying by 15, we multiply the return value by 15/60 = 0.25

                Hcorrected = Huncorrected + (HrefractionCorrectionSunriseSunset(Huncorrected) * 0.25);

                return Hcorrected;
            }


            catch (Exception e)
            {
                throw new Exception("HforZeroAltitudeDegrees: " + e.Message);
            }

        }

        //returns the correction in minutes
        private double HrefractionCorrectionSunriseSunset(double H)
        {
            try
            {
                double correction;
                //WARNING: be sure to use 34.0 and 15.0 here, otherwise it is truncated to integer
                correction = 34.0 / 15.0 / Math.Cos(DegreesToRadians(_Latitude)) /
                    Math.Cos(GetDeclinationRadians()) / Math.Sin(DegreesToRadians(H));
                return correction;
            }


            catch (Exception e)
            {
                throw e;
            }
        }

        //End of Sunrise/Sunset methods
        //***************************************************************************


        //Start Due East, Due West methods
        //was protected
        public void ComputeDueEastDueWest()
        {
            //Get the angle at sunrise. If it is greater than 90, then the sun rises south of due east and sets south of due west
            double altitude = 0;
            double azimuth = 0;
            GetAnglesLST(SunriseLST, ref altitude, ref azimuth);
            if (azimuth >89) { 
                DueEast = null;
                DueWest = null;
            }
            else
            {
                double H = Math.Acos(Math.Tan(GetDeclinationRadians()) / Math.Tan(DegreesToRadians(_Latitude)) );
                //convert the Hours Angle to time
                DueEast = ConvertToDateTime(12 - RadiansToDegrees(H) / 15.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0);
                DueWest = ConvertToDateTime(12 + RadiansToDegrees(H) / 15.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0);
                if (IsDaylightSavingTime(CurrentDay,_TimeZone))
                {
                    DueEast = DueEast.Value.AddHours(1);
                    DueWest = DueWest.Value.AddHours(1);
                }
            }
        }


        //End Due East, Due West methods


        //***************************************************************************
        //Percent of Solar Noon calculations

        public double TimeForPercentOfSolarNoon(double percentOfSolarNoon)
        {
            //calculate the time of sunrise and sunset. Then scale the current time as a percent

            if (!PercentOfSolarNoonAcceptable(percentOfSolarNoon))
                throw new Exception("TimeForPercentOfSolarNoon: percent of solar noon must be between 0 and 200");

            //first make sure that _currentDay is set
            if (!IsCurrentDaySet())
                throw new Exception("TimeForPercentOfSolarNoon: currentDay property must be set");


            Double TimeToNoon = GetSolarNoon() - SunriseDouble;  //SunriseDouble was ComputeSunrise
            Double HoursForPercent = SunriseDouble + percentOfSolarNoon / 100.0 * TimeToNoon;
            return HoursForPercent;

        }

        public DateTime TimeForPercentOfSolarNoonLST(double percentOfSolarNoon)
        {
            return ConvertToDateTime(TimeForPercentOfSolarNoon(percentOfSolarNoon));

        }
        public DateTime TimeForPercentOfSolarNoonLocalTime(double percentOfSolarNoon)
        {
            if (IsDaylightSavingsTime())
                return ConvertToDateTime(TimeForPercentOfSolarNoon(percentOfSolarNoon) + 1.0);
            else
                return ConvertToDateTime(TimeForPercentOfSolarNoon(percentOfSolarNoon));

        }


        //public double PercentOfSolarNoonForTime(DateTime timeOfDay)
        //{
        //    //note that SolarNoon returns the hours in standard time

        //    double Sunrise = ComputeSunrise();
        //    double differential;

        //    double TimeToNoon = GetSolarNoon() - Sunrise;

        //    //adjust TimeOfDay for daylightsavingstime
        //    double TimeOfDayLST;  //=time of day in standard time in hours

        //    if (IsDaylightSavingsTime())
        //        TimeOfDayLST = ConvertToHours(timeOfDay) - 1.0;
        //    else
        //        TimeOfDayLST = ConvertToHours(timeOfDay);

        //    if ((TimeOfDayLST - Sunrise) < 0)
        //        differential = 0;
        //    else
        //        if ((TimeOfDayLST - Sunrise) > (2.0 * TimeToNoon))
        //            differential = 2.0 * TimeToNoon;
        //        else
        //            differential = TimeOfDayLST - Sunrise;


        //    //get the time differential between timeOfDay and sunrise
        //    try
        //    {
        //        return Math.Round(differential / TimeToNoon * 100, 2);

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("PercentOfSolarNoonForTime: could not complete computation. Error: " + e.Message);
        //    }


        //}

        //returns the time for solarNoon in hours, no adjustment for DST
        protected double GetSolarNoon()
        {
            //returns a double with the number of hours
            //note that AST (apparent solar time) = 12 at solar noon
            return 12.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
        }

        protected DateTime SolarNoonLST()
        {
            return ConvertToDateTime(GetSolarNoon());
        }

        protected DateTime SolarNoonLocalTime()
        {
            if (IsDaylightSavingsTime())
                return ConvertToDateTime(GetSolarNoon() + 1.0);
            else
                return ConvertToDateTime(GetSolarNoon());


        }




        //End of Percent of Solar Noon calculations
        //***************************************************************************


        protected DateTime ConvertToDateTime(double Hours)
        {
            //int intHours = Convert.ToInt32(Math.Floor(Hours));
            //int intMinutes;
            //int intMilliseconds;

            //double seconds = (Hours - intHours) * 3600.0;
            //int intSeconds = Convert.ToInt32(Math.Floor(seconds)); //Convert.ToInt32(Math.Round((Hours - Math.Floor(Hours)) * 3600.0, 0));
            //intMilliseconds = Convert.ToInt32(Math.Round((seconds - Math.Floor(seconds)) * 1000.0, 0));
            //if (intMilliseconds==1000)


            //if (intSeconds == 3600)
            //{
            //    intHours += 1;
            //    intMinutes = 0;
            //    intSeconds = 0;
            //    intMilliseconds = 0;
            //}
            //else
            //{
            //    intMinutes = Math.DivRem(intSeconds, 60, out intSeconds);
            //    intMilliseconds = Convert.ToInt32(Math.Round((seconds - Math.Floor(seconds)) * 1000.0, 0));
            //}

            //DateTime timeDateTime = new DateTime(_currentDay.Year,
            //    _currentDay.Month,
            //    _currentDay.Day,
            //    intHours,
            //    intMinutes,
            //    intSeconds,
            //    intMilliseconds);


            DateTime timeDateTime = new DateTime(_currentDay.Year,
            _currentDay.Month,
            _currentDay.Day);

            timeDateTime = timeDateTime.AddHours(Hours);
            return timeDateTime;
        }

        //this method takes dateTime value and converts it to hours
        protected double ConvertToHours(DateTime inDateTime)
        {
            return inDateTime.TimeOfDay.TotalHours;
        }

        //***************************************************************************


        public double GetEquationOfTime()
        {
            if (Nacceptable(_N))
                return GetEquationOfTime(_N);
            else
                throw new ArgumentOutOfRangeException("n", "Argument N must be set and must be between 1 and 365");
        }

        public double GetEquationOfTime(int n)
        {
            if (Nacceptable(n))
            {
                double B = 2 * Math.PI * (n - 81) / 364;
                return 9.87 * Math.Sin(2 * B) - 7.53 * Math.Cos(B) - 1.5 * Math.Sin(B);
            }
            else
                throw new ArgumentOutOfRangeException("n", "Class variable N must be set and must be between 1 and 365");
        }


        private Boolean LatitudeAcceptable(double latitude)
        {
            if (latitude < 20 || latitude > 50)
                return false;
            else
                return true;
        }

        private Boolean LongitudeAcceptable(double longitude)
        {
            if (longitude < 65 || longitude > 130)
                return false;
            else
                return true;
        }

        private double DegreesToRadians(double Degrees)
        {
            return Degrees * Math.PI / 180;
        }

        private double DegreesToRadians(decimal Degrees)
        {
            return Convert.ToDouble(Degrees) * Math.PI / 180;
        }

        private double RadiansToDegrees(double Radians)
        {
            return Radians * 180 / Math.PI;
        }

        private bool PercentOfSolarNoonAcceptable(double percentOfSolarNoon)
        {
            if (percentOfSolarNoon >= 0 && percentOfSolarNoon <= 200.0)
                return true;
            else
                return false;
        }


        protected bool IsDaylightSavingsTime()
        {
            //check if _currentDay is set. If not, throw an exception
            if (IsCurrentDaySet())
                return IsDaylightSavingsTime(_currentDay);
            else
                throw new Exception("Property currentDay must be set and be on or after 1/1/1990");
        }

        //returns true if daylight savings time active for the current date
        protected bool IsDaylightSavingsTime(DateTime day)
        {
            if (!DaylightSaving)
                return false;
            else
                return IsDaylightSavingTime(day, _TimeZone);
        }

        //This is a static version of IsDaylightSavingsTime
        //returns true if daylight savings time active for the current date
        public static bool IsDaylightSavingTime(DateTime day, string TimeZone)
        {
            DateTime StartDate;
            DateTime EndDate;
            if (TimeZone == "atlantic" || TimeZone == "eastern" || TimeZone == "central"
                || TimeZone == "mountain" || TimeZone == "pacific")
            {
                if (day.Year <= 2006)
                {
                    //This is old daylightsavingstime which ends in 2006
                    //Daylight Saving Time begins for most of the United States at 2 a.m. on the first Sunday of April. 
                    //Time reverts to standard time at 2 a.m. on the last Sunday of October.
                    //tempDate is set to April 1. Then convert it to the first Sunday
                    StartDate = new DateTime(day.Year, 4, 1);
                    int DayOfWeekStartDate = (int)StartDate.DayOfWeek;

                    if (StartDate.DayOfWeek != DayOfWeek.Sunday)
                        StartDate = StartDate.AddDays((double)(7 - DayOfWeekStartDate));
                    EndDate = new DateTime(day.Year, 11, 1);
                    if (EndDate.DayOfWeek == DayOfWeek.Sunday)
                        EndDate = EndDate.AddDays(-7.0);
                    else
                        EndDate = EndDate.AddDays(-(double)EndDate.DayOfWeek);

                    if (day >= StartDate && day < EndDate)
                        return true;
                    else
                        return false;
                }
                else
                {
                    //this is new daylightsavingstime, which starts in 2007
                    // Beginning in 2007, DST will begin on the second Sunday of March, and end the first Sunday of November
                    //for the startdate, find the first Sunday in March and add 7 days
                    StartDate = new DateTime(day.Year, 3, 1);
                    if (StartDate.DayOfWeek != DayOfWeek.Sunday)
                        StartDate = StartDate.AddDays((double)(7 - (int)StartDate.DayOfWeek));
                    StartDate = StartDate.AddDays(7.0);
                    EndDate = new DateTime(day.Year, 11, 1);
                    if (EndDate.DayOfWeek != DayOfWeek.Sunday)
                        EndDate = EndDate.AddDays((double)(7 - (int)EndDate.DayOfWeek));

                    if (day >= StartDate && day < EndDate)
                        return true;
                    else
                        return false;
                }
            }
            else
                throw new Exception("IsDaylightSavingsTime: not a valid time zone, or time zone not set");

        }



        private bool IsCurrentDaySet()
        {
            //note that we say any date after 1/1/1990 is acceptable
            DateTime firstDate = new DateTime(1990, 1, 1, 0, 0, 0);
            if (DateTime.Compare(_currentDay, firstDate) > 0)
                return true;
            else
                return false;

        }


        //***********************************************************************************************
        //Methods to compute data for a specific time of a day

        public void GetAngles(DateTime currentDay, ref double altitude, ref double azimuth)
        {
            //If Daylight saving time is in effect, adjust the currentDay to be one hour earlier
            GetAnglesLST((IsDaylightSavingsTime(currentDay) ? currentDay.AddHours(-1) : currentDay), ref altitude, ref azimuth);
        }

        //This gets the angles. currentDay must be in local standard time
        public void GetAnglesLST(DateTime currentDay, ref double Altitude, ref double Azimuth)
        {

            //if (DateTime.Compare(currentDay, Sunrise) < 0 || DateTime.Compare(currentDay, Sunset) > 0)
            //    throw new Exception("GetAngles: the input date/time was before sunrise or after sunset");

            //first compute the true altitude and azimuth
            //then compute the atmospheric refraction
            double altitude;
            double azimuth;
            double intermediate0;  //used for computation purposes only
            double intermediate1;  //used for computation purposes only
            double intermediate2;  //used for computation purposes only
            double LST;  //local standard time
            double AST;  //apparent Solar Time
            double H;  //hours angle

            //convert the currentDay (with its time) to an Apparent Solar Time (AST)
            LST = currentDay.Hour + (currentDay.Minute / 60.0) + (currentDay.Second / 3600.0);


            AST = LST + GetEquationOfTime(currentDay.DayOfYear) / 60.0 + 4.0 * (GetLSTMeridianDegrees() - _Longitude) / 60.0;
            H = 15.0 * (AST - 12);
            intermediate0 = Math.Cos(DegreesToRadians(_Latitude)) *
                Math.Cos(GetDeclinationRadians()) *
                Math.Cos(DegreesToRadians(H)) +
                Math.Sin(DegreesToRadians(_Latitude)) *
                Math.Sin(GetDeclinationRadians());
            intermediate1 = Math.Pow(intermediate0, 2);
            intermediate2 = Math.Sqrt(1 - intermediate1);
            altitude = RadiansToDegrees(Math.Atan(intermediate0 / intermediate2));


            //apply the correction factor
            altitude += RefractionCorrection(altitude);

            //azimuth
            double x_azimuth = Math.Sin(DegreesToRadians(H)) * Math.Cos(GetDeclinationRadians());
            double y_azimuth = -Math.Cos(DegreesToRadians(H)) *
                        Math.Cos(GetDeclinationRadians()) *
                        Math.Sin(DegreesToRadians(_Latitude)) +
                        (Math.Cos(DegreesToRadians(_Latitude)) *
                        Math.Sin(GetDeclinationRadians()));
            double rawAngle = RadiansToDegrees(Math.Atan(x_azimuth / y_azimuth));
            //if (x_azimuth < 0 && y_azimuth > 0)
            //    azimuth = -180.0 - rawAngle;
            //else if (x_azimuth <= 0 && y_azimuth < 0)
            //    azimuth = -rawAngle;
            //else if (x_azimuth > 0 && y_azimuth < 0)
            //    azimuth = -rawAngle;
            //else if (x_azimuth > 0 && y_azimuth > 0)
            //    azimuth = 180.0 - rawAngle;
            //else
            //    throw new Exception("GetAngles: Could not determine azimuth equation");

            if (x_azimuth < 0 && y_azimuth > 0)
                azimuth = -rawAngle;
            else if ((x_azimuth <= 0 && y_azimuth < 0) || (x_azimuth > 0 && y_azimuth < 0))
                azimuth = 180-rawAngle;
            else if (x_azimuth > 0 && y_azimuth > 0)
                azimuth = 360 - rawAngle;
            else
                throw new Exception("GetAngles: Could not determine azimuth equation");


            Altitude = altitude;
            Azimuth = azimuth;
        }

        public void GetAngles(DateTime currentDay, ref decimal Altitude, ref decimal Azimuth)
        {
            double altitude = Convert.ToDouble(Altitude);
            double azimuth = Convert.ToDouble(Azimuth);
            GetAngles(currentDay, ref altitude, ref azimuth);  
            Altitude = Math.Round(Convert.ToDecimal(altitude),2, MidpointRounding.AwayFromZero);
            Azimuth = Math.Round(Convert.ToDecimal(azimuth),2, MidpointRounding.AwayFromZero);
        }

        //Note that the calculation for <15 degrees should be used with the refracted angle
        //However, it is also used with the calculated angle for simplicity
        //input is in degrees
        private double RefractionCorrection(double altitude)
        {
            double R;
            double Rprev;
            double Rnew;
            short c = 0;
            double Delta = 0.0001;  //Delta is the convergence criterion

            //apply the correction factor
            if (altitude < 15)
            {
                //The equation uses the apparent altitude for the computation of R
                //(per page 37, practical astronomy with your calculator)
                //To achieve this, run a loop until the R values converge

                Rnew = 1000 * (.1594 + .0196 * altitude + .00002 * Math.Pow(altitude, 2)) / 300.0 /
                    (1 + 0.505 * altitude + 0.0845 * Math.Pow(altitude, 2));

                //loop until the difference between values is .0001 (=.01%)
                do
                {
                    Rprev = Rnew;
                    Rnew = 1000 * (.1594 + .0196 * (altitude + Rprev) + .00002 * Math.Pow((altitude + Rprev), 2)) / 300.0 /
                        (1 + 0.505 * (altitude + Rprev) + 0.0845 * Math.Pow((altitude + Rprev), 2));
                    c += 1;
                }

                while (Math.Abs(Rnew - Rprev) / Rprev > Delta && c < 100);

                //if c=100, throw an exception 
                if (c >= 100)
                    throw new Exception("RefractionCorrection: could not get convergence on R value, altitude < 15");
                else
                    R = Rnew;
            }


            else
            {
                Rnew = 1000 * .00452 * Math.Tan(Math.PI / 2.0 - DegreesToRadians(altitude)) / 300.0;
                //loop until the difference between values is .0001 (=.01%)
                do
                {
                    Rprev = Rnew;
                    Rnew = 1000 * .00452 * Math.Tan(Math.PI / 2.0 - DegreesToRadians(altitude + Rprev)) / 300.0;
                    c += 1;
                }

                while (Math.Abs(Rnew - Rprev) / Rprev > Delta && c < 100);

                //if c=100, throw an exception 
                if (c >= 100)
                    throw new Exception("RefractionCorrection: could not get convergence on R value, altitude > 15");
                else
                    R = Rnew;

            }

            return R;
        }

        public void GetAnglesByPercentofSolarNoon(double percentOfSolarNoon, ref double Altitude, ref double Azimuth)
        {
            //convert the percentOfNoon to an exact time
            //NOTE: the _currentDay must be set for the class
            if (!IsCurrentDaySet())
            {
                throw new Exception("GetAnglesByPercentofSolarNoon: the class property currentDay must be set first");
            }
            else
            {
                DateTime currentTime = TimeForPercentOfSolarNoonLocalTime(percentOfSolarNoon);
                GetAngles(currentTime, ref Altitude, ref Azimuth);

            }

        }

        //For the class value of DateTime, return the times that the altitude occurs
        //Note that this will normall return two times, one in the AM and one in the PM
        //Note that an exception is thrown if the angle is less than 0 or more than the maximum angle for the day
        public void GetTimeForAltitudeLST(double altitude, out double AMoccurrenceLST, out double PMoccurrenceLST)
        {
            if (!IsCurrentDaySet())
                //throw an exception since the day must be set
                throw new Exception("GetTimeForAltitude: The property currentDay must be set");
            else
            {
                //first compute maximum altitude for the day
                double MaxAltitude =
                    RadiansToDegrees(
                    Math.Asin(
                    Math.Cos(DegreesToRadians(_Latitude)) *
                    Math.Cos(GetDeclinationRadians())
                    +
                    Math.Sin(DegreesToRadians(_Latitude)) *
                    Math.Sin(GetDeclinationRadians())
                    )
                    );
                if (altitude > MaxAltitude)
                    throw new Exception("GetTimeForAltitude: input altitude is greater than maximum altitude for that date");
                else
                {
                    //subtract the refraction since the calculated angle is less than the viewed angle
                    double AdjustedAltitude = altitude - RefractionCorrection(altitude);

                    //AM_H is the AM hours angle
                    double AM_H =
                        -RadiansToDegrees(Math.Acos(
                        (Math.Sin(DegreesToRadians(AdjustedAltitude)) -
                        Math.Sin(DegreesToRadians(_Latitude)) * Math.Sin(GetDeclinationRadians()))
                        /
                        (Math.Cos(DegreesToRadians(_Latitude)) * Math.Cos(GetDeclinationRadians()))));
                    AMoccurrenceLST = AM_H / 15.0 + 12.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
                    PMoccurrenceLST = -AM_H / 15.0 + 12.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;

                }
            }

        }

        public void GetTimeForAltitudeLST(double altitude, out DateTime AMoccurrenceLST, out DateTime PMoccurrenceLST)
        {
            double AMoccurrence = 0.0; //hours in double format
            double PMoccurrence = 0.0; //hours in double format
            GetTimeForAltitudeLST(altitude, out AMoccurrence, out PMoccurrence);
            //convert the doubles to dateTime
            AMoccurrenceLST = ConvertToDateTime(AMoccurrence);
            PMoccurrenceLST = ConvertToDateTime(PMoccurrence);

        }

        public void GetTimeForAltitudeLocalTime(double altitude, out DateTime AMoccurrenceLST, out DateTime PMoccurrenceLST)
        {
            double AMoccurrence = 0.0; //hours in double format
            double PMoccurrence = 0.0; //hours in double format
            GetTimeForAltitudeLST(altitude, out AMoccurrence, out PMoccurrence);
            if (IsDaylightSavingsTime())
            {
                AMoccurrence += 1.0;
                PMoccurrence += 1.0;
            }

            //convert the doubles to dateTime
            AMoccurrenceLST = ConvertToDateTime(AMoccurrence);
            PMoccurrenceLST = ConvertToDateTime(PMoccurrence);


        }


        public void GetAzimuthForAltitude(double altitude, out double azimuthAM, out double azimuthPM)
        {

            double CosAltitude;

            CosAltitude = (Math.Sin(DegreesToRadians(altitude)) * Math.Sin(DegreesToRadians(_Latitude)) -
                Math.Sin(GetDeclinationRadians()))
                /
                (Math.Cos(DegreesToRadians(altitude)) * Math.Cos(DegreesToRadians(_Latitude)));

            if (CosAltitude > 1.0)
                throw new Exception("GetTimeForAltitudeLST: the requested altitude does not occur on this day " + _currentDay.ToShortDateString());
            else
            {
                azimuthAM = -Math.Round(RadiansToDegrees(Math.Acos(CosAltitude)), 3);
                azimuthPM = -azimuthAM;
            }

        }


        //this sub receives the azimuth as input that returns the altitude
        //if the azimuth is out of the range for the day, return 0 (this situation could also warrant an exception)
        public decimal GetAltitudeForAzimuth(double azimuth)
        {

            if (!AzimuthIsValid(azimuth))
                return 0;
            else
            {
                //call the iterative function which returns the azimuth
                try
                {
                    double target = Math.Cos(DegreesToRadians(azimuth));

                    //we need to make sure the lower bound is low enough to include the refraction correction
                    //subtract 2x the correction to make sure we are low enough
                    double R = RefractionCorrection(0);

                    double Altitude = FindAltitudeForAzimuth(-2.0 * R, 90.0, target, 0);
                    R = RefractionCorrection(Altitude);
                    return Convert.ToDecimal(Math.Round(Altitude + R, 3));

                }
                catch (Exception ex)
                {
                    throw new Exception("GetAltitudeForAzimuth: an error occurred in the iterative calculation. Inner exception: " + ex.Message);
                }

            }

        }


        //This method computes the time corresponding to the input azimuth
        public double GetTimeForAzimuthLST(double azimuth)
        {
            if (!AzimuthIsValid(azimuth))
                throw new Exception("The azimuth requested is outside the bounds of the maximum and minimum for the day");
            else
            {
                //call the iterative function which returns the azimuth
                try
                {
                    double target = Math.Cos(DegreesToRadians(azimuth));

                    //we need to make sure the lower bound is low enough to include the refraction correction
                    //subtract 2x the correction to make sure we are low enough
                    double R = RefractionCorrection(0);

                    double Altitude = FindAltitudeForAzimuth(-2.0 * R, 90.0, target, 0);
                    //NOTE: do not adjust Altitude for refraction because this is a calculation in progress

                    //compute the hours angle H for this altitude and azimuth
                    //Do NOT use the equation with azimuth in it: it does not work (commented out below)
                    //					double H = RadiansToDegrees(
                    //						Math.Asin(
                    //							(Math.Sin(DegreesToRadians(azimuth)) *
                    //						Math.Cos(DegreesToRadians(Altitude))) /
                    //						Math.Cos(GetDeclinationRadians())
                    //						)
                    //						);

                    double arg = (Math.Sin(DegreesToRadians(Altitude)) -
                        Math.Sin(GetDeclinationRadians()) * Math.Sin(DegreesToRadians(_Latitude))) /
                        (Math.Cos(GetDeclinationRadians()) * Math.Cos(DegreesToRadians(_Latitude)));
                    //due to round-off error the arg value may be slightly larger than 1, which causes the Acos to fail
                    if (arg > 1)
                        arg = 1;


                    double H = RadiansToDegrees(Math.Acos(arg));

                    //if azimuth is negative, then H must also be negative
                    if (azimuth < 0)
                        H = -H;

                    //convert the Hours Angle to time
                    double LST = 12 + H / 15.0 - GetEquationOfTime() / 60.0 - (GetLSTMeridianDegrees() - _Longitude) / 15.0;
                    return LST;

                }
                catch (Exception ex)
                {
                    throw new Exception("GetTimeForAzimuthLST. Inner exception: " + ex.Message);
                }

            }

        }

        public DateTime GetTimeForAzimuthLSTDateTime(double azimuth)
        {
            return ConvertToDateTime(GetTimeForAzimuthLST(azimuth));
        }

        public DateTime GetTimeForAzimuthLocalTime(double azimuth)
        {
            double TimeForAzimuth = GetTimeForAzimuthLST(azimuth);
            if (IsDaylightSavingsTime())
                TimeForAzimuth += 1.0;

            return ConvertToDateTime(TimeForAzimuth);
        }

        //This method checks if the input azimuth is valid for the object's date.
        //if it is not, return false. If valid, return true
        private bool AzimuthIsValid(double azimuth)
        {
            //determine if the Azimuth is an acceptable value for the day
            //Get the angles for percent of solar noon = 0 and 200
            double SunriseAltitude = 0;
            double SunriseAzimuth = 0;
            double SunsetAltitude = 0;
            double SunsetAzimuth = 0;
            GetAnglesByPercentofSolarNoon(0, ref SunriseAltitude, ref SunriseAzimuth);
            GetAnglesByPercentofSolarNoon(200, ref SunsetAltitude, ref SunsetAzimuth);

            if (azimuth < SunriseAzimuth || azimuth > SunsetAzimuth)
                return false;
            else
                return true;
        }

        //This is a recursive method to iteratively find the altitude for a particular azimuth
        private double FindAltitudeForAzimuth(double lowerBound, double upperBound, double target, int recurseCount)
        {
            //NOTE NOTE NOTE: target is the cosine of the azimuth
            recurseCount += 1;

            double Delta = 0.00000001;
            if (recurseCount > 100)
                throw new Exception("Recursion count reached 100, abort to prevent infinite recursion.");


            //an upperbound=90 will cause the formula to crash.
            //if upperbound=90, reset it to 89.999
            if (Math.Abs(upperBound - 90) < Delta)
                upperBound = 89.999;


            double CurrentValue;
            double CurrentPosition;
            double PreviousPosition;
            double PreviousValue;




            double increment = (upperBound - lowerBound) / 100.0;
            PreviousPosition = lowerBound;
            PreviousValue = (Math.Sin(DegreesToRadians(PreviousPosition)) *
                Math.Sin(DegreesToRadians(_Latitude))
                - Math.Sin(GetDeclinationRadians()))
                /
                (Math.Cos(DegreesToRadians(PreviousPosition)) * Math.Cos(DegreesToRadians(_Latitude)));

            CurrentPosition = PreviousPosition + increment;

            //if the positions are on opposite sides of the target, then we are in the zone.
            //Otherwise, progress through the increments

            //if either value happens to hit the target, return the position
            if (Math.Abs(PreviousValue - target) < Delta)
                return PreviousPosition;

            while (Math.Round(CurrentPosition, 7) <= Math.Round(upperBound, 7))
            {

                CurrentValue = (Math.Sin(DegreesToRadians(CurrentPosition)) *
                    Math.Sin(DegreesToRadians(_Latitude))
                    - Math.Sin(GetDeclinationRadians()))
                    /
                    (Math.Cos(DegreesToRadians(CurrentPosition)) * Math.Cos(DegreesToRadians(_Latitude)));

                if (Math.Abs(CurrentValue - target) < Delta)
                    return CurrentPosition;

                if ((PreviousValue < target && CurrentValue > target) ||
                    (PreviousValue > target && CurrentValue < target))

                    return FindAltitudeForAzimuth(PreviousPosition, CurrentPosition, target, recurseCount);
                else
                {
                    PreviousPosition = CurrentPosition;
                    PreviousValue = CurrentValue;
                    CurrentPosition += increment;
                }

            }

            //if we reach this point we have reached the upper bound without finding a transition point
            StringBuilder message = new StringBuilder();
            message.Append("FindAltitudeForAzimuth:Could not find transition point. \n");
            throw new Exception(message.ToString());


        }


        private DateTime RoundTime(DateTime inDateTime)
        {
            DateTime intermediate = new DateTime(
                inDateTime.Year,
                inDateTime.Month,
                inDateTime.Day,
                inDateTime.Hour,
                inDateTime.Minute,
                0);

            if (inDateTime.Second < 30)
                return intermediate;
            else
            {
                return intermediate.AddMinutes(1);
            }

        }


        protected String AltitudePointMessageText(DateTime current, decimal altitude)
        {
            if (current == SolarNoon)
                return "Solar Noon, " + current.ToLongTimeString() + ", " + altitude.ToString("F1") + " degrees";
            else if (current==Sunrise)
                return "Sunrise, " + current.ToLongTimeString();
            else if (current == Sunset)
                return "Sunset, " + current.ToLongTimeString();
            else
                return String.Empty;
        }

        protected String AzimuthPointMessageText(DateTime current, decimal azimuth)
        {
            if (current == SolarNoon)
                return "Solar Noon, " + current.ToLongTimeString() + ", due south";
            else if (current == Sunrise)
                return "Sunrise, " + current.ToLongTimeString() + ", " + azimuth.ToString("F1") + " degrees";
            else if (current == Sunset)
                return "Sunset, " + current.ToLongTimeString() + ", " + azimuth.ToString("F1") + " degrees";
            else if (current == DueEast)
                return "Due East, " + current.ToLongTimeString();
            else if (current == DueWest)
                return "Due West, " + current.ToLongTimeString();
            else
                return String.Empty;
        }

    }
}