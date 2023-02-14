using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using BinReader;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinaryReaderLibraryTest
{
    [TestClass]
    public class TestLibrary
    {
        [TestMethod]
        [DataRow(555)]
        [DataRow(1337)]
        [DataRow(50000)]
        [DataRow(115851)]
        [DataRow(82485484)]
        public void testGetDatetimeStartBaikal7(int timeBegin)
        {
            timeBegin = timeBegin / 256000000;
            DateTime expected = new DateTime(1980, 1, 1).AddSeconds(timeBegin);
            var mock = new Mock<FileHeader>("123.00") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);

            var actual = mock.Object.GetDatetimeStartBaikal7((ulong)timeBegin);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void testReadBaikal7Header()
        {
            var mock = new Mock<FileHeader>("123.00") { CallBase = true };
            //mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            //.Returns((int)1)
            //.Returns((int)2)
            //.Returns((ulong)0)
            //.Returns((double)4)
            //.Returns((double)5);

            mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1)
            .Returns(2)
            .Returns((ulong)0)
            .Returns(4)
            .Returns(5);

            mock.Object.ReadBaikal7Header("123.00");

            Assert.AreEqual(1, mock.Object.channelCount);
            Assert.AreEqual(2, mock.Object.frequency);
            Assert.AreEqual(Constants.Baikal7BaseDateTime, mock.Object.datetimeStart);
            Assert.AreEqual(4.123123, mock.Object.coordinate.longitude);
            Assert.AreEqual(5.123123, mock.Object.coordinate.latitude);
        }

        [TestMethod]
        public void testReadBaikal8Header()
        {
            //var expected = new FileHeader(1, 1000, new DateTime(4, 3, 2, 0, 0 , 6), 8, 7);

            var mock = new Mock<BinarySeismicFile>("", 0, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(0);
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStart).Returns(new DateTime());
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStop).Returns(new DateTime());
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            //mock.As<IBinarySeismicFile>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.As<IBinarySeismicFile>().Setup(p => p.GetFileHeader).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            //.Returns(1) //channel
            //.Returns(2) //day
            //.Returns(3) //month
            //.Returns(4) //year
            //.Returns(0.001) //dt
            //.Returns(6) //seconds
            //.Returns(7) // latitude
            //.Returns(8); //longitude

            //var actual = mock.Object.ReadBaikal8Header("");

            //Assert.AreEqual(expected.longitude, actual.longitude);
            //Assert.AreEqual(expected.latitude, actual.latitude);
            //Assert.AreEqual(expected.channelCount, actual.channelCount);
            //Assert.AreEqual(expected.frequency, actual.frequency);
            //Assert.AreEqual(expected.datetimeStart, actual.datetimeStart);
        }

        [TestMethod]
        public void testReadSigmaHeader()
        {
            //var expected = new FileHeader(1, 2, new DateTime(4, 3, 2, 0, 0, 6), 8, 7);

            var mock = new Mock<BinarySeismicFile>("", 0, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(0);
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStart).Returns(new DateTime());
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStop).Returns(new DateTime());
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            //mock.As<IBinarySeismicFile>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.As<IBinarySeismicFile>().Setup(p => p.GetFileHeader).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            //.Returns(1) //channel
            //.Returns(2) //freq
            //.Returns("1234") //latit
            //.Returns("11234")
            //.Returns(2)
            //.Returns(2)
            //.Returns(2)
            //.Returns(2)
            //.Returns(2)
            //.Returns(2); //longit

            //var actual = mock.Object.ReadSigmaHeader("");

            //Assert.AreEqual(expected.longitude, actual.longitude);
            //Assert.AreEqual(expected.latitude, actual.latitude);
            //Assert.AreEqual(expected.channelCount, actual.channelCount);
            //Assert.AreEqual(expected.frequency, actual.frequency);
            //Assert.AreEqual(expected.datetimeStart, actual.datetimeStart);
        }

        [DataRow("gsdfgdf/bala.bol", "bala.bol")]
        [DataRow("gsd?/fgd/f/12.l", "12.l")]
        [DataRow("y543-0/g5/f", "f")]
        [DataRow("6hrte/3g/", "")]
        [TestMethod]
        public void testName(string path, string expected)
        {            
            var mock = new Mock<BinaryFileInfo>(path, "",0,new DateTimeInterval(new DateTime(),new DateTime()), new Coordinate(0,0));
            var actual = mock.Object.Name;
            Assert.AreEqual(expected, actual);
        }

        [DataRow(0)]
        [DataRow(12321)]
        [DataRow(734)]
        [DataRow(74567456)]
        [DataRow(2147483647)]
        [TestMethod]
        public void testDurationInSeconds(int seconds)
        {
            DateTime startDate = new DateTime();
            DateTime stopDate = startDate.AddSeconds(seconds);
            DateTimeInterval interval = new DateTimeInterval(startDate, stopDate);
            var mock = new Mock<BinaryFileInfo>("", "", 0, interval, new Coordinate(0, 0)) {CallBase = true};            
            var actual = mock.Object.DurationInSeconds;
            Assert.AreEqual(seconds, actual);
        }

        [TestMethod]
        [DataRow(555, "00:09:15,000")]
        [DataRow(1337, "00:22:17,000")]
        [DataRow(50000, "13:53:20,000")]
        [DataRow(115851, "1 days 08:10:51,000")]
        [DataRow(82485484, "954 days 16:38:04,000")]
        public void testFormattedDuration(int secondsAll, string expected)
        {
            var mock = new Mock<BinaryFileInfo>("", "", 0, new DateTimeInterval(new DateTime(), new DateTime()), new Coordinate(0, 0)) { CallBase = true };
            mock.As<IBinaryFileInfo>().Setup(p => p.DurationInSeconds).Returns(secondsAll);
            string actual = mock.Object.FormattedDuration;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("gdf.00", true)]
        [DataRow("gdf.xx", true)]
        [DataRow("gdf.00", true)]
        [DataRow("gdf.oo", false)]
        [DataRow("gdf.hgf", false)]
        public void testIsBinaryFileAtPath(string path, bool result)
        {
            File.Create(@"C:\Windows\Temp\"+path, 4096, FileOptions.DeleteOnClose);            
            var mock = new Mock<BinarySeismicFile>("123.10", 1, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.10")).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            bool actual = mock.Object.IsBinaryFileAtPath(@"C:\Windows\Temp\" + path);            
            Assert.AreEqual(result, actual);
        }

        [DataRow("gdf.6x")]
        [DataRow("fds/fsd/1fsd.sa")]
        [TestMethod]
        public void testGetPath(string path)
        {            
            var mock = new Mock<BinarySeismicFile>(path, 1, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(path)).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(123);
            string actual = mock.Object.GetPath;
            Assert.AreEqual(path, actual);
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testIsUseAvgValues(bool avg)
        {
            var mock = new Mock<BinarySeismicFile>("123.1", 1, avg) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.1")).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(123);
            bool actual = mock.Object.IsUseAvgValues;
            Assert.AreEqual(avg, actual);
        }

        [DataRow(1000)]
        [DataRow(11)]
        [TestMethod]
        public void testOriginFrequency(int freq)
        {
            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));

            var mockf = new Mock<FileHeader>("123.020") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
            mockf.Object.frequency = freq;

            mock.Object._FileHeader = mockf.Object;

            int actual = mock.Object.OriginFrequency;


            int expected = freq;

            Assert.AreEqual(expected, actual);
        }

        [DataRow(123)]
        [DataRow(321)]
        [TestMethod]
        public void testResampleFrequency(int freq)
        {
            var mock = new Mock<BinarySeismicFile>("123.1", freq, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.1")).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(123);
            int actual = mock.Object.ResampleFrequency;
            Assert.AreEqual(freq, actual);
        }

        [DataRow("gdf/sdf/text.tst", "tst")]
        [DataRow("gdf.qwe", "qwe")]
        [TestMethod]
        public void testFileExtension(string path, string ext)
        {
            var mock = new Mock<BinarySeismicFile>("123.1", 1, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.1")).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.GetPath).Returns(path);
            string actual = mock.Object.FileExtension;
            Assert.AreEqual(ext, actual);
        }

        [TestMethod]
        [DataRow("00", "Baikal7")]
        [DataRow("xx", "Baikal8")]
        [DataRow("bin", "Sigma")]
        [DataRow("oo", null)]
        [DataRow("txt", null)]
        public void testFormatType(string ext, string result)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.FileExtension).Returns(ext); 
            string actual = mock.Object.FormatType;     
            Assert.AreEqual(result, actual);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testHeaderMemorySize(int chcount)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.ChannelsCount).Returns(chcount);
            int actual = mock.Object.HeaderMemorySize;
            int result = 120 + 72 * chcount;
            Assert.AreEqual(result, actual);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testChannelsCount(int chcount)
        {
            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            var mockf = new Mock<FileHeader>("123.020") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
            mockf.Object.channelCount = chcount;
            mock.Object._FileHeader = mockf.Object;
            int actual = mock.Object.ChannelsCount;
            int expected = chcount;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void testDiscreteAmount()
        {
            Random rnd = new Random();

            using (var stream = File.Open("D:/exampleFile.123", FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < rnd.Next(80000, 10000000); i++)
                    { writer.Write(BitConverter.GetBytes((Int32)2)); ; }
                }
            }

            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));

            var mockf = new Mock<FileHeader>("123.020") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
            mockf.Object.channelCount = 3;

            mock.Object._FileHeader = mockf.Object;
            
            int actual = mock.Object.DiscreteAmount;
                                             
            FileInfo file = new FileInfo("D:/exampleFile.123");
            long fileSize = file.Length;
            int expected = (int)((fileSize - mock.Object.HeaderMemorySize) / (3 * sizeof(int)));

            file.Delete();

            Assert.AreEqual(expected, actual);
        }

        [DataRow(3, 10)]
        [DataRow(6, 10)]
        [TestMethod]
        public void testSecondsDuration(int discreteCount, int freq)
        {
            int acc = Convert.ToInt32(Math.Log10(freq));

            double btw = Convert.ToDouble(discreteCount) / freq;

            double expected = Math.Round(btw, acc);

            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(discreteCount);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(freq);

            double actual = mock.Object.SecondsDuration;
           
            Assert.AreEqual(expected, actual);
        }

        [DataRow(10432,2314)]
        [DataRow(-14123,-54431)]
        [TestMethod]
        public void testCoordinate(double longi, double lat)
        {
            Coordinate expected = new Coordinate(longi, lat);

            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));

            var mockf = new Mock<FileHeader>("123.020") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
            mockf.Object.coordinate = expected;

            mock.Object._FileHeader = mockf.Object;

            var actual = mock.Object.Coordinate;            

            Assert.AreEqual(expected.latitude, actual.latitude);
            Assert.AreEqual(expected.longitude, actual.longitude);
        }

        [DataRow(3)]
        [DataRow(3432)]
        [DataRow(498656516)]
        [TestMethod]
        public void testOriginDateTimeInterval(int second)
        {
            DateTime def = new DateTime();
            DateTimeInterval expinterval = new DateTimeInterval(def, def.AddSeconds(second));

            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);

            var mockf = new Mock<FileHeader>("123.020") { CallBase = true };
            mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
            mockf.Object.datetimeStart = def;            

            mock.Object._FileHeader = mockf.Object;

            var actual = mock.Object.OriginDateTimeInterval;

            Assert.AreEqual(expinterval.start, actual.start);
            Assert.AreEqual(expinterval.stop, actual.stop);
        }

        [DataRow(3, true)]
        [DataRow(3432, false)]
        [DataRow(498656516, true)]
        [TestMethod]
        public void testRecordDateTimeInterval(int second, bool isSigma)
        {
            DateTime def = new DateTime();
            DateTimeInterval interval = new DateTimeInterval(def, def.AddSeconds(second));
            DateTimeInterval expinterval = new DateTimeInterval(def, def.AddSeconds(second));
            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginDateTimeInterval).Returns(interval);
            if (isSigma == true)
            {
                expinterval.start.AddSeconds(2);
                expinterval.stop.AddSeconds(Constants.SigmaSecondsOffset + second);
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.SigmaFmt);                
            }
            else
            {
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.Baikal7Fmt);
                expinterval.start.AddSeconds(0);
                expinterval.stop.AddSeconds(second);
            }                        

            var actual = mock.Object.RecordDateTimeInterval;

            Assert.AreEqual(expinterval.start, actual.start);
            Assert.AreEqual(expinterval.stop, actual.stop);
        }

        [DataRow(31, 1000)]
        [DataRow(4234, 434)]
        [TestMethod]
        public void testStartMoment(int sec, int freq)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.ReadDateTimeInterval).Returns(new DateTimeInterval(new DateTime().AddSeconds(sec), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));            
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(freq);

            double actual = mock.Object.StartMoment;

            Assert.AreEqual(freq * sec, actual);
        }

        [DataRow(1000, 1000, 1)]
        [DataRow(123, 1234, 0)]
        [DataRow(4234, 434, 9)]
        [TestMethod]
        public void testResampleParameter(int origin, int resample, int res)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(origin);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleFrequency).Returns(resample);            

            double actual = mock.Object.ResampleParameter;

            Assert.AreEqual(res, actual);
        }

        [TestMethod]
        public void testComponentsIndex()
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(1);

            var actual = mock.Object.ComponentsIndex;

            var componentsIndexes = new Dictionary<string, int>();;
            componentsIndexes.Add("Z", 0);
            componentsIndexes.Add("X", 1);
            componentsIndexes.Add("Y", 2);

            Assert.AreEqual(componentsIndexes["Z"], actual["Z"]);
            Assert.AreEqual(componentsIndexes["X"], actual["X"]);
            Assert.AreEqual(componentsIndexes["Y"], actual["Y"]);
        }

        [TestMethod]
        public void testShortFileInfo()
        {                  
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.GetPath).Returns("123");
            mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns("type");
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1000);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime().AddDays(1)));
            mock.As<IBinarySeismicFile>().Setup(p => p.Coordinate).Returns(new Coordinate(1, 2));

            var actual = mock.Object.ShortFileInfo;

            BinaryFileInfo result = new BinaryFileInfo("123", "type", 1000, new DateTimeInterval(new DateTime(), new DateTime().AddDays(1)), new Coordinate(0, 0));

            Assert.AreEqual(result.path, actual.path);
            Assert.AreEqual(result.formatType, actual.formatType);
            Assert.AreEqual(result.frequency, actual.frequency);
            Assert.AreEqual(result.datetimeInterval.stop, actual.datetimeInterval.stop);
            Assert.AreEqual(result.datetimeInterval.start, actual.datetimeInterval.start);
            Assert.AreEqual(result.coordinate.longitude, actual.coordinate.longitude);
            Assert.AreEqual(result.coordinate.latitude, actual.coordinate.latitude);
        }

        [DataRow(100, -100, false)]
        [TestMethod]
        public void testIsCorrectResampleFrequency(int origin, int resample, bool result)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(origin);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleFrequency).Returns(resample);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(0);

            bool actual = mock.Object.IsCorrectResampleFrequency(resample);

            Assert.AreEqual(result, actual);
        }

        [TestMethod]        
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", "D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testPathMethod(string path, string expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            string actual = binFile._Path;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", true)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", true)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", true)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN01265409-14239.bin", false)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN01265409-19.xx", false)]
        public void testIsBinaryFileAtPathMethod(string path, bool expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00");
            if (expected == false)
            {
                Assert.ThrowsException<BinReader.BadFilePath>(() => binFile = new BinarySeismicFile(path));
                Assert.AreEqual(expected, binFile.IsBinaryFileAtPath(path));
            }
            else
            {
                binFile = new BinarySeismicFile(path);
                bool actual = binFile.IsBinaryFileAtPath(path);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testFileHeaderMethod(string path)
        {
            DateTime startDateTime = new DateTime();
            //FileHeader expectedHeader;

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                startDateTime = new DateTime(2022, 9, 19, 7, 48, 7);
                //expectedHeader = new FileHeader(3, 1000, startDateTime, 0, 57.3188800001);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                startDateTime = new DateTime(2022, 9, 19, 8, 53, 54);
                //expectedHeader = new FileHeader(3, 1000, startDateTime, 79.31958770751953, 66.75099182128906);
            }
            else
            {
                startDateTime = new DateTime(2022, 9, 19, 8, 38, 43);
                //expectedHeader = new FileHeader(3, 1000, startDateTime, 79.33, 66.74);
            }

            BinarySeismicFile binFile = new BinarySeismicFile(path);
            FileHeader actual = binFile._FileHeader;
            //Assert.AreEqual(expectedHeader.channelCount, actual.channelCount);
            //Assert.AreEqual(expectedHeader.frequency, actual.frequency);
            //Assert.AreEqual(expectedHeader.datetimeStart, actual.datetimeStart);
            //Assert.AreEqual(expectedHeader.longitude, actual.longitude);
            //Assert.AreEqual(expectedHeader.latitude, actual.latitude);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", false)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", false)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", false)]
        public void testIsUseAverageValuesMethod(string path, bool expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            bool actual = binFile.IsUseAvgValues;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 1000)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 1000)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 1000)]
        public void testOriginFrequencyMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.OriginFrequency;
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 1000)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 1000)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 1000)]
        public void testResampleFrequencyMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.ResampleFrequency;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", "00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", "xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", "bin")]
        public void testFileExtensionMethod(string path, string expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            string actual = binFile.FileExtension;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", "Baikal7")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", "Baikal8")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", "Sigma")]
        public void testFormatTypeMethod(string path, string expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            string actual = binFile.FormatType;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testOriginDateTimeStartMethod(string path)
        {
            DateTime originDateTimeStart = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 7, 48, 07);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 53, 54);
            }
            else
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 38, 43);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //DateTime actual = binFile.OriginDatetimeStart;
            //Assert.AreEqual(originDateTimeStart, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 3)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 3)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 3)]
        public void testChannelsCountMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.ChannelsCount;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 336)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 336)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 336)]
        public void testHeaderMemorySizeMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.HeaderMemorySize;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 58013000)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 54366000)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 166828010)]
        public void testDiscreteAmountMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.DiscreteAmount;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 58013.0)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 54366.0)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 166828.01)]
        public void testSecondsDurationMethod(string path, double expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            double actual = binFile.SecondsDuration;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testOriginDateTimeStopMethod(string path)
        {
            DateTime originDateTimeStart = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 23, 55, 0);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStart = new DateTime(2022, 9, 20, 0, 0, 0);
            }
            else
            {
                originDateTimeStart = new DateTime(2022, 9, 21, 6, 59, 11, 10);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //DateTime actual = binFile.OriginDatetimeStop;
            //Assert.AreEqual(originDateTimeStart, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testDateTimeStartMethod(string path)
        {
            DateTime originDateTimeStart = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 07, 48, 7);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 53, 54);
            }
            else
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 38, 45);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //DateTime actual = binFile.DatetimeStart;
            /// Assert.AreEqual(originDateTimeStart, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testDateTimeStopMethod(string path)
        {
            DateTime originDateTimeStart = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 23, 55, 0);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStart = new DateTime(2022, 9, 20, 0, 0, 0);
            }
            else
            {
                originDateTimeStart = new DateTime(2022, 9, 21, 6, 59, 13, 10);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //DateTime actual = binFile.DatetimeStop;
            //Assert.AreEqual(originDateTimeStart, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 0)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 79.319588)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 79.33)]
        public void testLongitudeMethod(string path, double expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //double actual = binFile.Longitude;
            //Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 57.31888)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 66.750992)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 66.74)]
        public void testLatitudeMethod(string path, double expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //double actual = binFile.Latitude;
            //Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testReadDateTimeStartMethod(string path)
        {
            DateTime originDateTimeStart = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 07, 48, 7);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 53, 54);
            }
            else
            {
                originDateTimeStart = new DateTime(2022, 9, 19, 8, 38, 45);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            DateTime actual = binFile.ReadDateTimeInterval.start;
            Assert.AreEqual(originDateTimeStart, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testReaedDateTimeStopMethod(string path)
        {
            DateTime originDateTimeStop = new DateTime();

            if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            {
                originDateTimeStop = new DateTime(2022, 9, 19, 23, 55, 0);
            }
            else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            {
                originDateTimeStop = new DateTime(2022, 9, 20, 0, 0, 0);
            }
            else
            {
                originDateTimeStop = new DateTime(2022, 9, 21, 6, 59, 13, 10);
            }
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            //DateTime actual = binFile.DatetimeStop;
            //Assert.AreEqual(originDateTimeStop, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 0)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 0)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 0)]
        public void testStartMomentMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.StartMoment;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 58013000)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 54366000)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 166828010)]
        public void testEndMomentMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.EndMoment;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", 1)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", 1)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", 1)]
        public void testResampleParameterMethod(string path, int expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            int actual = binFile.ResampleParameter;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", "ZXY")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", "ZXY")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", "ZXY")]
        public void testRecordTypeMethod(string path, string expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            string actual = binFile.RecordType;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testComponentIndexMethod(string path)
        {
            Dictionary<string, int> real = new Dictionary<string, int>()
                {
                    { "Z", 0},
                    { "X", 1},
                    { "Y", 2}
                };
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            var actual = binFile.ComponentsIndex;
            Assert.AreEqual(real["Z"], actual["Z"]);
            Assert.AreEqual(real["X"], actual["X"]);
            Assert.AreEqual(real["Y"], actual["Y"]);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin")]
        public void testShortFileInfoMethod(string path)
        {
            BinaryFileInfo expectedInfo;

            //if (path == "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00")
            //{
            //    expectedInfo = new BinaryFileInfo(
            //        "D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00",
            //        "Baikal7",
            //        1000,
            //        new DateTime(2022, 9, 19, 7, 48, 7),
            //        new DateTime(2022, 9, 19, 23, 55, 0),
            //        0,
            //        57.31888);
            //}
            //else if (path == "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx")
            //{
            //    expectedInfo = new BinaryFileInfo(
            //       "D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx",
            //       "Baikal8",
            //       1000,
            //       new DateTime(2022, 9, 19, 8, 53, 54),
            //       new DateTime(2022, 9, 20, 0, 0, 0),
            //       79.319588,
            //       66.750992);
            //}
            //else
            //{
            //    expectedInfo = new BinaryFileInfo(
            //        "D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin",
            //        "Sigma",
            //        1000,
            //        new DateTime(2022, 9, 19, 8, 38, 45),
            //        new DateTime(2022, 9, 21, 6, 59, 13, 10),
            //        79.33,
            //        66.74);
            //}

            BinarySeismicFile binFile = new BinarySeismicFile(path);
            BinaryFileInfo actual = binFile.ShortFileInfo;
            //Assert.AreEqual(actual.path, expectedInfo.path);
            // Assert.AreEqual(actual.formatType, expectedInfo.formatType);
            // Assert.AreEqual(actual.frequency, expectedInfo.frequency);
            // Assert.AreEqual(actual.timeStart, expectedInfo.timeStart);
            // Assert.AreEqual(actual.timeStop, expectedInfo.timeStop);
            // Assert.AreEqual(actual.longitude, expectedInfo.longitude);
            // //Assert.AreEqual(actual.latitude, expectedInfo.latitude);
        }

        [TestMethod]
        [DataRow("D:/testbinary/HF_0002_2022-09-19_07-48-07_90004_2022-09-19.00", true)]
        [DataRow("D:/testbinary/HF_0004_2022-09-19_08-53-54_K14_2022-09-19.xx", true)]
        [DataRow("D:/testbinary/HF_0009_2022-09-19_08-38-45_SigmaN012_2022-09-19.bin", true)]
        public void testIsCorrectResampleFrequencyMethod(string path, bool expected)
        {
            BinarySeismicFile binFile = new BinarySeismicFile(path);
            bool actual = binFile.IsCorrectResampleFrequency(1000);
            Assert.AreEqual(expected, actual);
        }
    }
}
