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
        public int channelCount;
        public int frequency;
        public DateTime datetimeStart;
        public double longitude;
        public double latitude;

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
        static public string name
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

                return FormatDuration(days, hours, minutes, seconds);
            }
        }
        static public string FormatDuration(int days, int hours, int minutes, double seconds)
        {
            string hoursFmt = Convert.ToString(hours).PadLeft(2, '0');
            string minutesFmt = Convert.ToString(minutes).PadLeft(2, '0');
            string secondsFmt = string.Format("{0:f3}", seconds).PadLeft(6, '0'); ; //THERE SHOULD BE f'{seconds:.3f}'

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

    public class JsonFileBinary
    {
        public const string Sigma = ".bin";
        public const string Baikal7 = ".00";
        public const string Baikal8 = ".xx";
        string order = "ZXY";

        public string path;
        public int[] signal;
        public int channelCount;
        public int frequency;
        public double latitude;
        public double longitude;
        public DateTime dateTimeStart;


        public JsonFileBinary(
            string pathToSave,             
            int[] signal,                
            double latitude = 0, 
            double longitude = 0, 
            DateTime dateTimeStart = new DateTime(),
            int channelCount = 3,
            int frequency = 1000
            )
        {
            this.path = pathToSave;
            this.signal = signal;
            this.channelCount = channelCount;
            this.frequency = frequency;
            this.latitude = latitude;
            this.longitude = longitude;
            this.dateTimeStart = dateTimeStart;
        }

        public void WriteBinaryFile(string path)
        {
            FileStream filestream = new FileStream(path, FileMode.Create);
            //File binfile = File.Open(fileName, FileMode.Create)
            BinaryWriter binaryWriter = new BinaryWriter(filestream);

            if (FileExtension == ".bin")
            {
                binaryWriter.Seek(12, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt16(this.channelCount)));
                binaryWriter.Seek(24, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt16(this.frequency)));
                binaryWriter.Seek(40, SeekOrigin.Begin);
                string latitudeString = Convert.ToString(this.latitude);
                binaryWriter.Write(latitudeString); //string 8 digits
                binaryWriter.Seek(48, SeekOrigin.Begin);
                string longitudeString = Convert.ToString(this.latitude);
                binaryWriter.Write(longitudeString); //string 8 digits
                binaryWriter.Seek(60, SeekOrigin.Begin);
                string date = Convert.ToString(this.dateTimeStart.Year).Substring(2, 2)
                    + Convert.ToString(this.dateTimeStart.Month).PadLeft(2, '0')
                + Convert.ToString(this.dateTimeStart.Day).PadLeft(2, '0');
                string time = Convert.ToString(this.dateTimeStart.Hour).PadLeft(2, '0')
                    + Convert.ToString(this.dateTimeStart.Minute).PadLeft(2, '0')
                + Convert.ToString(this.dateTimeStart.Second).PadLeft(2, '0');
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt32(date)));
                binaryWriter.Seek(64, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt32(time)));
            }

            else if (FileExtension == ".00")
            {
                binaryWriter.Seek(0, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt16(this.channelCount)));
                binaryWriter.Seek(22, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt16(this.frequency)));
                binaryWriter.Seek(80, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToDouble(this.longitude)));
                binaryWriter.Seek(72, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToDouble(this.latitude)));
                binaryWriter.Seek(104, SeekOrigin.Begin);
                string date = Convert.ToString(this.dateTimeStart.Year).Substring(2, 2)
                    + Convert.ToString(this.dateTimeStart.Month).PadLeft(2, '0')
                + Convert.ToString(this.dateTimeStart.Day).PadLeft(2, '0');
                string time = Convert.ToString(this.dateTimeStart.Hour).PadLeft(2, '0')
                    + Convert.ToString(this.dateTimeStart.Minute).PadLeft(2, '0')
                + Convert.ToString(this.dateTimeStart.Second).PadLeft(2, '0');
                string datetime = date + time;
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt64(datetime)));
            }

            else if (FileExtension == ".xx")
            {
                binaryWriter.Seek(0, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToUInt16(this.channelCount)));
                binaryWriter.Seek(6, SeekOrigin.Begin);
                UInt16 day = Convert.ToUInt16(this.dateTimeStart.Day);
                binaryWriter.Write(BitConverter.GetBytes(day));
                binaryWriter.Seek(8, SeekOrigin.Begin);
                UInt16 month = Convert.ToUInt16(this.dateTimeStart.Month);
                binaryWriter.Write(BitConverter.GetBytes(month));
                binaryWriter.Seek(10, SeekOrigin.Begin);
                UInt16 year = Convert.ToUInt16(this.dateTimeStart.Year);
                binaryWriter.Write(BitConverter.GetBytes(year));
                binaryWriter.Seek(48, SeekOrigin.Begin);                
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToDouble(1) / this.frequency));
                binaryWriter.Seek(56, SeekOrigin.Begin);
                double second = Convert.ToDouble(this.dateTimeStart.Second);
                binaryWriter.Write(BitConverter.GetBytes(second));
                binaryWriter.Seek(72, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToDouble(this.latitude)));
                binaryWriter.Seek(80, SeekOrigin.Begin);
                binaryWriter.Write(BitConverter.GetBytes(Convert.ToDouble(this.longitude)));
            }

            int headerMemorySize = 120 + 72 * channelCount;
            int columnIndex = 1 * 4;
            binaryWriter.Seek(headerMemorySize + columnIndex, 0);
            for (int i = 0; i < signal.Length; i++)
            {
                binaryWriter.Write(BitConverter.GetBytes(signal[i]));
                binaryWriter.Seek(4 * 2, SeekOrigin.Current);
            }            
            binaryWriter.Close();
            filestream.Close();            
        }                
        public string FileExtension
        {
            get
            {
                return System.IO.Path.GetExtension(this.path);
            }
        }               
    }

    public class BinarySeismicFile
    {
        public const string Baikal7Fmt = ".00";
        public const string Baikal8Fmt = ".xx";
        public const string SigmaFmt = ".bin";

        public const int SigmaSecondsOffset = 2;
        public static string ComponentsOrder = "ZXY";

        public string __Path;
        public int __ResampleFrequency;
        public bool __IsUseAvgValues;

        public FileHeader __FileHeader;
        public bool __IsCorrectResampleFrequency;
        public string __UniqueFileName;
        public DateTime __ReadDatetimeStart;
        public DateTime __ReadDatetimeStop;

        string BAIKAL7_FMT = "Baikal7";
        string BAIKAL8_FMT = "Baikal8";
        string SIGMA_FMT = "Sigma";

        string BAIKAL7_EXTENSION = "00";
        string BAIKAL8_EXTENSION = "xx";
        string SIGMA_EXTENSION = "bin";

        public Dictionary<string, string> BinaryFileFormats
        {
            get
            {
                var indexes = new Dictionary<string, string>()
                    {
                        {BAIKAL7_FMT, BAIKAL7_EXTENSION},
                        {BAIKAL8_FMT, BAIKAL8_EXTENSION},
                        {SIGMA_FMT, SIGMA_EXTENSION}
                    };
                return indexes;
            }
        }
        public BinarySeismicFile(string filePath, int resampleFrequency = 0, bool isUseAvgValues = false)
        {
            bool isPathCorrect = IsBinaryFileAtPath(filePath);
            if (isPathCorrect == false) { throw new BadFilePath("Invalid path - {1}", __Path); }
            // full file path
            this.__Path = filePath;

            // header file data
            this.__FileHeader = GetFileHeader;

            // boolean-parameter for subtraction average values
            this.__IsUseAvgValues = isUseAvgValues;

            // resample frequency
            if (IsCorrectResampleFrequency(resampleFrequency) == true)
            {
                __ResampleFrequency = resampleFrequency;
            }
            else { throw new InvalidResampleFrequency(); }

            //this.__unique_file_name = this.__create_unique_file_name()

            // date and time for start signal reading
            __ReadDatetimeStart = new DateTime();
            // date and time for end signal reading
            __ReadDatetimeStop = new DateTime();
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
            bool booling = false;

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
                booling = false;
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
                return booling;
            }
        }
        static public DateTime GetDatetimeStartBaikal7(ulong timeBegin)
        {
            DateTime constDatetime = new DateTime(1980, 1, 1);
            ulong seconds = timeBegin / 256000000;

            return constDatetime.AddSeconds(seconds);
        }
        static public object ReadBaikal7Header(string path)
        {
            int channelCount = BinaryRead(path, "uint16", 1, 0);
            int frequency = BinaryRead(path, "uint16", 1, 22);
            ulong timeBegin = BinaryRead(path, "long", 1, 104);
            double longitude = BinaryRead(path, "double", 1, 80);
            double latitude = BinaryRead(path, "double", 1, 72);
            DateTime datetime = GetDatetimeStartBaikal7(timeBegin);

            return new FileHeader(channelCount, frequency, datetime, longitude, latitude);
        }
        static public object ReadBaikal8Header(string path)
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
        static public object ReadSigmaHeader(string path)
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

            }

            try
            {
                longitude = Math.Round((Convert.ToInt32(longitudeSrc.Substring(0, 3)) + Convert.ToDouble(longitudeSrc.Substring(3, 5), provider) / 60), 2);
                latitude = Math.Round((Convert.ToInt32(latitudeSrc.Substring(0, 2)) + Convert.ToDouble(latitudeSrc.Substring(2, 4), provider) / 60), 2);
            }
            catch (Exception e)
            {

            }

            return new FileHeader(channelCount, frequency, datetimeStart, longitude, latitude);
        }
        public virtual bool IsBinaryFileAtPath(string path)
        {
            if (File.Exists(path) == true)
            {
                string extension = Path.GetExtension(path);

                if (extension == Baikal7Fmt | extension == Baikal8Fmt | extension == SigmaFmt)
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
                return this.__Path;
            }
        }
        private FileHeader FileHeader
        {
            get
            {
                return this.__FileHeader;
            }
        }
        private bool IsUseAvgValues
        {
            get
            {
                return this.__IsUseAvgValues;
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
                if (this.__ResampleFrequency == 0)
                {
                    this.__ResampleFrequency = OriginFrequency;
                }

                return this.__ResampleFrequency;
            }
        }
        public string FileExtension
        {
            get
            {
                return System.IO.Path.GetExtension(GetPath);
            }
        }
        private string UniqueFileName
        {
            get
            {
                return this.__UniqueFileName;
            }
        }
        private string FormatType
        {
            get
            {
                foreach (var file in BinaryFileFormats)
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
                FileInfo file = new FileInfo(this.__Path);
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
                    return OriginDatetimeStart.AddSeconds(SigmaSecondsOffset);
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
        //WARNING! there are a datetime that need to be ...
        private DateTime ReadDatetimeStart
        {
            get
            {
                if (this.__ReadDatetimeStart == new DateTime())
                {
                    this.__ReadDatetimeStart = DatetimeStart;
                }

                return this.__ReadDatetimeStart;
            }
            set
            {
                DateTime datetime = new DateTime();
                double dt1 = datetime.Subtract(DatetimeStart).TotalSeconds;
                double dt2 = DatetimeStop.Subtract(datetime).TotalSeconds;

                if (dt1 >= 0 & dt2 > 0)
                {
                    this.__ReadDatetimeStart = datetime;
                }

                else
                {
                    throw new InvalidDateTimeValue("Invalid start reading datetime");
                }
            }
        }
        //WARNING! there are a datetime that need to be ...
        private DateTime ReadDatetimeStop
        {
            get
            {
                if (this.__ReadDatetimeStop == new DateTime())
                {
                    this.__ReadDatetimeStop = DatetimeStop;
                }

                return this.__ReadDatetimeStop;
            }
            set
            {
                DateTime datetime = new DateTime();
                double dt1 = datetime.Subtract(DatetimeStart).TotalSeconds;
                double dt2 = DatetimeStop.Subtract(datetime).TotalSeconds;

                if (dt1 > 0 & dt2 >= 0)
                {
                    this.__ReadDatetimeStop = datetime;
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
                signalLength = signalLength - (signalLength % ResampleParameter);
                discreetIndex = StartMoment + signalLength;
                return discreetIndex;
            }
        }
        //THERE MAY BE A PROBLEM WITH TYPE ADDUCTION
        private int ResampleParameter
        {
            get
            {
                double division = OriginFrequency / ResampleFrequency;
                return Convert.ToInt32(Math.Floor(division));
            }
        }
        private string RecordType
        {
            get
            {
                return ComponentsOrder;
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
                string extension = System.IO.Path.GetExtension(this.__Path);

                if (extension == Baikal7Fmt)
                {
                    return ReadBaikal7Header(this.__Path);
                }

                else if (extension == Baikal8Fmt)
                {
                    return ReadBaikal8Header(this.__Path);
                }

                else if (extension == SigmaFmt)
                {
                    return ReadSigmaHeader(this.__Path);
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
        //def __create_unique_file_name(self) -> str:
        //return '{}.{}'.format(uuid.uuid4().hex, self.file_extension)
        public dynamic GetComponentSignal(string componentName = "Y")
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

            //signal_array = np.ndarray(signal_size, buffer = mm, dtype = np.int32, offset = offset_size, strides = strides_size).copy()

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

