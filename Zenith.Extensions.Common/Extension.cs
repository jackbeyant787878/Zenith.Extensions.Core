using System.Globalization;
using System.Text.RegularExpressions;
using TimeZoneConverter;
namespace Zenith.Extensions.Common
{
    public static class Extension
    {
        public static string FileNameReplaceSpecialChar(string fileName)
        {
            // / \ < > * ? : " |
            return fileName.Replace("/", "-").Replace("\\", "-").Replace("<", "-").Replace(">", "-").Replace("*", "-").Replace("?", "-").Replace(":", "-").Replace("|", "-");
        }

        public static string ToThousand(double num)
        {
            CultureInfo us = new CultureInfo("en-US");
            return num.ToString("N0", us);
        }

        public static string ToThousand2(double num)
        {
            CultureInfo us = new CultureInfo("en-US");
            return num.ToString("N2", us);
        }

        /// <summary>
        /// 转%
        /// </summary>
        /// <param name="num"></param>
        /// <param name="d">true代表需要除以100</param>
        /// <returns></returns>
        public static string ToPercent(double num, bool d = true)
        {
            CultureInfo us = new CultureInfo("en-US");
            return (d ? (num / 100) : num).ToString("P2", us);
        }

        public static string ToMoney(decimal num)
        {
            CultureInfo us = new CultureInfo("en-US");
            return num.ToString("C2", us);
        }

        public static DateTime ToDate(this string yyyyMMdd, string format = "yyyyMMdd")
        {


            var reportDatetime = DateTime.ParseExact(yyyyMMdd, format, CultureInfo.InvariantCulture);
            return reportDatetime;

        }

        public static string ToYMD(this DateTime dateTime, string format = "yyyy-MM-dd")
        {
            return dateTime.ToString(format);
        }

        public static int WeekOfYear(this DateTime startDate)
        {
            return GetIso8601WeekOfYear(startDate);
            //return Convert.ToInt32(startDate.Year.ToString() + startDate.GetWeekOfYear().ToString().PadLeft(2, '0'));
        }

        public static int MonthOfYear(this DateTime startDate)
        {
            return Convert.ToInt32(startDate.ToString("yyyyMM"));
        }

