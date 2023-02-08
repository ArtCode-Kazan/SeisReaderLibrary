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
    public class Coordinates 
    {
        public double longitude;
        public double latitude;

        public Coordinates(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
    public interface IFileHeader
    {
        dynamic BinaryRead(string path, string type, int count, int SkippingBytes = 0);
        DateTime GetDatetimeStartBaikal7(ulong timeBegin);
        void ReadBaikal7Header(string path);
        void ReadBaikal8Header(string path);
        void ReadSigmaHeader(string path);
    }

    public class FileHeader : IFileHeader
    {
        public int channelCount;
        public int frequency;
        public DateTime datetimeStart;
        public Coordinates coordinates;

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

        public virtual dynamic BinaryRead(string path, string type, int count, int SkippingBytes = 0)
        {
            dynamic returnedValue;

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = SkippingBytes;

                using (BinaryReader binreader = new BinaryReader(fileStream))
                {
                    switch (type)
                    {
                        case "uint16":
                            returnedValue = binreader.ReadUInt16();
                            break;

                        case "uint32":
                            returnedValue = binreader.ReadUInt32();
                            break;

                        case "double":
                            returnedValue = binreader.ReadDouble();
                            break;

                        case "long":
                            returnedValue = binreader.ReadUInt64();
                            break;

                        case "string":
                            returnedValue = new string(binreader.ReadChars(count));
                            break;

                        default:
                            return null;
                    }
                }
            }
            return returnedValue;
        }

        public virtual DateTime GetDatetimeStartBaikal7(ulong timeBegin)
        {
            ulong seconds = timeBegin / 256000000;
            return Constants.Baikal7BaseDateTime.AddSeconds(seconds);
        }

        public virtual void ReadBaikal7Header(string path)
        {
            this.channelCount = BinaryRead(path, "uint16", 1, 0);
            this.frequency = BinaryRead(path, "uint16", 1, 22);
            ulong timeBegin = BinaryRead(path, "long", 1, 104);
            this.datetimeStart = GetDatetimeStartBaikal7(timeBegin);
            this.coordinates.longitude = Math.Round(BinaryRead(path, "double", 1, 80), 6);
            this.coordinates.latitude = Math.Round(BinaryRead(path, "double", 1, 72), 6);
        }

        public virtual void ReadBaikal8Header(string path)
        {
            this.channelCount = BinaryRead(path, "uint16", 1, 0);
            int day = BinaryRead(path, "uint16", 1, 6);
            int month = BinaryRead(path, "uint16", 1, 8);
            int year = BinaryRead(path, "uint16", 1, 10);
            double dt = BinaryRead(path, "double", 1, 48);
            double seconds = BinaryRead(path, "double", 1, 56);
            this.coordinates.latitude = Math.Round(BinaryRead(path, "double", 1, 72), 6);
            this.coordinates.longitude = Math.Round(BinaryRead(path, "double", 1, 80), 6);
            this.datetimeStart = new DateTime(year, month, day).AddSeconds(seconds);
            this.frequency = Convert.ToInt16(1 / dt);
        }

        public virtual void ReadSigmaHeader(string path)
        {
            this.channelCount = BinaryRead(path, "uint16", 1, 12);
            this.frequency = BinaryRead(path, "uint16", 1, 24);
            string latitudeSrc = BinaryRead(path, "string", 8, 40);
            string longitudeSrc = BinaryRead(path, "string", 9, 48);
            string dateSrc = Convert.ToString(BinaryRead(path, "uint32", 1, 60));
            string timeSrc = Convert.ToString(BinaryRead(path, "uint32", 1, 64));
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
                this.coordinates.longitude = Math.Round((Convert.ToInt32(longitudeSrc.Substring(0, 3)) + Convert.ToDouble(longitudeSrc.Substring(3, 5), provider) / 60), 2);
                this.coordinates.latitude = Math.Round((Convert.ToInt32(latitudeSrc.Substring(0, 2)) + Convert.ToDouble(latitudeSrc.Substring(2, 4), provider) / 60), 2);
            }
            catch (Exception e)
            {
                throw new InvalidCoordinates("Invalid coordinates: " + Convert.ToString(e));
            }
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
        public DateTime timeStart;
        public DateTime timeStop;
        public double longitude;
        public double latitude;

        public BinaryFileInfo(
            string path,
            string formatType,
            int frequency,
            DateTime dateTimeStart,
            DateTime dateTimeStop,
            double longitude,
            double latitude
            )
        {
            this.path = path;
            this.formatType = formatType;
            this.frequency = frequency;
            this.timeStart = dateTimeStart;
            this.timeStop = dateTimeStop;
            this.longitude = longitude;
            this.latitude = latitude;
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
                return this.timeStop.Subtract(this.timeStart).TotalSeconds;
            }
        }

        public virtual string FormattedDuration
        {
            get
            {
                int days = (int)(this.DurationInSeconds / (24 * 3600));
                int hours = (int)((this.DurationInSeconds - days * 24 * 3600) / 3600);
                int minutes = (int)((this.DurationInSeconds - days * 24 * 3600 - hours * 3600) / 60);
                double seconds = this.DurationInSeconds - days * 24 * 3600 - hours * 3600 - minutes * 60;

                string hoursFmt = Convert.ToString(hours).PadLeft(2, '0');
                string minutesFmt = Convert.ToString(minutes).PadLeft(2, '0');
                string secondsFmt = string.Format("{0:f3}", seconds).PadLeft(6, '0');

                string durationFormat;
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
        DateTime OriginDatetimeStart { get; }
        int ChannelsCount { get; }
        int HeaderMemorySize { get; }
        int DiscreteAmount { get; }
        double SecondsDuration { get; }
        DateTime OriginDatetimeStop { get; }
        DateTime DatetimeStart { get; }
        DateTime DatetimeStop { get; }
        Coordinates Coordinates { get; }
        DateTime ReadDatetimeStart { get; set; }
        DateTime ReadDatetimeStop { get; set; }
        int StartMoment { get; }
        int ResampleParameter { get; }
        int EndMoment { get; }
        string RecordType { get; }
        Dictionary<string, int> ComponentsIndex { get; }
        BinaryFileInfo ShortFileInfo { get; }        
        bool IsCorrectResampleFrequency(int value);
        dynamic Resampling(Int32[] signal, int ResampleParameter);
        dynamic GetComponentSignal(string componentName);
        dynamic ResampleSignal(Int32[] SrcSignal);
        dynamic ReadSignal(string component = "Z");
    }

    public class BinarySeismicFile : IBinarySeismicFile
    {
        public readonly string _Path;
        public readonly bool _IsUseAvgValues;
        public readonly FileHeader _FileHeader;
        public DateTime _ReadDatetimeStart;
        public DateTime _ReadDatetimeStop;

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

            this._ReadDatetimeStart = this.DatetimeStart;
            this._ReadDatetimeStop = this.DatetimeStop;
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
                foreach (var file in Constants.BinaryFileFormats)
                {
                    if (file.Value == this.FileExtension)
                    {
                        return file.Key;
                    }
                }

                return null;
            }
        }

        public virtual DateTime OriginDatetimeStart
        {
            get
            {
                return this._FileHeader.datetimeStart;
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
                int channelCount = this.ChannelsCount;

                return 120 + 72 * channelCount;
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

        public virtual DateTime OriginDatetimeStop
        {
            get
            {
                return this.OriginDatetimeStart.AddSeconds(this.SecondsDuration);
            }
        }

        public virtual DateTime DatetimeStart
        {
            get
            {
                if (this.FormatType == Constants.SigmaFmt)
                {
                    return this.OriginDatetimeStart.AddSeconds(Constants.SigmaSecondsOffset);
                }

                else
                {
                    return this.OriginDatetimeStart.AddSeconds(0);
                }
            }
        }

        public virtual DateTime DatetimeStop
        {
            get
            {
                return this.DatetimeStart.AddSeconds(this.SecondsDuration);
            }
        }
        
        public Coordinates Coordinates
        { 
            get 
            {
                return this._FileHeader.coordinates;
            } 
        }        

        public virtual DateTime ReadDatetimeStart
        {
            get
            {
                return this._ReadDatetimeStart;
            }
            set
            {
                double dt1 = value.Subtract(this.DatetimeStart).TotalSeconds;
                double dt2 = this.DatetimeStop.Subtract(value).TotalSeconds;

                if (dt1 >= 0 & dt2 > 0)
                {
                    this._ReadDatetimeStart = value;
                }
                else
                {
                    throw new InvalidDateTimeValue("Invalid start reading datetime");
                }
            }
        }

        public virtual DateTime ReadDatetimeStop
        {
            get
            {
                return this._ReadDatetimeStop;
            }
            set
            {
                double dt1 = value.Subtract(this.DatetimeStart).TotalSeconds;
                double dt2 = this.DatetimeStop.Subtract(value).TotalSeconds;

                if (dt1 > 0 & dt2 >= 0)
                {
                    this._ReadDatetimeStop = value;
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
                TimeSpan dtDiff = this.ReadDatetimeStart.Subtract(this.DatetimeStart);
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
                double dt = this.ReadDatetimeStop.Subtract(this.DatetimeStart).TotalSeconds;
                int discreetIndex = Convert.ToInt32(Math.Round(dt * this.OriginFrequency));
                int signalLength = discreetIndex - this.StartMoment;
                signalLength -= (signalLength % this.ResampleParameter);
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
                BinaryFileInfo value = new BinaryFileInfo(this.GetPath,
                    this.FormatType,
                    this.OriginFrequency,
                    this.DatetimeStart,
                    this.DatetimeStop,
                    this.Coordinates.longitude,
                    this.Coordinates.latitude);
                return value;
            }
        }        

        public virtual bool IsCorrectResampleFrequency(int value)
        {
            if (value < 0)
            {
                return false;
            }
            else if (value == 0)
            {
                return true;
            }
            else
            {
                if (this.OriginFrequency % value == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public virtual dynamic Resampling(Int32[] signal, int ResampleParameter)
        {
            int discreteAmount = signal.GetLength(0);
            int ResampleDiscreteAmount = (discreteAmount - (discreteAmount % ResampleParameter)) / ResampleParameter;
            Int32[] ResampleSignal = new int[ResampleDiscreteAmount];

            for (int i = 0; i < ResampleDiscreteAmount; i++)
            {
                int sum = 0;
                for (int j = i * ResampleParameter; j < (i + 1) * ResampleParameter; j++)
                {
                    sum += signal[i];
                }
                int sum_val = sum;
                ResampleSignal[i] = sum_val;
            }

            return ResampleSignal;
        }

        public virtual dynamic GetComponentSignal(string componentName)
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

        public virtual dynamic ResampleSignal(Int32[] SrcSignal)
        {
            if (this.ResampleParameter == 1)
            {
                return SrcSignal;
            }
            return this.Resampling(SrcSignal, this.ResampleParameter);
        }

        public virtual dynamic ReadSignal(string component = "Z")
        {
            component = component.ToUpper();

            if (this.ComponentsIndex.ContainsKey(component) == false)
            {
                throw new InvalidComponentName("{1} not found", component);
            }

            Int32[] SignalArray = this.GetComponentSignal(component);
            Int32[] ResampleSignalArray = this.ResampleSignal(SignalArray);

            if (this.IsUseAvgValues == false)
            {
                return ResampleSignalArray;
            }

            Int32[] AveragedSignalArray = ResampleSignalArray;
            int avgValue = Convert.ToInt32(Enumerable.Average(ResampleSignalArray));

            for (int i = 0; i < AveragedSignalArray.Length; i++)
            {
                AveragedSignalArray[i] = AveragedSignalArray[i] - avgValue;
            }

            return AveragedSignalArray;
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

