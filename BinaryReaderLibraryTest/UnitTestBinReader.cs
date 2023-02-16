﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using BinReader;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinaryReaderLibraryTest
{
    public class Helpers
    { 
        public static Mock<FileHeader> getMockFileHeader
        {
            get 
            {
                var mock = new Mock<FileHeader>("123.00") { CallBase = true };
                mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
                return mock;
            }
        }

        public static Mock<BinaryFileInfo> getMockBinaryFileInfo
        {
            get
            {
                var mock = new Mock<BinaryFileInfo>(
                    "", 
                    "", 
                    0, 
                    new DateTimeInterval(new DateTime(), new DateTime()), 
                    new Coordinate(0, 0)
                ) { CallBase = true };                
                return mock;
            }
        }

        public static Mock<BinarySeismicFile> getMockBinarySeismicFile
        {
            get
            {
                var mock = new Mock<BinarySeismicFile>("123.10", 1, false) { CallBase = true };
                mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.10")).Returns(true);
                mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(1)).Returns(true);
                mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
                return mock;
            }
        }        
    }

    [TestClass]
    public class TestLibrary
    {
        [TestMethod]
        [DataRow("uint16", 1)]
        [DataRow("uint32", 1)]
        [DataRow("double", 1)]
        [DataRow("long", 1)]
        [DataRow("string", 4)]
        public void testBinaryRead(string type, int count)
        {
            string filename = "/testGetComponentSignal.binary";
            string path = Environment.CurrentDirectory + filename;

            var filehead = new FileHeader(path);
            dynamic actual;

            switch (type)
            {
                case "uint16":
                    {
                        using (var stream = File.Open(path, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((UInt16)1234), 0, 2);
                            }
                        }
                        actual = filehead.BinaryRead(path, type, count, 0);                        
                        Assert.AreEqual((UInt16)1234, (UInt16)actual);
                        break;
                    }
                case "uint32":
                    {
                        using (var stream = File.Open(path, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((UInt32)1234), 0, 4);
                            }
                        }
                        actual = filehead.BinaryRead(path, type, count, 0);
                        Assert.AreEqual((UInt32)1234, (UInt32)actual);
                        break;
                    }
                case "double":
                    {
                        using (var stream = File.Open(path, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((double)1234), 0, 8);
                            }
                        }
                        actual = filehead.BinaryRead(path, type, count, 0);
                        Assert.AreEqual((double)1234, (double)actual);
                        break;
                    }
                case "long":
                    {
                        using (var stream = File.Open(path, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((ulong)1234), 0, 8);
                            }
                        }
                        actual = filehead.BinaryRead(path, type, count, 0);
                        Assert.AreEqual((long)1234, (long)actual);
                        break;
                    }
                case "string":
                    {
                        using (var stream = File.Open(path, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(Encoding.UTF8.GetBytes("1234"));
                            }
                        }
                        actual = filehead.BinaryRead(path, type, count, 0);
                        Assert.AreEqual("1234", actual);
                        break;
                    }
            }

            File.Delete(path);            
        }

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
            
            var actual = Helpers.getMockFileHeader.Object.GetDatetimeStartBaikal7((ulong)timeBegin);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void testReadBaikal7Header()
        {
            var mock = new Mock<FileHeader>("123.10") { CallBase = true };
            mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1)
            .Returns(2)
            .Returns((ulong)0)
            .Returns((double)4.12312342543)
            .Returns((double)5.12312312543);

            bool headerRead = mock.Object.ReadBaikal7Header("123.00");

            Assert.AreEqual(1, mock.Object.channelCount);
            Assert.AreEqual(2, mock.Object.frequency);
            Assert.AreEqual(Constants.Baikal7BaseDateTime, mock.Object.datetimeStart);
            Assert.AreEqual(4.123123, mock.Object.coordinate.longitude);
            Assert.AreEqual(5.123123, mock.Object.coordinate.latitude);
        }

        [TestMethod]
        public void testReadBaikal8Header()
        {
            var mock = Helpers.getMockFileHeader;
            mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1) //channel
            .Returns(2) //day
            .Returns(3) //month
            .Returns(4) //year
            .Returns(0.001) //dt
            .Returns(6) //seconds
            .Returns((double)7.7469722438) // latitude
            .Returns((double)8.166847342); //longitude

            bool headerRead = mock.Object.ReadBaikal8Header("123.00");

            Assert.AreEqual(1, mock.Object.channelCount);
            Assert.AreEqual(1000, mock.Object.frequency);
            Assert.AreEqual(new DateTime(4, 3, 2).AddSeconds(6), mock.Object.datetimeStart);
            Assert.AreEqual(7.746972, mock.Object.coordinate.longitude);
            Assert.AreEqual(8.166847, mock.Object.coordinate.latitude);
        }

        [TestMethod]
        public void testReadSigmaHeader()
        {
            var mock = Helpers.getMockFileHeader;
            mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1) // channel
            .Returns(2) // freq
            .Returns("6644.66X") // latit
            .Returns("07919.53Z") // long
            .Returns("200221") // datesrc
            .Returns("1232"); // timesrc

            int year = 2000 + 20;
            int month = 2;
            int day = 21;
            int hours = 0;
            int minutes = 12;
            int seconds = 32;
            DateTime expDateTimeStart = new DateTime(year, month, day, hours, minutes, seconds);
            double longitude = Math.Round(79 + Convert.ToDouble((Convert.ToDouble(19) / Convert.ToDouble(60))), 2);
            double latitude = Math.Round(66 + Convert.ToDouble((Convert.ToDouble(44) / Convert.ToDouble(60))), 2);

            bool headerRead = mock.Object.ReadSigmaHeader("123.00");

            Assert.AreEqual(1, mock.Object.channelCount);
            Assert.AreEqual(2, mock.Object.frequency);
            Assert.AreEqual(expDateTimeStart, mock.Object.datetimeStart);
            Assert.AreEqual(79.33, mock.Object.coordinate.longitude);
            Assert.AreEqual(66.74, mock.Object.coordinate.latitude);
        }

        [DataRow("gsdfgdf/bala.bol", "bala.bol")]
        [DataRow("gsd?/fgd/f/12.l", "12.l")]
        [DataRow("y543-0/g5/f", "f")]
        [DataRow("6hrte/3g/", "")]
        [TestMethod]
        public void testName(string path, string expected)
        {
            var mock = new Mock<BinaryFileInfo>(path, "", 0, new DateTimeInterval(new DateTime(), new DateTime()), new Coordinate(0, 0));

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
            var mock = new Mock<BinaryFileInfo>("", "", 0, interval, new Coordinate(0, 0)) { CallBase = true };

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
            var mock = Helpers.getMockBinaryFileInfo;
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
            File.Create(@"C:\Windows\Temp\" + path, 4096, FileOptions.DeleteOnClose);
            var mock = Helpers.getMockBinarySeismicFile;
            
            bool actual = mock.Object.IsBinaryFileAtPath(@"C:\Windows\Temp\" + path);

            Assert.AreEqual(result, actual);
        }

        [DataRow("gdf.6x")]
        [DataRow("fds/fsd/1fsd.sa")]
        [TestMethod]
        public void testGetPath(string path)
        {
            var mock = Helpers.getMockBinarySeismicFile;
            mock.Object._Path = path;

            string actual = mock.Object.GetPath;

            Assert.AreEqual(path, actual);
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testIsUseAvgValues(bool avg)
        {
            var mock = Helpers.getMockBinarySeismicFile;
            mock.Object._IsUseAvgValues = avg;

            bool actual = mock.Object.IsUseAvgValues;

            Assert.AreEqual(avg, actual);
        }

        [DataRow(1000)]
        [DataRow(11)]
        [TestMethod]
        public void testOriginFrequency(int freq)
        {
            var mock = Helpers.getMockBinarySeismicFile;
            var mockf = Helpers.getMockFileHeader;
            mockf.Object.frequency = freq;
            mock.Object._FileHeader = mockf.Object;

            int actual = mock.Object.OriginFrequency;            

            Assert.AreEqual(freq, actual);
        }

        [DataRow(123)]
        [DataRow(321)]
        [TestMethod]
        public void testResampleFrequency(int freq)
        {
            var mock = Helpers.getMockBinarySeismicFile;
            mock.Object._ResampleFrequency = freq;

            int actual = mock.Object.ResampleFrequency;

            Assert.AreEqual(freq, actual);
        }

        [DataRow("gdf/sdf/text.tst", "tst")]
        [DataRow("gdf.qwe", "qwe")]
        [TestMethod]
        public void testFileExtension(string path, string ext)
        {
            var mock = Helpers.getMockBinarySeismicFile;
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
            var mock = Helpers.getMockBinarySeismicFile;
            mock.As<IBinarySeismicFile>().Setup(p => p.FileExtension).Returns(ext);

            string actual = mock.Object.FormatType;

            Assert.AreEqual(result, actual);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testHeaderMemorySize(int chcount)
        {
            var mock = Helpers.getMockBinarySeismicFile;
            mock.As<IBinarySeismicFile>().Setup(p => p.ChannelsCount).Returns(chcount);
            int result = 120 + 72 * chcount;

            int actual = mock.Object.HeaderMemorySize;
            
            Assert.AreEqual(result, actual);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testChannelsCount(int chcount)
        {
            var mockbsf = Helpers.getMockBinarySeismicFile;
            var mockfh = Helpers.getMockFileHeader;
            mockfh.Object.channelCount = chcount;
            mockbsf.Object._FileHeader = mockfh.Object;

            int actual = mockbsf.Object.ChannelsCount;            

            Assert.AreEqual(chcount, actual);
        }

        [TestMethod]
        public void testDiscreteAmount()
        {
            string filename = "/testGetComponentSignal.binary";
            string path = Environment.CurrentDirectory + filename;

            Random rnd = new Random();

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < rnd.Next(80000, 10000000); i++)
                    { writer.Write(BitConverter.GetBytes((Int32)2)); ; }
                }
            }

            var mockfh = Helpers.getMockFileHeader;
            mockfh.Object.channelCount = 3;
            var mockbsf = Helpers.getMockBinarySeismicFile;
            mockbsf.Object._Path = path;
            mockbsf.Object._FileHeader = mockfh.Object;

            int actual = mockbsf.Object.DiscreteAmount;

            FileInfo file = new FileInfo(path);
            long fileSize = file.Length;
            int expected = (int)((fileSize - mockbsf.Object.HeaderMemorySize) / (3 * sizeof(int)));
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
            var mock = Helpers.getMockBinarySeismicFile;
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(discreteCount);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(freq);

            double actual = mock.Object.SecondsDuration;

            Assert.AreEqual(expected, actual);
        }

        [DataRow(10432, 2314)]
        [DataRow(-14123, -54431)]
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
            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginDateTimeInterval).Returns(interval);

            DateTimeInterval expinterval = new DateTimeInterval(def, def.AddSeconds(second));

            if (isSigma == true)
            {
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.SigmaFmt);
                expinterval.start = expinterval.start.AddSeconds(Constants.SigmaSecondsOffset);
                expinterval.stop = expinterval.stop.AddSeconds(Constants.SigmaSecondsOffset);
            }
            else
            {
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.Baikal7Fmt);
            }

            var actual = mock.Object.RecordDateTimeInterval;

            Assert.AreEqual(expinterval.start, actual.start);
            Assert.AreEqual(expinterval.stop, actual.stop);
        }

        [DataRow(3)]
        [DataRow(3432)]
        [DataRow(498656516)]
        [TestMethod]
        public void testReadDateTimeIntervalGetter(int second)
        {
            DateTime def = new DateTime();
            DateTimeInterval interval = new DateTimeInterval(def, def.AddSeconds(second));
            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginDateTimeInterval).Returns(interval);

            mock.Object._ReadDatetimeInterval = interval;

            var actual = mock.Object.ReadDateTimeInterval;

            Assert.AreEqual(interval.start, actual.start);
            Assert.AreEqual(interval.stop, actual.stop);
        }

        [DataRow(5, 1, 2)]
        [DataRow(3432, 123, 452)]
        [DataRow(498656516, 53442, 6234574)]
        [TestMethod]
        public void testReadDateTimeIntervalSetter(int readlong, int dt1Start, int dt2Stop)
        {
            DateTime def = new DateTime();
            DateTimeInterval recordInterval = new DateTimeInterval(
                def.AddSeconds(dt1Start + dt2Stop),
                def.AddSeconds(dt1Start + dt2Stop + readlong)
            );

            DateTimeInterval intervalToRead = new DateTimeInterval(
                    recordInterval.start.AddSeconds(dt1Start),
                    recordInterval.stop.AddSeconds(dt2Stop * -1)
                );

            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(readlong);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordInterval);

            mock.Object.ReadDateTimeInterval = intervalToRead;

            var actual = mock.Object.ReadDateTimeInterval;

            Assert.AreEqual(intervalToRead.start, actual.start);
            Assert.AreEqual(intervalToRead.stop, actual.stop);
        }

        [DataRow(50, 2, 2, 0, 0, false)]
        [DataRow(60000, 5234, 45678, 0, 0, false)]
        [DataRow(50, 0, 2, 0, 0, false)]
        [DataRow(60000, 0, 45678, 0, 0, false)]
        [DataRow(50, 2, 0, 0, 0, false)]
        [DataRow(60000, 5234, 0, 0, 0, false)]
        [DataRow(50, -2, 20, 0, 0, true)]
        [DataRow(60000, -5234, 45678, 0, 0, true)]
        [DataRow(50, 20, -2, 0, 0, true)]
        [DataRow(60000, 45234, -5678, 0, 0, true)]
        [DataRow(50, 0, 0, 30, 30, false)]
        [DataRow(60000, 0, 0, 40591, 45678, false)]
        [DataRow(50, 0, 0, 0, 30, true)]
        [DataRow(60000, 0, 0, 0, 45678, true)]
        [DataRow(50, 0, 0, 30, 0, true)]
        [DataRow(60000, 0, 0, 40591, 0, true)]
        [DataRow(350, 0, 0, 60, -60, true)]
        [DataRow(160000, 0, 0, 40591, -45678, true)]
        [DataRow(50, 0, 0, -30, 30, true)]
        [DataRow(60000, 0, 0, -40591, 45678, true)]
        [TestMethod]
        public void testReadDateTimeIntervalSetterException(int readlong, int dt1Start, int dt2Stop, int dt2Start, int dt1Stop, bool exceptionExp)
        {
            bool isException = false;

            DateTime def = new DateTime();
            DateTimeInterval recordInterval = new DateTimeInterval(
                def.AddSeconds(Math.Abs(dt1Start) + Math.Abs(dt2Start) + Math.Abs(dt1Stop) + Math.Abs(dt2Stop)),
                def.AddSeconds(Math.Abs(dt1Start) + Math.Abs(dt2Start) + Math.Abs(dt1Stop) + Math.Abs(dt2Stop) + readlong)
            );

            DateTimeInterval intervalToRead = new DateTimeInterval(def, def);

            if (dt2Start == 0 && dt1Stop == 0)
            {
                intervalToRead = new DateTimeInterval(
                        recordInterval.start.AddSeconds(dt1Start),
                        recordInterval.stop.AddSeconds(-dt2Stop)
                    );
            }
            else if (dt1Start == 0 && dt2Stop == 0)
            {
                intervalToRead = new DateTimeInterval(
                            recordInterval.stop.AddSeconds(-dt2Start),
                            recordInterval.start.AddSeconds(dt1Stop)
                        );
            }

            var mock = new Mock<BinarySeismicFile>(@"D:/exampleFile.123", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(336);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(readlong);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordInterval);

            try
            {
                mock.Object.ReadDateTimeInterval = intervalToRead;
            }
            catch
            {
                isException = true;
            }

            Assert.AreEqual(exceptionExp, isException);
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

        [DataRow(3, 2)]
        [DataRow(4234, 434)]
        [DataRow(1425344, 84534)]
        [TestMethod]
        public void testEndMoment(int sec, int startMom)
        {
            int exp = sec * 1000 - startMom;
            exp = exp - (exp % 4);
            exp = exp + startMom;

            DateTime def = new DateTime();
            DateTimeInterval recordDateTimeInterval = new DateTimeInterval(def, def.AddSeconds(sec + sec));
            DateTimeInterval readDateTimeInterval = new DateTimeInterval(def, def.AddSeconds(sec));

            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1000, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.ReadDateTimeInterval).Returns(readDateTimeInterval);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordDateTimeInterval);
            mock.As<IBinarySeismicFile>().Setup(p => p.StartMoment).Returns(startMom);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleParameter).Returns(4);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1000);

            double actual = mock.Object.EndMoment;

            Assert.AreEqual(exp, actual);
        }

        [TestMethod]
        public void testRecordType()
        {
            var mock = new Mock<BinarySeismicFile>("123.1", 1, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath("123.1")).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(123);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(123);
            string actual = mock.Object.RecordType;
            Assert.AreEqual(Constants.ComponentsOrder, actual);
        }

        [TestMethod]
        public void testComponentsIndex()
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(1);

            var actual = mock.Object.ComponentsIndex;

            var componentsIndexes = new Dictionary<string, int>(); ;
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
        [DataRow(100, 0, true)]
        [DataRow(1000, 100, true)]
        [DataRow(1000, 98, false)]
        [TestMethod]
        public void testIsCorrectResampleFrequency(int origin, int resample, bool exp)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 99, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(99)).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleParameter).Returns(4);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(origin);

            bool actual = mock.Object.IsCorrectResampleFrequency(resample);

            Assert.AreEqual(exp, actual);
        }

        [DataRow(new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 2, new int[5] { 2, 4, 6, 8, 10 })]
        [DataRow(new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, new int[3] { 3, 6, 9 })]
        [TestMethod]
        public void testResampling(int[] signal, int resampleParam, int[] expected)
        {
            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(0);

            int[] actual = mock.Object.Resampling(signal, resampleParam);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void testGetComponentSignal()
        {
            string filename = "/testGetComponentSignal.binary";
            string path = Environment.CurrentDirectory + filename;

            Random rnd = new Random();
            Int32[] signalArr = new Int32[10000];

            for (int i = 0; i < signalArr.Length; i++)
            {
                signalArr[i] = rnd.Next(-32768, 32768);
            }

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    for (int i = 0; i < signalArr.Length; i++)
                    {
                        bw.Write(BitConverter.GetBytes(signalArr[i]), 0, 4);
                        bw.Seek(8, SeekOrigin.Current);
                    }
                }
            }

            var mock = new Mock<BinarySeismicFile>(path, 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.StartMoment).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.EndMoment).Returns(signalArr.Length);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.ChannelsCount).Returns(3);

            int[] actual = mock.Object.GetComponentSignal("Z");

            File.Delete(path);

            for (int i = 0; i < signalArr.Length; i++)
            {
                Assert.AreEqual(signalArr[i], actual[i]);
            }
        }

        [DataRow(1, false)]
        [DataRow(2, true)]
        [DataRow(0, true)]
        [DataRow(1432, true)]
        [TestMethod]
        public void testResampleSignal(int ResampleParameter, bool isCalled)
        {
            int[] signal = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var mock = new Mock<BinarySeismicFile>(@"C:\Windows\Temp\gdf.10", 99, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(99)).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.Resampling(It.IsAny<int[]>(), It.IsAny<int>())).Returns(signal);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleParameter).Returns(ResampleParameter);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);

            int[] actual = mock.Object.ResampleSignal(signal);

            if (isCalled == true)
            {
                mock.Verify(p => p.Resampling(It.IsAny<int[]>(), It.IsAny<int>()), Times.Once());
            }
            else
            {
                mock.Verify(p => p.Resampling(It.IsAny<int[]>(), It.IsAny<int>()), Times.Never());
            }
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testReadSignal(bool isUseAvgValues)
        {
            string filename = "/testGetComponentSignal.binary";
            string path = Environment.CurrentDirectory + filename;

            Int32[] signalArr = new Int32[10000];

            Random rnd = new Random();
            for (int i = 0; i < signalArr.Length; i++)
            {
                signalArr[i] = rnd.Next(-32768, 32768);
            }

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    for (int i = 0; i < signalArr.Length; i++)
                    {
                        bw.Write(BitConverter.GetBytes(signalArr[i]), 0, 4);
                        bw.Seek(8, SeekOrigin.Current);
                    }
                }
            }

            var mock = new Mock<BinarySeismicFile>(path, 1, true) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(It.IsAny<int>())).Returns(true);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.GetComponentSignal(It.IsAny<string>())).Returns(signalArr);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleSignal(It.IsAny<int[]>())).Returns(signalArr);

            int[] actual = mock.Object.ReadSignal("Z");

            if (isUseAvgValues == false)
            {
                for (int i = 0; i < signalArr.Length; i++)
                {
                    Assert.AreEqual(signalArr[i], actual[i]);
                }
            }

            int signalArrAvg = 0;
            for (int i = 0; i < signalArr.Length; i++)
            {
                signalArrAvg += signalArr[i];
            }
            signalArrAvg = signalArrAvg / signalArr.Length;
            int[] signalAvg = new int[signalArr.Length];
            for (int i = 0; i < signalAvg.Length; i++)
            {
                signalAvg[i] = signalArr[i] - signalArrAvg;
            }

            File.Delete(path);

            for (int i = 0; i < signalArr.Length; i++)
            {
                Assert.AreEqual(signalAvg[i], actual[i]);
            }
        }
    }
}
