using System.Globalization;
using System.Runtime.Serialization;
using System.IO.MemoryMappedFiles;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinReader
{
    public class FileHeader
    {
        public readonly int channelCount;
        public readonly int frequency;
        public readonly DateTime datetimeStart;
        public readonly double longitude;
        public readonly double latitude;

        public FileHeader(
            int channelCount,
            int frequency,
            DateTime datetimeStart,
            double longitude,
            double latitude
            )
        {
            this.channelCount = channelCount;
            this.frequency = frequency;
            this.datetimeStart = datetimeStart;
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }

    public class BinaryFileInfo
    {
        public static string path;
        public static string formatType;
        public static int frequency;
        public static DateTime timeStart;
        public static DateTime timeStop;
        public static double longitude;
        public static double latitude;

        public BinaryFileInfo(
            string path,
            string formatType,
            int frequency,
            DateTime timeStart,
            DateTime timeStop,
            double longitude,
            double latitude
            )
        {
            BinaryFileInfo.path = path;
            BinaryFileInfo.formatType = formatType;
            BinaryFileInfo.frequency = frequency;
            BinaryFileInfo.timeStart = timeStart;
            BinaryFileInfo.timeStop = timeStop;
            BinaryFileInfo.longitude = longitude;
            BinaryFileInfo.latitude = latitude;
        }
        static public string Name
        {
            get
            {
                return Path.GetFileName(BinaryFileInfo.path);
            }
        }
        static public object GetShortInfo
        {
            get
            {
                return (path, formatType, frequency, timeStart, timeStop, longitude, latitude);
            }
        }
        static public double DurationInSeconds
        {
            get
            {
                return BinaryFileInfo.timeStop.Subtract(BinaryFileInfo.timeStart).TotalSeconds;
            }
        }

        static public string FormattedDuration
        {
            get
            {
                int secs = Convert.ToInt32(BinaryFileInfo.DurationInSeconds);
                int days = secs / 24 * 3600;
                int hours = (secs - days * 24 * 3600) / 3600;
                int minutes = (secs - days * 24 * 3600 - hours * 3600) / 60;
                double seconds = BinaryFileInfo.DurationInSeconds - days * 24 * 3600 - hours * 3600 - minutes * 60;

                string secondsFmt = string.Format("{0:f3}", seconds).PadLeft(6, '0');
                string minutesFmt = Convert.ToString(minutes).PadLeft(2, '0');
                string hoursFmt = Convert.ToString(hours).PadLeft(2, '0');

                if (days != 0)
                {
                    return Convert.ToString(days) + " days " + hoursFmt + ":" + minutesFmt + ":" + secondsFmt;
                }
                else
                {
                    return hoursFmt + ":" + minutesFmt + ":" + secondsFmt;
                }                
            }
        }       
    }

    public class Constants
    {
        public static string ComponentsOrder = "ZXY";
        public const int SigmaSecondsOffset = 2;

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
                var indexes = new Dictionary<string, string>()
                    {
                        {Constants.Baikal7Fmt, Constants.Baikal7Extension},
                        {Constants.Baikal8Fmt, Constants.Baikal8Extension},
                        {Constants.SigmaFmt, Constants.SigmaExtension}
                    };
                return indexes;
            }
        }
    }

    public class BinarySeismicFile
    {            
        public readonly string _Path;
        public readonly bool _IsUseAvgValues;
        public readonly FileHeader _FileHeader;

        public int _ResampleFrequency;
        public bool _IsCorrectResampleFrequency;
        public string _UniqueFileName;
        public DateTime _ReadDatetimeStart;
        public DateTime _ReadDatetimeStop;
        
        public BinarySeismicFile(string filePath, int resampleFrequency = 0, bool isUseAvgValues = false)
        {
            bool isPathCorrect = IsBinaryFileAtPath(filePath);
            if (isPathCorrect == false) { throw new BadFilePath("Invalid path - {1}", _Path); }            
            this._Path = filePath;
            this._FileHeader = GetFileHeader;            
            this._IsUseAvgValues = isUseAvgValues;
            
            if (IsCorrectResampleFrequency(resampleFrequency) == true)
            {
                _ResampleFrequency = resampleFrequency;
            }
            else { throw new InvalidResampleFrequency(); }
           
            _ReadDatetimeStart = new DateTime();
            _ReadDatetimeStop = new DateTime();
        }

        static public dynamic BinaryRead(string path, string type, int count, int SkippingBytes = 0)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            int size = (int)stream.Length;
            byte[] data = new byte[size];
            stream.Read(data, 0, 336);
            var memoryStream = new MemoryStream(data, 0, 336);
            memoryStream.Seek(SkippingBytes, SeekOrigin.Begin);
            var reader = new BinaryReader(memoryStream);

            int uInt16 = 0;
            uint uInt32 = 0;
            double dabl = 0;
            ulong uInt64 = 0;
            string stroke = "";

            if (type == "uint16")
            {
                uInt16 = reader.ReadUInt16();
            }
            else if (type == "uint32")
            {
                uInt32 = reader.ReadUInt32();
            }
            else if (type == "double")
            {
                dabl = reader.ReadDouble();
            }
            else if (type == "long")
            {
                uInt64 = reader.ReadUInt64();
            }
            else if (type == "string")
            {
                stroke = new string(reader.ReadChars(count));
            }
            else
            {
                return null;
            }

            stream.Close();
            memoryStream.Close();
            reader.Close();

            if (type == "uint16")
            {
                return uInt16;
            }
            else if (type == "uint32")
            {
                return uInt32;
            }
            else if (type == "double")
            {
                return dabl;
            }
            else if (type == "long")
            {
                return uInt64;
            }
            else if (type == "string")
            {
                return stroke;
            }
            else
            {
                return null;
            }
        }
        static public DateTime GetDatetimeStartBaikal7(ulong timeBegin)
        {
            DateTime constDatetime = new DateTime(1980, 1, 1);
            ulong seconds = timeBegin / 256000000;

            return constDatetime.AddSeconds(seconds);
        }
        static public FileHeader ReadBaikal7Header(string path)
        {
            int channelCount = BinaryRead(path, "uint16", 1, 0);
            int frequency = BinaryRead(path, "uint16", 1, 22);
            ulong timeBegin = BinaryRead(path, "long", 1, 104);
            double longitude = BinaryRead(path, "double", 1, 80);
            double latitude = BinaryRead(path, "double", 1, 72);
            DateTime datetime = GetDatetimeStartBaikal7(timeBegin);

            return new FileHeader(channelCount, frequency, datetime, longitude, latitude);
        }
        static public FileHeader ReadBaikal8Header(string path)
        {
            int channelCount = BinaryRead(path, "uint16", 1, 0);
            int day = BinaryRead(path, "uint16", 1, 6);
            int month = BinaryRead(path, "uint16", 1, 8);
            int year = BinaryRead(path, "uint16", 1, 10);
            double dt = BinaryRead(path, "double", 1, 48);
            double seconds = BinaryRead(path, "double", 1, 56);
            double latitude = BinaryRead(path, "double", 1, 72);
            double longitude = BinaryRead(path, "double", 1, 80);
            DateTime datetimeStart = new DateTime(year, month, day, 0, 0, 1).AddSeconds(seconds);
            int frequency = Convert.ToInt16(1 / dt);

            return new FileHeader(channelCount, frequency, datetimeStart, longitude, latitude);
        }
        static public FileHeader ReadSigmaHeader(string path)
        {
            DateTime datetimeStart = new DateTime(1999, 1, 1);
            double longitude = 0;
            double latitude = 0;
            int channelCount = BinaryRead(path, "uint16", 1, 12);
            int frequency = BinaryRead(path, "uint16", 1, 24);
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
                datetimeStart = new DateTime(year, month, day, hours, minutes, seconds);
            }
            catch (Exception e)
            {
                throw new InvalidDateTimeValue("Invalid start reading datetime");
            }

            try
            {
                longitude = Math.Round((Convert.ToInt32(longitudeSrc.Substring(0, 3)) + Convert.ToDouble(longitudeSrc.Substring(3, 5), provider) / 60), 2);
                latitude = Math.Round((Convert.ToInt32(latitudeSrc.Substring(0, 2)) + Convert.ToDouble(latitudeSrc.Substring(2, 4), provider) / 60), 2);
            }
            catch (Exception e)
            {
                throw new InvalidCoordinates("Invalid coordinates");
            }

            return new FileHeader(channelCount, frequency, datetimeStart, longitude, latitude);
        }
        public virtual bool IsBinaryFileAtPath(string path)
        {
            if (File.Exists(path) == true)
            {
                string extension = Path.GetExtension(path).Replace(".", "");

                if (extension == Constants.Baikal7Extension | extension == Constants.Baikal8Fmt | extension == Constants.SigmaFmt)
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
        private FileHeader FileHeader
        {
            get
            {
                return this._FileHeader;
            }
        }
        private bool IsUseAvgValues
        {
            get
            {
                return this._IsUseAvgValues;
            }
        }
        private int OriginFrequency
        {
            get
            {
                return FileHeader.frequency;
            }
        }
        private int ResampleFrequency
        {
            get
            {
                if (this._ResampleFrequency == 0)
                {
                    this._ResampleFrequency = OriginFrequency;
                }

                return this._ResampleFrequency;
            }
        }
        public string FileExtension
        {
            get
            {
                return Path.GetExtension(GetPath);
            }
        }
        private string UniqueFileName
        {
            get
            {
                return this._UniqueFileName;
            }
        }
        private string FormatType
        {
            get
            {
                foreach (var file in Constants.BinaryFileFormats)
                {
                    if (file.Value == FileExtension)
                    {
                        return file.Key;
                    }
                }

                return null;
            }
        }
        private DateTime OriginDatetimeStart
        {
            get
            {
                return FileHeader.datetimeStart;
            }
        }
        private int ChannelsCount
        {
            get
            {
                return FileHeader.channelCount;
            }
        }
        private int HeaderMemorySize
        {
            get
            {
                int channelCount = ChannelsCount;

                return 120 + 72 * channelCount;
            }
        }
        private int DiscreteAmount
        {
            get
            {
                FileInfo file = new FileInfo(this._Path);
                long fileSize = file.Length;
                int discreteAmount = Convert.ToInt32((fileSize - HeaderMemorySize) / (FileHeader.channelCount * 4));

                return discreteAmount;
            }
        }
        private double SecondsDuration
        {
            get
            {
                int discreteCount = DiscreteAmount;
                int freq = OriginFrequency;
                int accuracy = Convert.ToInt32(Math.Log10(freq));
                double deltaSeconds = Math.Round(Convert.ToDouble(discreteCount / freq), accuracy);

                return deltaSeconds;
            }
        }
        public DateTime OriginDatetimeStop
        {
            get
            {
                return OriginDatetimeStart.AddSeconds(SecondsDuration);
            }
        }
        public DateTime DatetimeStart
        {
            get
            {
                if (FormatType == "SIGMA_FMT")
                {
                    return OriginDatetimeStart.AddSeconds(Constants.SigmaSecondsOffset);
                }

                else
                {
                    return OriginDatetimeStart.AddSeconds(0);
                }
            }
        }
        public DateTime DatetimeStop
        {
            get
            {
                return DatetimeStart.AddSeconds(SecondsDuration);
            }
        }
        public double Longitude
        {
            get
            {
                return Math.Round(FileHeader.longitude, 6);
            }
        }
        public double Latitude
        {
            get
            {
                return Math.Round(FileHeader.latitude, 6);
            }
        }
        
        private DateTime ReadDatetimeStart
        {
            get
            {
                if (this._ReadDatetimeStart == new DateTime())
                {
                    this._ReadDatetimeStart = DatetimeStart;
                }

                return this._ReadDatetimeStart;
            }
            set
            {
                DateTime datetime = new DateTime();
                double dt1 = datetime.Subtract(DatetimeStart).TotalSeconds;
                double dt2 = DatetimeStop.Subtract(datetime).TotalSeconds;

                if (dt1 >= 0 & dt2 > 0)
                {
                    this._ReadDatetimeStart = datetime;
                }

                else
                {
                    throw new InvalidDateTimeValue("Invalid start reading datetime");
                }
            }
        }
        
        private DateTime ReadDatetimeStop
        {
            get
            {
                if (this._ReadDatetimeStop == new DateTime())
                {
                    this._ReadDatetimeStop = DatetimeStop;
                }

                return this._ReadDatetimeStop;
            }
            set
            {
                DateTime datetime = new DateTime();
                double dt1 = datetime.Subtract(DatetimeStart).TotalSeconds;
                double dt2 = DatetimeStop.Subtract(datetime).TotalSeconds;

                if (dt1 > 0 & dt2 >= 0)
                {
                    this._ReadDatetimeStop = datetime;
                }

                else
                {
                    throw new InvalidDateTimeValue("Invalid stop reading datetime");
                }
            }
        }
        private int StartMoment
        {
            get
            {
                TimeSpan dtDiff = ReadDatetimeStart.Subtract(DatetimeStart);
                double dtSeconds = dtDiff.TotalSeconds;
                return Convert.ToInt32(Math.Round(dtSeconds * OriginFrequency));
            }
        }
        private int EndMoment
        {
            get
            {
                double dt = ReadDatetimeStop.Subtract(DatetimeStart).TotalSeconds;
                int discreetIndex = Convert.ToInt32(Math.Round(dt * OriginFrequency));
                int signalLength = discreetIndex - StartMoment;
                signalLength -= (signalLength % ResampleParameter);
                discreetIndex = StartMoment + signalLength;
                return discreetIndex;
            }
        }        
        private int ResampleParameter
        {
            get
            {
                double division = Convert.ToDouble(OriginFrequency / ResampleFrequency);
                return Convert.ToInt32(Math.Floor(division));
            }
        }
        private string RecordType
        {
            get
            {
                return Constants.ComponentsOrder;
            }
        }
        public Dictionary<string, int> ComponentsIndex
        {
            get
            {
                var indexes = new Dictionary<string, int>()
                    {
                        {RecordType.Substring(0,1), 1},
                        {RecordType.Substring(1,1), 2},
                        {RecordType.Substring(2,1), 3}
                    };

                return indexes;
            }
        }
        public object ShortFileInfo
        {
            get
            {
                return BinaryFileInfo.GetShortInfo;
            }
        }
        public dynamic GetFileHeader
        {
            get
            {
                string extension = Path.GetExtension(this._Path).Replace(".", "");

                if (extension == Constants.Baikal7Extension)
                {
                    return ReadBaikal7Header(this._Path);
                }

                else if (extension == Constants.Baikal8Fmt)
                {
                    return ReadBaikal8Header(this._Path);
                }

                else if (extension == Constants.SigmaFmt)
                {
                    return ReadSigmaHeader(this._Path);
                }

                else
                    return null;
            }
        }
        public bool IsCorrectResampleFrequency(int value)
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
                if (OriginFrequency % value == 0)
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }
        }
        public dynamic Resampling(Int32[] signal, int ResampleParameter)
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
        public dynamic GetComponentSignal(string componentName)
        {
            int columnIndex;

            if (ChannelsCount == 3)
            {
                ComponentsIndex.TryGetValue(componentName, out columnIndex);
            }

            else
            {
                ComponentsIndex.TryGetValue(componentName, out columnIndex);
                columnIndex = columnIndex + 3;
            }

            int skipDataSize = 4 * ChannelsCount * StartMoment;
            int offsetSize = HeaderMemorySize + skipDataSize + columnIndex * 4;
            int stridesSize = 4 * ChannelsCount;
            int signalSize = EndMoment - StartMoment;            

            FileStream fileStream = new FileStream(GetPath, FileMode.Open, FileAccess.Read);
            MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(
                fileStream: fileStream,
                mapName: "mn",
                capacity: 0,
                access: MemoryMappedFileAccess.Read,
                inheritability: HandleInheritability.None,
                leaveOpen: false
                );

            byte[] byteArray = new byte[signalSize];
            byte[] byteArrayClip = new byte[signalSize / stridesSize];
            Int32[] intArray = new int[byteArrayClip.Length / 4];

            if (offsetSize < 0)
            {
                return intArray;
            }

            MemoryMappedViewStream memoryMappedViewStream = memoryMappedFile.CreateViewStream(offsetSize, signalSize, MemoryMappedFileAccess.Read);
            memoryMappedViewStream.Read(byteArray, 0, signalSize);

            for (int i = 0; i < (signalSize / stridesSize / 4); i++)
            {
                byteArrayClip[i * 4] = byteArray[i * stridesSize];
                byteArrayClip[i * 4 + 1] = byteArray[i * stridesSize + 1];
                byteArrayClip[i * 4 + 2] = byteArray[i * stridesSize + 2];
                byteArrayClip[i * 4 + 3] = byteArray[i * stridesSize + 3];
            }

            for (int i = 0; i < intArray.Length / 4; i++)
            {
                Int32 currentSignal = BitConverter.ToInt32(byteArrayClip, i * 4);
                intArray[i] = currentSignal;
            }

            fileStream.Close();
            memoryMappedViewStream.Close();
            memoryMappedFile.Dispose();

            return intArray;
        }
        public dynamic ResampleSignal(Int32[] SrcSignal)
        {
            if (ResampleParameter == 1)
                return SrcSignal;

            return Resampling(SrcSignal, ResampleParameter);
        }
        public dynamic ReadSignal(string component = "Z")
        {
            component = component.ToUpper();

            if (ComponentsIndex.ContainsKey(component) == false)
            {
                throw new InvalidComponentName("{1} not found", component);
            }

            Int32[] SignalArray = GetComponentSignal(component);
            Int32[] ResampleSignalArray = ResampleSignal(SignalArray);

            if (IsUseAvgValues == false)
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
    internal class BadFilePath : Exception
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

