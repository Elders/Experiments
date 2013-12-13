using System;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public static class DateTimeExtensions
    {
        static readonly DateTime unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Converts a <see cref="DateTime"/> object into a unix timestamp number.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>An intger for the number of seconds since 1st January 1970, as per unix specification.</returns>
        public static int ToUnixTimestamp(this DateTime date)
        {
            TimeSpan ts = date - unixStartDate;
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// Converts a string, representing a unix timestamp number into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="timestamp">The timestamp, as a string.</param>
        /// <returns>The <see cref="DateTime"/> object the time represents.</returns>
        public static DateTime UnixTimestampToDate(string timestamp)
        {
            if (timestamp == null || timestamp.Length == 0)
                return DateTime.MinValue;

            return UnixTimestampToDate(Int32.Parse(timestamp));
        }

        /// <summary>
        /// Converts a <see cref="long"/>, representing a unix timestamp number into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="timestamp">The unix timestamp.</param>
        /// <returns>The <see cref="DateTime"/> object the time represents.</returns>
        public static DateTime UnixTimestampToDate(int timestamp)
        {
            return unixStartDate.AddSeconds(timestamp);
        }

        /// <summary>
        /// Converts a <see cref="double"/>, representing a unix timestamp + milliseconds number into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="timestamp">The unix timestamp.</param>
        /// <returns>The <see cref="DateTime"/> object the time represents.</returns>
        public static DateTime UnixTimestampToDate(double timestamp)
        {
            int unixSeconds = (int)Math.Floor(timestamp);
            var current = unixStartDate.AddSeconds(timestamp);
            return current.AddMilliseconds(timestamp - unixSeconds);
        }
    }
}
