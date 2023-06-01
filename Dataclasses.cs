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

    /// <summary>
    /// Class BinaryNameInfo, which contains information from binary file name.
    /// </summary>
    public class NameInfo
    {
        public int StationNumber { get; set; }
        public string Sensor { get; set; }
        public string Registrator { get; set; }

        public NameInfo(int stationNumber, string sensor, string registrator)
        {
            StationNumber = stationNumber;
            Sensor = sensor;
            Registrator = registrator;
        }
    }

    /// <summary>
    /// Class BinaryRecordFileInfo, which contains record with information about binary seismic file.
    /// </summary>
    public class BinaryRecordFileInfo
    {
        public int Frequency { get; set; }
        public int DiscreteCount { get; set; }
        public string OriginName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string Path { get; set; }
        public NameInfo NameInfo { get; set; }

        public BinaryRecordFileInfo(
            int frequency,
            int discreteCount,
            string originName,
            DateTime startTime,
            DateTime stopTime,
            string path,
            NameInfo nameInfo
        )
        {
            this.Frequency = frequency;
            this.DiscreteCount = discreteCount;
            this.OriginName = originName;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.Path = path;
            this.NameInfo = nameInfo;
        }
    }
}
