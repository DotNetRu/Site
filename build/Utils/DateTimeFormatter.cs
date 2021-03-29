using System;
using System.Globalization;

namespace DotNetRu.Site.Utils
{
    static class DateTimeFormatter
    {
        static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
        static readonly TimeZoneInfo RussianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        const string DateTimeTemplate = "yyyy-MM-ddTHH:mm:ssZ";

        public static DateTime ToDateTime(string dateTimeText)
        {
            var localTime = DateTime.ParseExact(dateTimeText, DateTimeTemplate, InvariantCulture);
            var utcTime = localTime.ToUniversalTime();
            var russianTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, RussianTimeZone);
            return russianTime;
        }

        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString(DateTimeTemplate, InvariantCulture);
        }
    }
}
