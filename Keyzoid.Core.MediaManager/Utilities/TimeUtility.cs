using System;

namespace Keyzoid.Core.MediaManager.Utilities
{
    /// <summary>
    /// Represents a utility class for time.
    /// </summary>
    public class TimeUtility
    {
        /// <summary>
        /// Gets the Unix time.
        /// </summary>
        /// <returns>The Unix time.</returns>
        public static int GetUnixTime()
        {
            return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Gets the Unix time.
        /// </summary>
        /// <param name="days">The number of days to add to now before returning Unix time.</param>
        /// <returns>The Unix time.</returns>
        public static int GetUnixTime(int days)
        {
            return (int)DateTimeOffset.UtcNow.AddDays(days).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Gets the Unix time.
        /// </summary>
        /// <param name="date">The date string to return as Unix time.</param>
        /// <returns>The Unix time.</returns>
        public static int GetUnixTime(string date)
        {
            var success = DateTime.TryParse(date, out DateTime dt);

            if (!success)
                return -1;

            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            DateTimeOffset dt2 = dt;

            return (int)dt2.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts from a Unix time.
        /// </summary>
        /// <param name="seconds">The number of seconds since 1970.</param>
        /// <returns>A date time offset of the Unix time.</returns>
        public static DateTimeOffset FromUnixTime(long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }
    }
}
