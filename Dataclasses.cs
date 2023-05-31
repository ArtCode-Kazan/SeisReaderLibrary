using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinReader
{
    /// <summary>
    /// Class Coordinate, which contains 2 axis coordinates for work with maps.
    /// </summary>
    public class Coordinate
    {
        public double longitude;
        public double latitude;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate"/> class.
        /// </summary>
        /// <param name="longitude">The current longitude.</param>
        /// <param name="latitude">The current latitude.</param>
        public Coordinate(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }

    /// <summary>
    /// Class DateTimeInterval, which contains 2 datetimes for work with seismic files.
    /// </summary>
    public class DateTimeInterval
    {
        public DateTime start;
        public DateTime stop;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeInterval"/> class.
        /// </summary>
        /// <param name="datetimeStart">The first datetime which meaning start.</param>
        /// <param name="datetimeStop">The second datetime which meaning end or stop.</param>
        public DateTimeInterval(DateTime datetimeStart, DateTime datetimeStop)
        {
            this.start = datetimeStart;
            this.stop = datetimeStop;
        }
    }

    public interface IBinaryFileInfo
    {
        string Name { get; }
        double DurationInSeconds { get; }
        string FormattedDuration { get; }
    }

    /// <summary>
    /// Class BinaryFileInfo, stores information about binary seismic file.
    /// Implements the <see cref="BinReader.IBinaryFileInfo" />
    /// </summary>
    /// <seealso cref="BinReader.IBinaryFileInfo" />
    public class BinaryFileInfo : IBinaryFileInfo
    {
        public string path;
        public string formatType;
        public int frequency;
        public DateTimeInterval datetimeInterval;
        public Coordinate coordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileInfo"/> class.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        /// <param name="formatType">Type of the format.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="datetimeInterval">The DateTime interval.</param>
        /// <param name="coordinate">The coordinateы.</param>
        public BinaryFileInfo(
            string path,
            string formatType,
            int frequency,
            DateTimeInterval datetimeInterval,
            Coordinate coordinate
        )
        {
            this.path = path;
            this.formatType = formatType;
            this.frequency = frequency;
            this.datetimeInterval = datetimeInterval;
            this.coordinate = coordinate;
        }

        /// <summary>
        /// Gets the name of binary seismic file.
        /// </summary>
        /// <value>File name</value>
        public string Name
        {
            get
            {
                return Path.GetFileName(this.path);
            }
        }

        /// <summary>
        /// Gets the duration from start to stop in seconds.
        /// </summary>
        /// <value>The duration in seconds.</value>
        public virtual double DurationInSeconds
        {
            get
            {
                return this.datetimeInterval.stop.Subtract(this.datetimeInterval.start).TotalSeconds;
            }
        }

        /// <summary>
        /// Gets the string formatted duration from start to stop in seconds.
        /// </summary>
        /// <value>The string with formatted duration.</value>
        public virtual string FormattedDuration
        {
            get
            {
                string durationFormat;

                int days = (int)(this.DurationInSeconds / (24 * 3600));
                int hours = (int)((this.DurationInSeconds - days * 24 * 3600) / 3600);
                int minutes = (int)((this.DurationInSeconds - days * 24 * 3600 - hours * 3600) / 60);
                double seconds = this.DurationInSeconds - days * 24 * 3600 - hours * 3600 - minutes * 60;

                string hoursFmt = Convert.ToString(hours).PadLeft(2, '0');
                string minutesFmt = Convert.ToString(minutes).PadLeft(2, '0');
                string secondsFmt = string.Format("{0:f3}", seconds).PadLeft(6, '0');

                if (days != 0)
                {
                    durationFormat = days + " days " + hoursFmt + ":" + minutesFmt + ":" + secondsFmt;
                }
                else
                {
                    durationFormat = hoursFmt + ":" + minutesFmt + ":" + secondsFmt;
                }
                return durationFormat;
            }
        }
    }
}
