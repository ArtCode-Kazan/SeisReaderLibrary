using System.Globalization;
using System.Runtime.Serialization;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BinReader
{
    public class Constants
    {
        public const string ComponentsOrder = "ZXY";
        public const int SigmaSecondsOffset = 2;
        public static DateTime Baikal7BaseDateTime = new DateTime(1980, 1, 1);

        public const string Baikal7Fmt = "Baikal7";
        public const string Baikal8Fmt = "Baikal8";
        public const string SigmaFmt = "Sigma";

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

    public class Coordinate
    {
        public double longitude;
        public double latitude;

        public Coordinate(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }

    public class DateTimeInterval
    {
        public DateTime start;
        public DateTime stop;

        public DateTimeInterval(DateTime datetimeStart, DateTime datetimeStop)
        {
            this.start = datetimeStart;
            this.stop = datetimeStop;
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

    public class FileHeader : IFileHeader
    {
        public int channelCount;
        public int frequency;
        public DateTime datetimeStart;
        public Coordinate coordinate = new Coordinate(0, 0);

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
                else if (extension == Constants.SigmaExtension)
                {
                    ReadSigmaHeader(path);
                }
            }
        }

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

        public virtual DateTime GetDatetimeStartBaikal7(ulong timeBegin)
        {
            ulong seconds = timeBegin / 256000000;
            return Constants.Baikal7BaseDateTime.AddSeconds(seconds);
        }

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

    public class BinaryFileInfo : IBinaryFileInfo
    {
        public string path;
        public string formatType;
        public int frequency;
        public DateTimeInterval datetimeInterval;
        public Coordinate coordinate;

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

        public string Name
        {
            get
            {
                return Path.GetFileName(this.path);
            }
        }

        public virtual double DurationInSeconds
        {
            get
            {
                return this.datetimeInterval.stop.Subtract(this.datetimeInterval.start).TotalSeconds;
            }
        }

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

    public class BinarySeismicFile : IBinarySeismicFile
    {
        public string _Path;
        public bool _IsUseAvgValues;
        public FileHeader _FileHeader;
        public DateTimeInterval _ReadDatetimeInterval;

        public int _ResampleFrequency;
        public bool _IsCorrectResampleFrequency;

        public BinarySeismicFile(string filePath, int resampleFrequency = 0, bool isUseAvgValues = false)
        {
            bool isPathCorrect = this.IsBinaryFileAtPath(filePath);

            if (isPathCorrect == false)
            {
                throw new BadFilePath("Invalid path - {1}", _Path);
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

        public virtual bool IsBinaryFileAtPath(string path)
        {
            if (File.Exists(path) == true)
            {
                string extension = Path.GetExtension(path).Substring(1);

                if (Constants.BinaryFileFormats.ContainsValue(extension))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public virtual string GetPath
        {
            get
            {
                return this._Path;
            }
        }

        public virtual bool IsUseAvgValues
        {
            get
            {
                return this._IsUseAvgValues;
            }
        }

        public virtual int OriginFrequency
        {
            get
            {
                return this._FileHeader.frequency;
            }
        }

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

        public virtual string FileExtension
        {
            get
            {
                return Path.GetExtension(this.GetPath).Split('.')[1];
            }
        }

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

        public virtual int ChannelsCount
        {
            get
            {
                return this._FileHeader.channelCount;
            }
        }

        public virtual int HeaderMemorySize
        {
            get
            {
                return 120 + 72 * this.ChannelsCount;
            }
        }

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

        public Coordinate Coordinate
        {
            get
            {
                return this._FileHeader.coordinate;
            }
        }

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

        public virtual int StartMoment
        {
            get
            {
                TimeSpan dtDiff = this.ReadDateTimeInterval.start.Subtract(this.RecordDateTimeInterval.start);
                double dtSeconds = dtDiff.TotalSeconds;
                return Convert.ToInt32(Math.Round(dtSeconds * this.OriginFrequency));
            }
        }

        public virtual int ResampleParameter
        {
            get
            {
                double division = Convert.ToDouble(this.OriginFrequency / this.ResampleFrequency);
                return Convert.ToInt32(Math.Floor(division));
            }
        }

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

        public virtual string RecordType
        {
            get
            {
                return Constants.ComponentsOrder;
            }
        }

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

        public virtual Int32[] ResampleSignal(Int32[] srcSignal)
        {
            if (this.ResampleParameter == 1)
            {
                return srcSignal;
            }
            return this.Resampling(srcSignal, this.ResampleParameter);
        }

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

    [Serializable]
    internal class InvalidCoordinates : Exception
    {
        public InvalidCoordinates()
        {
        }

        public InvalidCoordinates(string message) : base(message)
        {
        }

        public InvalidCoordinates(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCoordinates(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class InvalidDateTimeValue : Exception
    {
        public InvalidDateTimeValue()
        {
        }

        public InvalidDateTimeValue(string message) : base(message)
        {
        }

        public InvalidDateTimeValue(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidDateTimeValue(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class InvalidResampleFrequency : Exception
    {
        public InvalidResampleFrequency()
        {
        }

        public InvalidResampleFrequency(string message) : base(message)
        {
        }

        public InvalidResampleFrequency(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidResampleFrequency(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class BadFilePath : Exception
    {
        private string v;
        private string file_path;

        public BadFilePath()
        {
        }

        public BadFilePath(string message) : base(message)
        {
        }

        public BadFilePath(string v, string file_path)
        {
            this.v = v;
            this.file_path = file_path;
        }

        public BadFilePath(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadFilePath(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class InvalidComponentName : Exception
    {
        private string v;
        private string component;

        public InvalidComponentName()
        {
        }

        public InvalidComponentName(string message) : base(message)
        {
        }

        public InvalidComponentName(string v, string component)
        {
            this.v = v;
            this.component = component;
        }

        public InvalidComponentName(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidComponentName(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