        public static DateTime? ToDateTime(this string yyyyMMdd, string format = "yyyy-MM-dd")
        {
            try
            {


                var reportDatetime = DateTime.ParseExact(yyyyMMdd, format, CultureInfo.InvariantCulture);
                return reportDatetime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="timezoneName"></param>
        /// <returns></returns>
        [Obsolete]
        public static DateTime ToTimezone_Old(this DateTime dt, string timezoneName = "America/Los_Angeles", bool isUtc = false)
        {
            try
            {
                //string displayName = "(GMT-08:00) America/Los_Angeles Time";
                //string standardName = "America/Los_Angeles";
                //TimeSpan offset = new TimeSpan(-08, 00, 00);
                //TimeZoneInfo targetTimezone = TimeZoneInfo.CreateCustomTimeZone("America/Los_Angeles", offset, displayName, standardName);

                TimeZoneInfo targetTimezone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                switch (timezoneName)
                {
                    case "Europe/Paris":
                        targetTimezone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");// ("Central Europe Standard Time");
                        break;
                    case "Europe/London":
                        targetTimezone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");// ("W. Europe Standard Time");
                        break;
                    case "America/Los_Angeles":
                        targetTimezone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        break;
                    case "Asia/Tokyo":
                        targetTimezone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                        break;
                    default:
                        targetTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
                        break;
                }

                var newtime = isUtc ? TimeZoneInfo.ConvertTimeFromUtc(dt, targetTimezone) : TimeZoneInfo.ConvertTime(dt, targetTimezone);
                return newtime;
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }

        public static DateTime ToTimezone(this DateTime dt, string timezoneName = "America/Los_Angeles", bool isUtc = false)
        {
            try
            {

                TimeZoneInfo targetTimezone = TZConvert.GetTimeZoneInfo("Pacific Standard Time");

                switch (timezoneName)
                {
                    case "Europe/Paris":
                        targetTimezone = TZConvert.GetTimeZoneInfo("Romance Standard Time");// ("Central Europe Standard Time");
                        break;
                    case "Europe/London":
                        targetTimezone = TZConvert.GetTimeZoneInfo("GMT Standard Time");// ("W. Europe Standard Time");
                        break;
                    case "America/Los_Angeles":
                        targetTimezone = TZConvert.GetTimeZoneInfo("Pacific Standard Time");
                        break;
                    case "Asia/Tokyo":
                        targetTimezone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");
                        break;
                    default:
                        targetTimezone = TZConvert.GetTimeZoneInfo(timezoneName);
                        break;
                }

                var newtime = isUtc ? TimeZoneInfo.ConvertTimeFromUtc(dt, targetTimezone) : TimeZoneInfo.ConvertTime(dt, targetTimezone);
                return newtime;
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }

        public static int GetDayofWeek(this DateTime dt)
        {
            return (int)dt.DayOfWeek;
        }

        /// <summary>
        /// 取得某月的第一天
        /// </summary>
        /// <param name="datetime">要取得月份第一天的时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(this DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }

        /// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 周日->周六 算一周
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int GetWeekOfYear(this DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();
            int weekOfYear = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
            return weekOfYear;
        }

        private static Calendar cal = CultureInfo.InvariantCulture.Calendar;

        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            DayOfWeek day = cal.GetDayOfWeek(time);
            var thursday = time.AddDays(DayOfWeek.Thursday - day);
            return thursday.Year * 100 + cal.GetWeekOfYear(thursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
        }

        /// <summary>
        /// 根据年，年周，算这年周的最后一天
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekofyear"></param>
        /// <returns></returns>
        public static DateTime CalcWeekDay(int year, int weekofyear)
        {
            //年份超限
            if (year < 1700 || year > 9999) return DateTime.Now.Date;
            //周数错误
            if (weekofyear < 1 || weekofyear > 53) return DateTime.Now.Date;
            //指定年范围
            DateTime start = new DateTime(year, 1, 1);
            int startWeekDay = (int)start.DayOfWeek;
            //周的起始日期
            var first = start.AddDays((7 - startWeekDay) + (weekofyear - 2) * 7);
            var last = first.AddDays(6);

            return last;
        }

        /// <summary>
        /// 得到本周第一天(以星期天为第一天)
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime GetWeekFirstDaySun(DateTime datetime)
        {
            //星期天为第一天
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);
            int daydiff = (-1) * weeknow;

            //本周第一天
            string FirstDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(FirstDay);
        }

        /// <summary>
        /// 得到本周最后一天(以星期六为最后一天)
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime GetWeekLastDaySat(DateTime datetime)
        {
            //星期六为最后一天
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);
            int daydiff = (7 - weeknow) - 1;

            //本周最后一天
            string LastDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(LastDay);
        }

        /// <summary>
        ///  201853 返回 2018-12-30
        /// </summary>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        public static DateTime CalcWeekFirstByweekOfYear(int weekOfYear)
        {
            if (weekOfYear < 100000)
                return DateTime.Now.Date;
            int year;
            int week;
            int.TryParse(weekOfYear.ToString().Substring(0, 4), out year);
            int.TryParse(weekOfYear.ToString().Substring(4, 2), out week);

            return CalcWeekDayFirst(year, week);
        }

        /// <summary>
        ///  201853 返回 2019-01-05
        /// </summary>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        public static DateTime CalcWeekLastByweekOfYear(int weekOfYear)
        {
            if (weekOfYear < 100000)
                return DateTime.Now.Date;
            int year;
            int week;
            int.TryParse(weekOfYear.ToString().Substring(0, 4), out year);
            int.TryParse(weekOfYear.ToString().Substring(4, 2), out week);

            return CalcWeekDayLast(year, week);
        }

        /// <summary>
        ///  201901 返回 01-2019
        /// </summary>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        public static string GetweekOfYearFormat(int weekOfYear)
        {
            if (weekOfYear < 100000)
                return weekOfYear.ToString();
            string year;
            string week;
            year = weekOfYear.ToString().Substring(0, 4);
            week = weekOfYear.ToString().Substring(4, 2);

            return week + "-" + year;
        }

        /// <summary>
        /// 根据年，年周，算这年周的第一天 美国 周日 是第一天
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekofyear"></param>
        /// <returns></returns>
        public static DateTime CalcWeekDayFirst(int year, int weekofyear)
        {
            //年份超限
            if (year < 1700 || year > 9999) return DateTime.Now.Date;
            //周数错误
            if (weekofyear < 1 || weekofyear > 53) return DateTime.Now.Date;
            //指定年范围
            DateTime start = new DateTime(year, 1, 1);
            int startWeekDay = (int)start.DayOfWeek;
            //周的起始日期
            var first = start.AddDays((7 - startWeekDay) + (weekofyear - 2) * 7);
            return first;
        }

        /// <summary>
        /// 根据年，年周，算这年周的最后天 美国 周日 是第一天
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekofyear"></param>
        /// <returns></returns>
        public static DateTime CalcWeekDayLast(int year, int weekofyear)
        {
            //年份超限
            if (year < 1700 || year > 9999) return DateTime.Now.Date;
            //周数错误
            if (weekofyear < 1 || weekofyear > 53) return DateTime.Now.Date;
            //指定年范围
            DateTime start = new DateTime(year, 1, 1);
            int startWeekDay = (int)start.DayOfWeek;
            //周的起始日期
            var first = start.AddDays((7 - startWeekDay) + (weekofyear - 2) * 7);
            return first.AddDays(6);
        }

        /// <summary>
        /// 时间转 Aug 10
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string MonthDayConvert(this DateTime dt)
        {
            return dt.ToString("MM/dd/yyyy", new CultureInfo("en-US"));
        }

        /// <summary>
        /// QuarterOfYear 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>20191,20192,20193,20194</returns>
        public static int GetQuarterOfYear(this DateTime dt)
        {

            return dt.Year * 10 + Convert.ToInt16(Math.Ceiling(dt.Month / 3.0));
            ;
        }

        /// <summary>
        /// 根据 季度获取该季度的月
        /// </summary>
        /// <param name="quarter"></param>
        /// <returns></returns>
        public static int[] GetMonthsByQuarter(int quarter)
        {
            var month = new int[3];
            switch (quarter)
            {
                case 1:
                    month = new int[] { 1, 2, 3 };
                    break;
                case 2:
                    month = new int[] { 4, 5, 6 };
                    break;
                case 3:
                    month = new int[] { 7, 8, 9 };
                    break;
                case 4:
                    month = new int[] { 10, 11, 12 };
                    break;
                default:
                    break;
            }
            return month;
        }

        public static DateTime GetTime(this long timeStamp, string timezon = "America/Los_Angeles")
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = dtStart.ToTimezone();
            //long lTime = long.Parse(timeStamp +"0000");
            //TimeSpan toNow = new TimeSpan(lTime);
            return time.AddMilliseconds(timeStamp);
        }

        public static DateTime GegDate(this string str)
        {
            var reportDatetime = DateTime.ParseExact(str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return reportDatetime;
        }

        public static DateTime getDate(this string str)
        {
            var reportDatetime = DateTime.ParseExact(str, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            return reportDatetime;
        }

        public static string GetMd5(string value)
        {
            byte[] textBytes = System.Text.Encoding.Default.GetBytes(value);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
                cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

        public static int? ToInt(this string str)
        {
            try
            {


                var strtemp = str.Replace(",", "");
                if (string.IsNullOrEmpty(str.Trim()) || str.Trim() == "—")
                {
                    return null;
                }
                return Convert.ToInt32(strtemp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Sub(this string str, int maxCount)
        {
            try
            {
                if (!string.IsNullOrEmpty(str.Trim()) && str.Length > maxCount)
                {
                    var res = str.Substring(0, maxCount);
                    return res;
                }

            }
            catch (Exception)
            {

            }
            return str;
        }

        public static string Take(this string str, int count)
        {
            if (count > 0 && str.Length > count)
            {
                return str.Substring(0, count);
            }
            return str;
        }

        /// <summary>
        /// 输出sql需要的in params
        /// </summary>
        /// <param name="list">List {'a','b','c'}</param>
        /// <returns>'a','b','c'</returns>
        public static string ToSqlInParams(this IEnumerable<string> list)
        {
            var querysParams = list.Aggregate("", (a, b) => a + $"'{b}',");
            querysParams = querysParams.Take(querysParams.Count() - 1);
            return querysParams;
        }

        /// <summary>
        /// 输出sql需要的in params
        /// </summary>
        /// <param name="list">List {1,2,3}</param>
        /// <returns>'a','b','c'</returns>
        public static string ToSqlInParams(this IEnumerable<long> list)
        {
            var querysParams = list.Aggregate("", (a, b) => a + $"{b},");
            querysParams = querysParams.Take(querysParams.Count() - 1);
            return querysParams;
        }

        public static Decimal? ToDecimal(this string str)
        {
            try
            {


                if (string.IsNullOrEmpty(str) || str.Trim() == "—" || str.Trim() == "-")
                {
                    return null;
                }
                var strtemp = str.Replace(",", "").Replace("%", "").Replace("$", "");

                return Convert.ToDecimal(strtemp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 12 AM => 24,12PM=>12 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int To24Hour(this string str)
        {
            try
            {
                var hour = -1;
                var isAM = str.Contains("AM");
                var number = Convert.ToInt16(str.Replace("AM", "").Replace("PM", ""));
                if (isAM && number == 12)
                {
                    hour = 24;
                }
                else if (!isAM && number == 12)
                {
                    hour = 12;
                }
                else
                {
                    hour = number + (isAM ? 0 : 12);
                }
                return hour;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }

        public static string IntMonthToString(int month)
        {
            var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            if (month > 0 && month <= 12)
                return months[month - 1];
            return "Jan";
        }

        public static int GetMonthNum(string month)
        {
            int index = 1;
            string strParaMonthn = "jan_feb_mar_apr_may_jun_jul_aug_sep_oct_nov_dec";
            string[] strSubMonth = strParaMonthn.Split("_".ToCharArray());
            index = strSubMonth.ToList().IndexOf(month.ToLower());
            return index + 1;
        }

        /// <summary>
        /// yyyyMM 转 oct-2018
        /// </summary>
        /// <param name="yearmonth"></param>
        /// <returns></returns>
        public static string YearMonthToString(string yearmonth)
        {
            var year = yearmonth.Substring(0, 4);
            var month = int.Parse(yearmonth.Substring(4, 2));
            return string.Format("{0}-{1}", IntMonthToString(month), year);
        }

        /// <summary>
        /// yyyyMM 转 oct-18
        /// </summary>
        /// <param name="yearmonth"></param>
        /// <returns></returns>
        public static string YearMonthToToString(string year, int month)
        {
            year = year.Substring(2, 2);
            //month = int.Parse(yearmonth.Substring(4, 2));
            return string.Format("{0}-{1}", IntMonthToString(month), year);
        }

        public static string GetDayToWeek(DateTime dt)
        {
            string[] weekArr = { "Sun.", "Mon.", "Tues.", "Wed.", "Thu.", "Fri.", "Sat." };
            return weekArr[Convert.ToInt32(dt.DayOfWeek.ToString("d"))].ToString();

        }

        public static string[] weekArr()
        {
            string[] weekArr = { "Sun.", "Mon.", "Tues.", "Wed.", "Thu.", "Fri.", "Sat." };
            return weekArr;
        }

        public static string GetCurrencySymbol(string countryCode)
        {
            switch (countryCode)
            {

                case "AU":
                    return "$";
                case "BR":
                    return "R$";
                case "CA":
                    return "CDN$";
                case "CN":
                    return "¥";
                case "FR":
                    return "€";
                case "DE":
                    return "€";
                case "IT":
                    return "€";
                case "JP":
                    return "￥";
                case "ES":
                    return "€";
                case "UK":
                    return "£";
                case "US":
                    return "$";
                default:
                    return "$";

            }
        }

        public static string GetCountryAddress(string countryCode)
        {
            switch (countryCode)
            {
                case "AU":
                    return "au";
                case "BR":
                    return "com.br";
                case "CA":
                    return "ca";
                case "CN":
                    return "cn";
                case "FR":
                    return "fr";
                case "DE":
                    return "de";
                case "IN":
                    return "in";
                case "IT":
                    return "it";
                case "JP":
                    return "co.jp";
                case "MX":
                    return "com.mx";
                case "NL":
                    return "nl";
                case "ES":
                    return "es";
                case "TR":
                    return "com.tr";
                case "UK":
                    return "co.uk";
                case "US":
                    return "com";
                default:
                    return "com";

            }
        }

        /// <summary>
        /// 判断是否是浮点型数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        /// <summary>
        /// 判断是否是整形
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
    }
}
