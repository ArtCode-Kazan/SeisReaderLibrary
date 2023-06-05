// ***********************************************************************
// Assembly         : BinReader
// Author           : Fotuka
// Created          : 03-06-2023
//
// Last Modified By : user
// Last Modified On : 03-06-2023
// ***********************************************************************
// <copyright file="BinReader.cs" company="ArtCode">
//     Copyright ©  2022
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace BinReader
{
    public class Constants
    {
        /// <summary>
        /// The order of components which shows how channels wrote to the file.
        /// </summary>
        public const string ComponentsOrder = "ZXY";
        /// <summary>
        /// The offset which sigma files must have.
        /// </summary>
        public const int SigmaSecondsOffset = 2;
        /// <summary>
        /// The baikal7 base date time, used to calculate real DateTime of baikal files.
        /// </summary>
        public static DateTime Baikal7BaseDateTime = new DateTime(1980, 1, 1);
        /// <summary>
        /// The names for binary seismic files.
        /// </summary>
        public const string Baikal7Fmt = "Baikal7";
        public const string Baikal8Fmt = "Baikal8";
        public const string SigmaFmt = "Sigma";
        /// <summary>
        /// The extensions which binary seismic files could have.
        /// </summary>
        public const string Baikal7Extension = "00";
        public const string Baikal8Extension = "xx";
        public const string SigmaExtension = "bin";

        public static Dictionary<string, string> BinaryFileFormats
        {
            get
            {
                var sensorExtension = new Dictionary<string, string>()
                    {
                        {Constants.Baikal7Fmt, Constants.Baikal7Extension},
                        {Constants.Baikal8Fmt, Constants.Baikal8Extension},
                        {Constants.SigmaFmt, Constants.SigmaExtension}
                    };
                return sensorExtension;
            }
        }
    }

    public interface IFileHeader
    {
        dynamic BinaryRead(string path, string type, int count, int SkippingBytes = 0);
        DateTime GetDatetimeStartBaikal7(ulong timeBegin);
        bool ReadBaikal7Header(string path);
        bool ReadBaikal8Header(string path);
        bool ReadSigmaHeader(string path);
    }

    /// <summary>
    /// Class FileHeader, which describes header information of binary seicmic file.
    /// Implements the <see cref="BinReader.IFileHeader" />
    /// </summary>
    /// <seealso cref="BinReader.IFileHeader" />
    public class FileHeader : IFileHeader
    {
        public int channelCount;
        public int frequency;
        public DateTime datetimeStart;
        public Coordinate coordinate = new Coordinate(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHeader"/> class.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        public FileHeader(string path)
        {
            string extension = Path.GetExtension(path).Substring(1);

            if (Constants.BinaryFileFormats.ContainsValue(extension))
            {
                if (extension == Constants.Baikal7Extension)
                {
                    ReadBaikal7Header(path);
                }
                else if (extension == Constants.Baikal8Extension)
                {
                    ReadBaikal8Header(path);
                }
                else
                {

                    ReadSigmaHeader(path);
                }
            }
        }

        /// <summary>
        /// Read digit from binary file bytes.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        /// <param name="type">The type of data stored in memory, which need to read.</param>
        /// <param name="count">The count of data that need to read.</param>
        /// <param name="skippingBytes">Amount of bytes that need to skeep before reading.</param>
        /// <returns>dynamic.</returns>
        public virtual dynamic BinaryRead(string path, string type, int count, int skippingBytes = 0)
        {
            dynamic returnValue;

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = skippingBytes;

                using (BinaryReader binReader = new BinaryReader(fileStream))
                {
                    switch (type)
                    {
                        case "uint16":
                            returnValue = binReader.ReadUInt16();
                            break;

                        case "uint32":
                            returnValue = binReader.ReadUInt32();
                            break;

                        case "double":
                            returnValue = binReader.ReadDouble();
                            break;

                        case "long":
                            returnValue = binReader.ReadUInt64();
                            break;

                        case "string":
                            returnValue = new string(binReader.ReadChars(count));
                            break;

                        default:
                            return null;
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets the datetime start baikal7.
        /// </summary>
        /// <param name="timeBegin">The number stored in Baikal7 file of time begin.</param>
        /// <returns>The real DateTime of Baikal7.</returns>
        public virtual DateTime GetDatetimeStartBaikal7(ulong timeBegin)
        {
            ulong seconds = timeBegin / 256000000;
            return Constants.Baikal7BaseDateTime.AddSeconds(seconds);
        }

        /// <summary>
        /// Reads the Baikal7 header information, that binary seismic file contains.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        /// <returns>Writes information to class fields and returns <c>true</c></returns>
        public virtual bool ReadBaikal7Header(string path)
        {
            this.channelCount = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 0);
            this.frequency = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 22);
            ulong timeBegin = BinaryRead(path: path, type: "long", count: 1, skippingBytes: 104);
            this.datetimeStart = GetDatetimeStartBaikal7(timeBegin);
            this.coordinate.longitude = Math.Round(BinaryRead(path: path, type: "double", count: 1, skippingBytes: 80), 6);
            this.coordinate.latitude = Math.Round(BinaryRead(path: path, type: "double", count: 1, skippingBytes: 72), 6);

            return true;
        }

        /// <summary>
        /// Reads the Baikal7 header information, that binary seismic file contains.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        /// <returns>Writes information to class fields and returns <c>true</c></returns>
        public virtual bool ReadBaikal8Header(string path)
        {
            this.channelCount = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 0);
            int day = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 6);
            int month = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 8);
            int year = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 10);
            double dt = BinaryRead(path: path, type: "double", count: 1, skippingBytes: 48);
            double seconds = BinaryRead(path: path, type: "double", count: 1, skippingBytes: 56);
            this.coordinate.longitude = Math.Round(BinaryRead(path: path, type: "double", count: 1, skippingBytes: 80), 6);
            this.coordinate.latitude = Math.Round(BinaryRead(path: path, type: "double", count: 1, skippingBytes: 72), 6);
            this.datetimeStart = new DateTime(year, month, day).AddSeconds(seconds);
            this.frequency = Convert.ToInt16(1 / dt);

            return true;
        }

        /// <summary>
        /// Reads the Sigma header information, that binary seismic file contains.
        /// </summary>
        /// <param name="path">Path to the binary seismic file.</param>
        /// <returns>Writes information to class fields and returns <c>true</c></returns>
        /// <exception cref="BinReader.InvalidDateTimeValue">Invalid start reading datetime: " + Convert.ToString(e)</exception>
        /// <exception cref="BinReader.InvalidCoordinates">Invalid coordinates: " + Convert.ToString(e)</exception>
        public virtual bool ReadSigmaHeader(string path)
        {
            this.channelCount = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 12);
            this.frequency = BinaryRead(path: path, type: "uint16", count: 1, skippingBytes: 24);
            string latitudeSrc = BinaryRead(path: path, type: "string", count: 8, skippingBytes: 40);
            string longitudeSrc = BinaryRead(path: path, type: "string", count: 9, skippingBytes: 48);
            string dateSrc = Convert.ToString(BinaryRead(path: path, type: "uint32", count: 1, skippingBytes: 60));
            string timeSrc = Convert.ToString(BinaryRead(path: path, type: "uint32", count: 1, skippingBytes: 64));

            timeSrc = timeSrc.PadLeft(6, '0');
            int year = 2000 + Convert.ToInt32(dateSrc.Substring(0, 2));
            int month = Convert.ToInt32(dateSrc.Substring(2, 2));
            int day = Convert.ToInt32(dateSrc.Substring(4));
            int hours = Convert.ToInt32(timeSrc.Substring(0, 2));
            int minutes = Convert.ToInt32(timeSrc.Substring(2, 2));
            int seconds = Convert.ToInt32(timeSrc.Substring(4));

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            try
            {
                this.datetimeStart = new DateTime(year, month, day, hours, minutes, seconds);
            }
            catch (Exception e)
            {
                throw new InvalidDateTimeValue("Invalid start reading datetime: " + Convert.ToString(e));
            }

            try
            {
                this.coordinate.longitude = Math.Round((Convert.ToInt32(longitudeSrc.Substring(0, 3)) + Convert.ToDouble(longitudeSrc.Substring(3, 5), provider) / 60), 2);
                this.coordinate.latitude = Math.Round((Convert.ToInt32(latitudeSrc.Substring(0, 2)) + Convert.ToDouble(latitudeSrc.Substring(2, 4), provider) / 60), 2);
            }
            catch (Exception e)
            {
                throw new InvalidCoordinates("Invalid coordinates: " + Convert.ToString(e));
            }

            return true;
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

    public interface IBinarySeismicFile
    {
        bool IsBinaryFileAtPath(string path);
        string GetPath { get; }
        bool IsUseAvgValues { get; }
        int OriginFrequency { get; }
        int ResampleFrequency { get; }
        string FileExtension { get; }
        string FormatType { get; }
        int ChannelsCount { get; }
        int HeaderMemorySize { get; }
        int DiscreteAmount { get; }
        double SecondsDuration { get; }
        Coordinate Coordinate { get; }
        DateTimeInterval OriginDateTimeInterval { get; }
        DateTimeInterval RecordDateTimeInterval { get; }
        DateTimeInterval ReadDateTimeInterval { get; set; }
        int StartMoment { get; }
        int ResampleParameter { get; }
        int EndMoment { get; }
        string RecordType { get; }
        Dictionary<string, int> ComponentsIndex { get; }
        BinaryFileInfo ShortFileInfo { get; }
        bool IsCorrectResampleFrequency(int value);
        Int32[] Resampling(Int32[] signal, int resampleParameter);
        Int32[] GetComponentSignal(string componentName);
        Int32[] ResampleSignal(Int32[] srcSignal);
        Int32[] ReadSignal(string component = "Z");
    }

    /// <summary>
    /// Class BinarySeismicFile, describes binary seismic files and has methods for interact with data from him.
    /// Implements the <see cref="BinReader.IBinarySeismicFile" />
    /// </summary>
    /// <seealso cref="BinReader.IBinarySeismicFile" />
    public class BinarySeismicFile : IBinarySeismicFile
    {
        public string _Path;
        public bool _IsUseAvgValues;
        public FileHeader _FileHeader;
        public DateTimeInterval _ReadDatetimeInterval;

        public int _ResampleFrequency;
        public bool _IsCorrectResampleFrequency;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySeismicFile"/> class.
        /// </summary>
        /// <param name="filePath">Path to the binary seismic file.</param>
        /// <param name="resampleFrequency">The frequency that need to be in output</param>
        /// <param name="isUseAvgValues">if set to <c>true</c> average value of full signal subtract.</param>
        /// <exception cref="BinReader.BadFilePath">Invalid path - {1}</exception>
        /// <exception cref="BinReader.InvalidResampleFrequency"></exception>
        public BinarySeismicFile(string filePath, int resampleFrequency = 0, bool isUseAvgValues = false)
        {
            bool isPathCorrect = this.IsBinaryFileAtPath(filePath);

            if (isPathCorrect == false)
            {
                throw new FileFormatException();
            }

            this._Path = filePath;
            this._FileHeader = new FileHeader(this._Path);
            this._IsUseAvgValues = isUseAvgValues;

            if (this.IsCorrectResampleFrequency(resampleFrequency) == true)
            {
                this._ResampleFrequency = resampleFrequency;
            }
            else
            {
                throw new InvalidResampleFrequency();
            }

            this._ReadDatetimeInterval = this.RecordDateTimeInterval;
        }

        /// <summary>
        /// Determines whether [is binary file at path] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if [is binary file at path] [the specified path]; otherwise, <c>false</c>.</returns>
        public virtual bool IsBinaryFileAtPath(string path)
        {
            if (File.Exists(path) == true)
            {
                string extension = Path.GetExtension(path).Substring(1);

                if (Constants.BinaryFileFormats.ContainsValue(extension))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets the absolute path to binary seismic file.
        /// </summary>
        /// <value>The path.</value>
        public virtual string GetPath
        {
            get
            {
                return this._Path;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is use average values.
        /// </summary>
        /// <value><c>true</c> if this instance is use average values; otherwise, <c>false</c>.</value>
        public virtual bool IsUseAvgValues
        {
            get
            {
                return this._IsUseAvgValues;
            }
        }

        /// <summary>
        /// Gets the origin frequency.
        /// </summary>
        /// <value>The origin frequency.</value>
        public virtual int OriginFrequency
        {
            get
            {
                return this._FileHeader.frequency;
            }
        }

        /// <summary>
        /// Gets the resample frequency.
        /// </summary>
        /// <value>The resample frequency.</value>
        public virtual int ResampleFrequency
        {
            get
            {
                if (this._ResampleFrequency == 0)
                {
                    this._ResampleFrequency = this.OriginFrequency;
                }

                return this._ResampleFrequency;
            }
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>The file extension.</value>
        public virtual string FileExtension
        {
            get
            {
                return Path.GetExtension(this.GetPath).Split('.')[1];
            }
        }

        /// <summary>
        /// Gets the type of the format.
        /// </summary>
        /// <value>The type of the format.</value>
        public virtual string FormatType
        {
            get
            {
                foreach (var record in Constants.BinaryFileFormats)
                {
                    if (record.Value == this.FileExtension)
                    {
                        return record.Key;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the channels count.
        /// </summary>
        /// <value>The channels count.</value>
        public virtual int ChannelsCount
        {
            get
            {
                return this._FileHeader.channelCount;
            }
        }

        /// <summary>
        /// Gets the size of the header in memory.
        /// </summary>
        /// <value>The amount of bytes.</value>
        public virtual int HeaderMemorySize
        {
            get
            {
                return 120 + 72 * this.ChannelsCount;
            }
        }

        /// <summary>
        /// Gets the discrete amount of signal which varies from frequency and duration.
        /// </summary>
        /// <value>The discrete amount.</value>
        public virtual int DiscreteAmount
        {
            get
            {
                FileInfo file = new FileInfo(this._Path);
                long fileSize = file.Length;
                int discreteAmount = Convert.ToInt32((fileSize - this.HeaderMemorySize) / (this._FileHeader.channelCount * sizeof(int)));
                return discreteAmount;
            }
        }

        /// <summary>
        /// Gets the seconds duration of signal which varies from frequency and duration.
        /// </summary>
        /// <value>The seconds amount.</value>
        public virtual double SecondsDuration
        {
            get
            {
                int discreteCount = this.DiscreteAmount;
                int freq = this.OriginFrequency;
                int accuracy = Convert.ToInt32(Math.Log10(freq));
                double deltaSeconds = Math.Round(Convert.ToDouble(Convert.ToDouble(discreteCount) / freq), accuracy);
                return deltaSeconds;
            }
        }

        /// <summary>
        /// Gets the coordinate.
        /// </summary>
        /// <value>The coordinate.</value>
        public Coordinate Coordinate
        {
            get
            {
                return this._FileHeader.coordinate;
            }
        }

        /// <summary>
        /// Gets the default origin DateTime interval.
        /// </summary>
        /// <value>The DateTime interval.</value>
        public virtual DateTimeInterval OriginDateTimeInterval
        {
            get
            {
                return new DateTimeInterval(
                    datetimeStart: this._FileHeader.datetimeStart,
                    datetimeStop: this._FileHeader.datetimeStart.AddSeconds(this.SecondsDuration)
                );
            }
        }

        /// <summary>
        /// Gets DateTime interval with sigma offset correction.
        /// </summary>
        /// <value>The DateTime interval.</value>
        public virtual DateTimeInterval RecordDateTimeInterval
        {
            get
            {
                if (this.FormatType == Constants.SigmaFmt)
                {
                    return new DateTimeInterval(
                        datetimeStart: this.OriginDateTimeInterval.start.AddSeconds(Constants.SigmaSecondsOffset),
                        datetimeStop: this.OriginDateTimeInterval.start.AddSeconds(Constants.SigmaSecondsOffset + this.SecondsDuration)
                    );
                }
                else
                {
                    return new DateTimeInterval(
                        datetimeStart: this.OriginDateTimeInterval.start,
                        datetimeStop: this.OriginDateTimeInterval.start.AddSeconds(this.SecondsDuration)
                    );
                }
            }
        }

        /// <summary>
        /// Gets or sets the read DateTime interval.
        /// </summary>
        /// <value>The DateTime interval.</value>
        /// <exception cref="BinReader.InvalidDateTimeValue">Invalid start reading datetime</exception>
        /// <exception cref="BinReader.InvalidDateTimeValue">Invalid stop reading datetime</exception>
        public virtual DateTimeInterval ReadDateTimeInterval
        {
            get
            {
                return this._ReadDatetimeInterval;
            }

            set
            {
                double dt1 = value.start.Subtract(this.RecordDateTimeInterval.start).TotalSeconds;
                double dt2 = this.RecordDateTimeInterval.stop.Subtract(value.start).TotalSeconds;

                if (dt1 >= 0 & dt2 > 0)
                {
                    this._ReadDatetimeInterval.start = value.start;
                }
                else
                {
                    throw new InvalidDateTimeValue("Invalid start reading datetime");
                }

                dt1 = value.stop.Subtract(this.RecordDateTimeInterval.start).TotalSeconds;
                dt2 = this.RecordDateTimeInterval.stop.Subtract(value.stop).TotalSeconds;

                if (dt1 > 0 & dt2 >= 0)
                {
                    this._ReadDatetimeInterval.stop = value.stop;
                }
                else
                {
                    throw new InvalidDateTimeValue("Invalid stop reading datetime");
                }
            }
        }

        /// <summary>
        /// Gets the amount of discretes from zero to current read DateTime start.
        /// </summary>
        /// <value>The amount of discrete</value>
        public virtual int StartMoment
        {
            get
            {
                TimeSpan dtDiff = this.ReadDateTimeInterval.start.Subtract(this.RecordDateTimeInterval.start);
                double dtSeconds = dtDiff.TotalSeconds;
                return Convert.ToInt32(Math.Round(dtSeconds * this.OriginFrequency));
            }
        }

        /// <summary>
        /// Gets the resample parameter.
        /// </summary>
        /// <value>The resample parameter.</value>
        public virtual int ResampleParameter
        {
            get
            {
                double division = Convert.ToDouble(this.OriginFrequency / this.ResampleFrequency);
                return Convert.ToInt32(Math.Floor(division));
            }
        }

        /// <summary>
        /// Gets the amount of discretes from current read DateTime start to current read DateTime stop.
        /// </summary>
        /// <value>The amount of discrete</value>
        public virtual int EndMoment
        {
            get
            {
                double dt = this.ReadDateTimeInterval.stop.Subtract(this.RecordDateTimeInterval.start).TotalSeconds;
                int discreetIndex = Convert.ToInt32(Math.Round(dt * this.OriginFrequency));
                int signalLength = discreetIndex - this.StartMoment;
                signalLength -= signalLength % this.ResampleParameter;
                discreetIndex = this.StartMoment + signalLength;
                return discreetIndex;
            }
        }

        /// <summary>
        /// Gets the type of the record.
        /// </summary>
        /// <value>The type of the record.</value>
        public virtual string RecordType
        {
            get
            {
                return Constants.ComponentsOrder;
            }
        }

        /// <summary>
        /// Gets the index of the signal components.
        /// </summary>
        /// <value>The index of the components.</value>
        public virtual Dictionary<string, int> ComponentsIndex
        {
            get
            {
                var componentsIndexes = new Dictionary<string, int>();

                for (int i = 0; i < this.RecordType.Length; i++)
                {
                    componentsIndexes.Add(this.RecordType[i].ToString(), i);
                }

                return componentsIndexes;
            }
        }

        /// <summary>
        /// Gets the short binary seismis file information.
        /// </summary>
        /// <value>BinaryFileInfo, short file information</value>
        /// <seealso cref="BinReader.BinaryFileInfo" />
        public virtual BinaryFileInfo ShortFileInfo
        {
            get
            {
                return new BinaryFileInfo(
                    path: this.GetPath,
                    formatType: this.FormatType,
                    frequency: this.OriginFrequency,
                    datetimeInterval: this.RecordDateTimeInterval,
                    coordinate: this.Coordinate
                );
            }
        }

        /// <summary>
        /// Gets the information that filename contain.
        /// </summary>
        /// <value>NameInfo, information in the file name</value>
        public virtual NameInfo GetNameInfo()
        {
            string fileName = Path.GetFileName(this.GetPath);
            string[] fileNameSplits = fileName.Split('_');
            string sensor;
            string registrator;

            if (!Int32.TryParse(fileNameSplits[0], out int number))
                return null;
            if (fileNameSplits.Length == 5)
            {
                registrator = fileNameSplits[3];
                sensor = fileNameSplits[4].Split('.')[0];
            }
            else if (fileNameSplits.Length == 4)
            {
                registrator = fileNameSplits[3].Split('.')[0];
                sensor = registrator;
            }
            else
                return null;
            return new NameInfo(number, sensor, registrator);
        }

        /// <summary>
        /// Gets the record with binary seismic info.
        /// </summary>
        /// <value>RecordFileInfo, information of the binary file</value>
        public virtual RecordFileInfo GetRecordFileInfo()
        {
            RecordFileInfo fileInfo = new RecordFileInfo(
                frequency: this.OriginFrequency,
                discreteCount: this.DiscreteAmount,
                originName: Path.GetFileName(this.GetPath),
                startTime: this.OriginDateTimeInterval.start,
                stopTime: this.OriginDateTimeInterval.stop,
                path: this.GetPath,
                nameInfo: GetNameInfo()
            );
            return fileInfo;
        }

        /// <summary>
        /// Determines whether [is correct resample frequency] [the specified frequency].
        /// </summary>
        /// <param name="frequency">The checked frequency.</param>
        /// <returns><c>true</c> if [is correct resample frequency] [the specified frequency]; otherwise, <c>false</c>.</returns>
        public virtual bool IsCorrectResampleFrequency(int frequency)
        {
            if (frequency < 0)
            {
                return false;
            }
            else if (frequency == 0)
            {
                return true;
            }
            else
            {
                if (this.OriginFrequency % frequency == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Resamplings the specified signal.
        /// </summary>
        /// <param name="signal">The raw signal.</param>
        /// <param name="resampleParameter">The resample parameter.</param>
        /// <returns>Resampled Int32 array from origin with resample parameter</returns>
        public virtual Int32[] Resampling(Int32[] signal, int resampleParameter)
        {
            int discreteAmount = signal.GetLength(0);
            int resampleDiscreteAmount = (discreteAmount - (discreteAmount % resampleParameter)) / resampleParameter;
            Int32[] resampleSignal = new int[resampleDiscreteAmount];

            for (int i = 0; i < resampleDiscreteAmount; i++)
            {
                int sum = 0;
                for (int j = i * resampleParameter; j < (i + 1) * resampleParameter; j++)
                {
                    sum += signal[j];
                }
                resampleSignal[i] = sum / resampleParameter;
            }

            return resampleSignal;
        }

        /// <summary>
        /// Gets the signal of specified component.
        /// </summary>
        /// <param name="componentName">Name of the component that need to read</param>
        /// <returns>Int32 signal array</returns>
        public virtual Int32[] GetComponentSignal(string componentName)
        {
            int columnIndex;

            if (this.ChannelsCount == this.ComponentsIndex.Count)
            {
                this.ComponentsIndex.TryGetValue(componentName, out columnIndex);
            }
            else
            {
                this.ComponentsIndex.TryGetValue(componentName, out columnIndex);
                columnIndex += this.ComponentsIndex.Count;
            }

            int skipDataSize = sizeof(int) * this.ChannelsCount * this.StartMoment;
            int offsetSize = this.HeaderMemorySize + skipDataSize + columnIndex * sizeof(int);
            int stridesSize = sizeof(int) * this.ChannelsCount;
            int signalSize = this.EndMoment - this.StartMoment;

            Int32[] intArray = new Int32[signalSize];

            using (FileStream fileStream = new FileStream(this.GetPath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = offsetSize;

                using (BinaryReader binreader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < intArray.Length; i++)
                    {
                        intArray[i] = binreader.ReadInt32();
                        fileStream.Seek(stridesSize - sizeof(int), SeekOrigin.Current);
                    }
                }
            }
            return intArray;
        }

        /// <summary>
        /// Resamples the signal.
        /// </summary>
        /// <param name="srcSignal">The source signal.</param>
        /// <returns>Resampled signal</returns>
        public virtual Int32[] ResampleSignal(Int32[] srcSignal)
        {
            if (this.ResampleParameter == 1)
            {
                return srcSignal;
            }
            return this.Resampling(srcSignal, this.ResampleParameter);
        }

        /// <summary>
        /// Massive method to reads the specified signal.
        /// </summary>
        /// <param name="component">The component that need to read</param>
        /// <returns>Signal array</returns>
        /// <exception cref="BinReader.InvalidComponentName">{1} not found</exception>
        public virtual Int32[] ReadSignal(string component = "Z")
        {
            component = component.ToUpper();

            if (this.ComponentsIndex.ContainsKey(component) == false)
            {
                throw new InvalidComponentName("{1} not found", component);
            }

            Int32[] signalArray = this.GetComponentSignal(component);
            Int32[] resampleSignalArray = this.ResampleSignal(signalArray);

            if (this.IsUseAvgValues == false)
            {
                return resampleSignalArray;
            }

            Int32[] averagedSignalArray = resampleSignalArray;
            int avgValue = (int)Math.Truncate(resampleSignalArray.Average());

            for (int i = 0; i < averagedSignalArray.Length; i++)
            {
                averagedSignalArray[i] = averagedSignalArray[i] - avgValue;
            }

            return averagedSignalArray;
        }
    }

    /// <summary>
    /// Class InvalidCoordinates.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    internal class InvalidCoordinates : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCoordinates"/> class.
        /// </summary>
        public InvalidCoordinates()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCoordinates"/> class.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public InvalidCoordinates(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCoordinates"/> class.
        /// </summary>
        /// <param name="message">Сообщение об ошибке, указывающее причину создания исключения.</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение, или пустая ссылка (<see langword="Nothing" /> в Visual Basic), если внутреннее исключение не задано.</param>
        public InvalidCoordinates(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCoordinates"/> class.
        /// </summary>
        /// <param name="info">Объект <see cref="T:System.Runtime.Serialization.SerializationInfo" />, хранящий сериализованные данные объекта, относящиеся к выдаваемому исключению.</param>
        /// <param name="context">Объект <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий контекстные сведения об источнике или назначении.</param>
        protected InvalidCoordinates(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Class InvalidDateTimeValue.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    internal class InvalidDateTimeValue : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDateTimeValue"/> class.
        /// </summary>
        public InvalidDateTimeValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDateTimeValue"/> class.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public InvalidDateTimeValue(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDateTimeValue"/> class.
        /// </summary>
        /// <param name="message">Сообщение об ошибке, указывающее причину создания исключения.</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение, или пустая ссылка (<see langword="Nothing" /> в Visual Basic), если внутреннее исключение не задано.</param>
        public InvalidDateTimeValue(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDateTimeValue"/> class.
        /// </summary>
        /// <param name="info">Объект <see cref="T:System.Runtime.Serialization.SerializationInfo" />, хранящий сериализованные данные объекта, относящиеся к выдаваемому исключению.</param>
        /// <param name="context">Объект <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий контекстные сведения об источнике или назначении.</param>
        protected InvalidDateTimeValue(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Class InvalidResampleFrequency.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    internal class InvalidResampleFrequency : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidResampleFrequency"/> class.
        /// </summary>
        public InvalidResampleFrequency()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidResampleFrequency"/> class.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public InvalidResampleFrequency(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidResampleFrequency"/> class.
        /// </summary>
        /// <param name="message">Сообщение об ошибке, указывающее причину создания исключения.</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение, или пустая ссылка (<see langword="Nothing" /> в Visual Basic), если внутреннее исключение не задано.</param>
        public InvalidResampleFrequency(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidResampleFrequency"/> class.
        /// </summary>
        /// <param name="info">Объект <see cref="T:System.Runtime.Serialization.SerializationInfo" />, хранящий сериализованные данные объекта, относящиеся к выдаваемому исключению.</param>
        /// <param name="context">Объект <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий контекстные сведения об источнике или назначении.</param>
        protected InvalidResampleFrequency(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Class InvalidComponentName.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    internal class InvalidComponentName : Exception
    {
        private string v;
        private string component;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentName"/> class.
        /// </summary>
        public InvalidComponentName()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentName"/> class.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public InvalidComponentName(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentName"/> class.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="component">The component.</param>
        public InvalidComponentName(string v, string component)
        {
            this.v = v;
            this.component = component;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentName"/> class.
        /// </summary>
        /// <param name="message">Сообщение об ошибке, указывающее причину создания исключения.</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение, или пустая ссылка (<see langword="Nothing" /> в Visual Basic), если внутреннее исключение не задано.</param>
        public InvalidComponentName(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentName"/> class.
        /// </summary>
        /// <param name="info">Объект <see cref="T:System.Runtime.Serialization.SerializationInfo" />, хранящий сериализованные данные объекта, относящиеся к выдаваемому исключению.</param>
        /// <param name="context">Объект <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий контекстные сведения об источнике или назначении.</param>
        protected InvalidComponentName(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

