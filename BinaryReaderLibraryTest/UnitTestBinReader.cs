using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public const string SomePath = "/some/path/some.file";
        public const string SomeFileName = "some.file";
        public const int ZeroResampleFrequency = 0;
        public static DateTime NullDateTime = new DateTime();


        public enum RemoveMethod
        {
            None,
            IsBinaryFileAtPath,
            IsCorrectResampleFrequency,
            RecordDateTimeInterval
        }

        public static Mock<FileHeader> GetMockFileHeader
        {
            get
            {
                var mock = new Mock<FileHeader>(Helpers.SomePath) { CallBase = true };
                mock.As<IFileHeader>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(true);
                return mock;
            }
        }

        public static Mock<BinaryFileInfo> GetMockBinaryFileInfo
        {
            get
            {
                var mock = new Mock<BinaryFileInfo>(
                    Helpers.SomePath,
                    Constants.Baikal7Fmt,
                    Helpers.ZeroResampleFrequency,
                    new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime),
                    new Coordinate(0, 0)
                )
                { CallBase = true };
                return mock;
            }
        }

        public static Mock<BinarySeismicFile> GetMockBinarySeismicFile(RemoveMethod removedMethod = RemoveMethod.None)
        {
            var mock = new Mock<BinarySeismicFile>(SomePath, 1, false) { CallBase = true };
            if (removedMethod != RemoveMethod.IsBinaryFileAtPath)
                mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(SomePath)).Returns(true);
            if (removedMethod != RemoveMethod.IsCorrectResampleFrequency)
                mock.As<IBinarySeismicFile>().Setup(p => p.IsCorrectResampleFrequency(1)).Returns(true);
            if (removedMethod != RemoveMethod.RecordDateTimeInterval)
                mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(new DateTime(), new DateTime()));
            return mock;
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
            string fullPath = Path.Combine(Path.GetTempPath(), Helpers.SomeFileName);
            int expectedValue = 1234;
            var filehead = new FileHeader(fullPath);
            dynamic actualValue;

            switch (type)
            {
                case "uint16":
                    {
                        using (var stream = File.Open(fullPath, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((UInt16)expectedValue), 0, 2);
                            }
                        }
                        actualValue = filehead.BinaryRead(fullPath, type, count, 0);
                        Assert.AreEqual((UInt16)expectedValue, (UInt16)actualValue);
                        break;
                    }
                case "uint32":
                    {
                        using (var stream = File.Open(fullPath, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((UInt32)expectedValue), 0, 4);
                            }
                        }
                        actualValue = filehead.BinaryRead(fullPath, type, count, 0);
                        Assert.AreEqual((UInt32)expectedValue, (UInt32)actualValue);
                        break;
                    }
                case "double":
                    {
                        using (var stream = File.Open(fullPath, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((double)expectedValue), 0, 8);
                            }
                        }
                        actualValue = filehead.BinaryRead(fullPath, type, count, 0);
                        Assert.AreEqual((double)expectedValue, (double)actualValue);
                        break;
                    }
                case "long":
                    {
                        using (var stream = File.Open(fullPath, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(BitConverter.GetBytes((ulong)expectedValue), 0, 8);
                            }
                        }
                        actualValue = filehead.BinaryRead(fullPath, type, count, 0);
                        Assert.AreEqual((long)expectedValue, (long)actualValue);
                        break;
                    }
                case "string":
                    {
                        using (var stream = File.Open(fullPath, FileMode.Create))
                        {
                            using (var bw = new BinaryWriter(stream))
                            {
                                bw.Write(Encoding.UTF8.GetBytes(Convert.ToString(expectedValue)));
                            }
                        }
                        actualValue = filehead.BinaryRead(fullPath, type, count, 0);
                        Assert.AreEqual(Convert.ToString(expectedValue), actualValue);
                        break;
                    }
            }
            File.Delete(fullPath);
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
            DateTime expectedDateTimeStart = new DateTime(1980, 1, 1).AddSeconds(timeBegin);

            var actualDateTimeStart = Helpers.GetMockFileHeader.Object.GetDatetimeStartBaikal7((ulong)timeBegin);

            Assert.AreEqual(expectedDateTimeStart, actualDateTimeStart);
        }

        [TestMethod]
        public void testReadBaikal7Header()
        {
            int expectedChannelCount = 1;
            int expectedFrequency = 2;
            DateTime expectedDateTimeStart = Constants.Baikal7BaseDateTime;
            double expectedLongitude = 4.123123;
            double expectedLatitude = 4.123123;

            var actual = new Mock<FileHeader>(Helpers.SomePath) { CallBase = true };
            actual.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(expectedChannelCount)
            .Returns(expectedFrequency)
            .Returns((ulong)0)
            .Returns(expectedLongitude)
            .Returns(expectedLatitude);
            actual.Object.ReadBaikal7Header(Helpers.SomePath);           

            Assert.AreEqual(expectedChannelCount, actual.Object.channelCount);
            Assert.AreEqual(expectedFrequency, actual.Object.frequency);
            Assert.AreEqual(expectedDateTimeStart, actual.Object.datetimeStart);
            Assert.AreEqual(expectedLongitude, actual.Object.coordinate.longitude);
            Assert.AreEqual(expectedLatitude, actual.Object.coordinate.latitude);
        }

        [TestMethod]
        public void testReadBaikal8Header()
        {
            int expectedChannelCount = 1;
            int expectedFrequency = 1000;
            DateTime expectedDateTimeStart = new DateTime(4, 3, 2).AddSeconds(6);
            double expectedLongitude = 7.746972;
            double expectedLatitude = 8.166847;

            var actual = Helpers.GetMockFileHeader;
            actual.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(expectedChannelCount) //channel
            .Returns(expectedDateTimeStart.Day) //day
            .Returns(expectedDateTimeStart.Month) //month
            .Returns(expectedDateTimeStart.Year) //year
            .Returns((double)1 / expectedFrequency) //dt
            .Returns(expectedDateTimeStart.Second) //seconds
            .Returns(expectedLongitude) // latitude
            .Returns(expectedLatitude); //longitude
            actual.Object.ReadBaikal8Header(Helpers.SomePath);

            Assert.AreEqual(expectedChannelCount, actual.Object.channelCount);
            Assert.AreEqual(expectedFrequency, actual.Object.frequency);
            Assert.AreEqual(expectedDateTimeStart, actual.Object.datetimeStart);
            Assert.AreEqual(expectedLongitude, actual.Object.coordinate.longitude);
            Assert.AreEqual(expectedLatitude, actual.Object.coordinate.latitude);
        }

        [TestMethod]
        public void testReadSigmaHeader()
        {                        
            string longitudeSource = "07919.53Z";
            string latitudeSource = "6644.66X";
            string dateSource = "200221";
            string fullTimeSource = "001232";
            int year = 2000 + Convert.ToInt32(dateSource.Substring(0, 2));
            int month = Convert.ToInt32(dateSource.Substring(2, 2));
            int day = Convert.ToInt32(dateSource.Substring(4, 2));
            int hours = Convert.ToInt32(fullTimeSource.Substring(0, 2));
            int minutes = Convert.ToInt32(fullTimeSource.Substring(2, 2));
            int seconds = Convert.ToInt32(fullTimeSource.Substring(4, 2));            
            int expectedChannelCount = 1;
            int expectedFrequency = 2;
            DateTime expectedDateTimeStart = new DateTime(year, month, day, hours, minutes, seconds);
            double expectedLongitude = Math.Round(
                Convert.ToInt32(longitudeSource.Substring(1, 2)) + //<79,317>. Фактически: <79,33>.
                Convert.ToDouble(Convert.ToDouble(longitudeSource.Substring(3, 2)) / Convert.ToDouble(60)), 
                2
            );
            double expectedLatitude = Math.Round(
                Convert.ToInt32(latitudeSource.Substring(0, 2)) + 
                Convert.ToDouble(Convert.ToDouble(longitudeSource.Substring(2, 2)) / Convert.ToDouble(60)), 
                2
            );

            var mock = Helpers.GetMockFileHeader;
            mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(expectedChannelCount) // channel
            .Returns(expectedFrequency) // freq
            .Returns(latitudeSource) // latit
            .Returns(longitudeSource) // long
            .Returns(dateSource) // datesrc
            .Returns(fullTimeSource); // timesrc            
            mock.Object.ReadSigmaHeader(Helpers.SomePath);

            Assert.AreEqual(expectedChannelCount, mock.Object.channelCount);
            Assert.AreEqual(expectedFrequency, mock.Object.frequency);
            Assert.AreEqual(expectedDateTimeStart, mock.Object.datetimeStart);
            Assert.AreEqual(expectedLongitude, mock.Object.coordinate.longitude);
            Assert.AreEqual(expectedLatitude, mock.Object.coordinate.latitude);
        }

        [DataRow("gsdfgdf/bala.bol", "bala.bol")]
        [DataRow("gsd?/fgd/f/12.l", "12.l")]
        [DataRow("y543-0/g5/f", "f")]
        [DataRow("6hrte/3g/", "")]
        [TestMethod]
        public void testName(string path, string expectedName)
        {
            var mock = new Mock<BinaryFileInfo>(
                path,
                Constants.Baikal7Fmt,
                Helpers.ZeroResampleFrequency,
                new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime),
                new Coordinate(0, 0)
            );

            var actualName = mock.Object.Name;

            Assert.AreEqual(expectedName, actualName);
        }

        [DataRow(0)]
        [DataRow(12321)]
        [DataRow(734)]
        [DataRow(74567456)]
        [DataRow(2147483647)]
        [TestMethod]
        public void testDurationInSeconds(int expectedSeconds)
        {
            DateTime startDate = Helpers.NullDateTime;
            DateTime stopDate = startDate.AddSeconds(expectedSeconds);
            DateTimeInterval interval = new DateTimeInterval(startDate, stopDate);
            var mock = new Mock<BinaryFileInfo>(
                Helpers.SomePath,
                Constants.Baikal7Fmt,
                Helpers.ZeroResampleFrequency,
                interval,
                new Coordinate(0, 0))
            { CallBase = true };

            var actualSeconds = mock.Object.DurationInSeconds;

            Assert.AreEqual(expectedSeconds, actualSeconds);
        }

        [TestMethod]
        [DataRow(555, "00:09:15,000")]
        [DataRow(1337, "00:22:17,000")]
        [DataRow(50000, "13:53:20,000")]
        [DataRow(115851, "1 days 08:10:51,000")]
        [DataRow(82485484, "954 days 16:38:04,000")]
        public void testFormattedDuration(int secondsAll, string expectedFormat)
        {
            var mock = Helpers.GetMockBinaryFileInfo;
            mock.As<IBinaryFileInfo>().Setup(p => p.DurationInSeconds).Returns(secondsAll);

            string actualFormat = mock.Object.FormattedDuration;

            Assert.AreEqual(expectedFormat, actualFormat);
        }

        [TestMethod]
        [DataRow("gdf.00", true)]
        [DataRow("gdf.xx", true)]
        [DataRow("gdf.00", true)]
        [DataRow("gdf.oo", false)]
        [DataRow("gdf.hgf", false)]
        public void testIsBinaryFileAtPath(string path, bool expectedBinary)
        {
            string fullPath = Path.Combine(Path.GetTempPath(), path);
            File.Create(fullPath, 4096, FileOptions.DeleteOnClose);
            var mock = Helpers.GetMockBinarySeismicFile();

            bool actualBinary = mock.Object.IsBinaryFileAtPath(fullPath);

            Assert.AreEqual(expectedBinary, actualBinary);
        }

        [DataRow("gdf.6x")]
        [DataRow("fds/fsd/1fsd.sa")]
        [TestMethod]
        public void testGetPath(string expectedPath)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.Object._Path = expectedPath;

            string actualPath = mock.Object.GetPath;

            Assert.AreEqual(expectedPath, actualPath);
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testIsUseAvgValues(bool expectedAvg)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.Object._IsUseAvgValues = expectedAvg;

            bool actualAvg = mock.Object.IsUseAvgValues;

            Assert.AreEqual(expectedAvg, actualAvg);
        }

        [DataRow(1000)]
        [DataRow(11)]
        [TestMethod]
        public void testOriginFrequency(int expectedFrequency)
        {
            var mockbsf = Helpers.GetMockBinarySeismicFile();
            var mockf = Helpers.GetMockFileHeader;
            mockf.Object.frequency = expectedFrequency;
            mockbsf.Object._FileHeader = mockf.Object;

            int actualFrequency = mockbsf.Object.OriginFrequency;

            Assert.AreEqual(expectedFrequency, actualFrequency);
        }

        [DataRow(123)]
        [DataRow(321)]
        [TestMethod]
        public void testResampleFrequency(int expectedFrequency)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.Object._ResampleFrequency = expectedFrequency;

            int actualFrequency = mock.Object.ResampleFrequency;

            Assert.AreEqual(expectedFrequency, actualFrequency);
        }

        [DataRow("gdf/sdf/text.tst", "tst")]
        [DataRow("gdf.qwe", "qwe")]
        [TestMethod]
        public void testFileExtension(string path, string expectedExtension)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.GetPath).Returns(path);

            string actualExtension = mock.Object.FileExtension;

            Assert.AreEqual(expectedExtension, actualExtension);
        }

        [TestMethod]
        [DataRow(Constants.Baikal7Extension, Constants.Baikal7Fmt)]
        [DataRow(Constants.Baikal8Extension, Constants.Baikal8Fmt)]
        [DataRow(Constants.SigmaExtension, Constants.SigmaFmt)]
        [DataRow("oo", null)]
        [DataRow("txt", null)]
        public void testFormatType(string extension, string expectedType)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.FileExtension).Returns(extension);

            string actualType = mock.Object.FormatType;

            Assert.AreEqual(expectedType, actualType);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testHeaderMemorySize(int channelCount)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.ChannelsCount).Returns(channelCount);
            int expectedSize = 120 + 72 * channelCount;

            int actualSize = mock.Object.HeaderMemorySize;

            Assert.AreEqual(expectedSize, actualSize);
        }

        [DataRow(3)]
        [DataRow(6)]
        [TestMethod]
        public void testChannelsCount(int expectedChannelCount)
        {
            var mockbsf = Helpers.GetMockBinarySeismicFile();
            var mockfh = Helpers.GetMockFileHeader;
            mockfh.Object.channelCount = expectedChannelCount;
            mockbsf.Object._FileHeader = mockfh.Object;

            int actualChannelCount = mockbsf.Object.ChannelsCount;

            Assert.AreEqual(expectedChannelCount, actualChannelCount);
        }

        [TestMethod]
        public void testDiscreteAmount()
        {
            string path = Path.Combine(Path.GetTempPath(), Helpers.SomeFileName);

            var random = new Random();

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < random.Next(20000, 4000000); i++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            writer.Write(BitConverter.GetBytes((Int32)2));
                        }                        
                    }
                }
            }

            var mockfh = Helpers.GetMockFileHeader;
            mockfh.Object.channelCount = 3;
            var mockbsf = Helpers.GetMockBinarySeismicFile();
            mockbsf.Object._Path = path;
            mockbsf.Object._FileHeader = mockfh.Object;

            int actualDiscreteAmount = mockbsf.Object.DiscreteAmount;

            FileInfo file = new FileInfo(path);
            long fileSize = file.Length;
            int expectedDiscreteAmount = (int)((fileSize - mockbsf.Object.HeaderMemorySize) / (3 * sizeof(int)));            

            Assert.AreEqual(expectedDiscreteAmount, actualDiscreteAmount);

            file.Delete();
        }

        [DataRow(3, 10)]
        [DataRow(6, 10)]
        [TestMethod]
        public void testSecondsDuration(int discreteCount, int freq)
        {
            int acc = Convert.ToInt32(Math.Log10(freq));
            double btw = Convert.ToDouble(discreteCount) / freq;
            double expectedSecondsDuration = Math.Round(btw, acc);
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(discreteCount);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(freq);

            double actualSecondsDuration = mock.Object.SecondsDuration;

            Assert.AreEqual(expectedSecondsDuration, actualSecondsDuration);
        }

        [DataRow(10432, 2314)]
        [DataRow(-14123, -54431)]
        [TestMethod]
        public void testCoordinate(double longitude, double latitude)
        {
            Coordinate expected = new Coordinate(longitude, latitude);
            var mockfh = Helpers.GetMockFileHeader;
            mockfh.Object.coordinate = expected;
            var mockbsf = Helpers.GetMockBinarySeismicFile();
            mockbsf.Object._FileHeader = mockfh.Object;

            var actual = mockbsf.Object.Coordinate;

            Assert.AreEqual(expected.latitude, actual.latitude);
            Assert.AreEqual(expected.longitude, actual.longitude);
        }

        [DataRow(3)]
        [DataRow(3432)]
        [DataRow(498656516)]
        [TestMethod]
        public void testOriginDateTimeInterval(int second)
        {
            DateTimeInterval expectedInterval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(second));

            var mockfh = Helpers.GetMockFileHeader;
            mockfh.Object.datetimeStart = Helpers.NullDateTime;
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.Object._FileHeader = mockfh.Object;

            var actualInterval = mock.Object.OriginDateTimeInterval;

            Assert.AreEqual(expectedInterval.start, actualInterval.start);
            Assert.AreEqual(expectedInterval.stop, actualInterval.stop);
        }

        [DataRow(3, true)]
        [DataRow(3432, false)]
        [DataRow(498656516, true)]
        [TestMethod]
        public void testRecordDateTimeInterval(int second, bool isSigma)
        {
            DateTimeInterval interval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(second));
            DateTimeInterval expectedInterval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(second));

            var mock = Helpers.GetMockBinarySeismicFile(Helpers.RemoveMethod.RecordDateTimeInterval);
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginDateTimeInterval).Returns(interval);

            if (isSigma == true)
            {
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.SigmaFmt);
                expectedInterval.start = expectedInterval.start.AddSeconds(Constants.SigmaSecondsOffset);
                expectedInterval.stop = expectedInterval.stop.AddSeconds(Constants.SigmaSecondsOffset);
            }
            else
            {
                mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.Baikal7Fmt);
            }

            var actualInterval = mock.Object.RecordDateTimeInterval;

            Assert.AreEqual(expectedInterval.start, actualInterval.start);
            Assert.AreEqual(expectedInterval.stop, actualInterval.stop);
        }

        [DataRow(3)]
        [DataRow(3432)]
        [DataRow(498656516)]
        [TestMethod]
        public void testReadDateTimeIntervalGetter(int second)
        {
            DateTimeInterval expectedInterval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(second));

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(second);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginDateTimeInterval).Returns(expectedInterval);
            mock.Object._ReadDatetimeInterval = expectedInterval;

            var actualInterval = mock.Object.ReadDateTimeInterval;

            Assert.AreEqual(expectedInterval.start, actualInterval.start);
            Assert.AreEqual(expectedInterval.stop, actualInterval.stop);
        }

        [DataRow(5, 1, 2)]
        [DataRow(3432, 123, 452)]
        [DataRow(498656516, 53442, 6234574)]
        [TestMethod]
        public void testReadDateTimeIntervalSetter(int readlong, int dt1Start, int dt2Stop)
        {
            DateTimeInterval recordInterval = new DateTimeInterval(
                Helpers.NullDateTime.AddSeconds(dt1Start + dt2Stop),
                Helpers.NullDateTime.AddSeconds(dt1Start + dt2Stop + readlong)
            );
            DateTimeInterval expectedInterval = new DateTimeInterval(
                recordInterval.start.AddSeconds(dt1Start),
                recordInterval.stop.AddSeconds(dt2Stop * -1)
            );

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(readlong);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordInterval);
            mock.Object.ReadDateTimeInterval = expectedInterval;

            var actualInterval = mock.Object.ReadDateTimeInterval;

            Assert.AreEqual(expectedInterval.start, actualInterval.start);
            Assert.AreEqual(expectedInterval.stop, actualInterval.stop);
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
        public void testReadDateTimeIntervalSetterException(int readlong, int dt1Start, int dt2Stop, int dt2Start, int dt1Stop, bool exceptedException)
        {
            bool actualException = false;
            DateTimeInterval recordInterval = new DateTimeInterval(
                Helpers.NullDateTime.AddSeconds(Math.Abs(dt1Start) + Math.Abs(dt2Start) + Math.Abs(dt1Stop) + Math.Abs(dt2Stop)),
                Helpers.NullDateTime.AddSeconds(Math.Abs(dt1Start) + Math.Abs(dt2Start) + Math.Abs(dt1Stop) + Math.Abs(dt2Stop) + readlong)
            );
            DateTimeInterval intervalToRead = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime);

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

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(readlong);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordInterval);

            try
            {
                mock.Object.ReadDateTimeInterval = intervalToRead;
            }
            catch
            {
                actualException = true;
            }

            Assert.AreEqual(exceptedException, actualException);
        }

        [DataRow(31, 1000)]
        [DataRow(4234, 434)]
        [TestMethod]
        public void testStartMoment(int sec, int freq)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.ReadDateTimeInterval).Returns(new DateTimeInterval(new DateTime().AddSeconds(sec), new DateTime()));
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(freq);
            int expectedStartMoment = freq * sec;
            double actualStartMoment = mock.Object.StartMoment;

            Assert.AreEqual(expectedStartMoment, actualStartMoment);
        }

        [DataRow(1000, 1000, 1)]
        [DataRow(123, 1234, 0)]
        [DataRow(4234, 434, 9)]
        [TestMethod]
        public void testResampleParameter(int origin, int resample, int expectedResampleParameter)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(origin);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleFrequency).Returns(resample);

            double actualResampleParameter = mock.Object.ResampleParameter;

            Assert.AreEqual(expectedResampleParameter, actualResampleParameter);
        }

        [DataRow(3, 2)]
        [DataRow(4234, 434)]
        [DataRow(1425344, 84534)]
        [TestMethod]
        public void testEndMoment(int sec, int startMoment)
        {
            int frequency = 1000;
            int resampleParameter = 4;
            int expectedEndMoment = sec * frequency - startMoment;
            expectedEndMoment = expectedEndMoment - (expectedEndMoment % resampleParameter);
            expectedEndMoment = expectedEndMoment + startMoment;

            DateTimeInterval recordDateTimeInterval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(sec + sec));
            DateTimeInterval readDateTimeInterval = new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddSeconds(sec));

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.ReadDateTimeInterval).Returns(readDateTimeInterval);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(recordDateTimeInterval);
            mock.As<IBinarySeismicFile>().Setup(p => p.StartMoment).Returns(startMoment);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleParameter).Returns(resampleParameter);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(frequency);

            double actualEndMoment = mock.Object.EndMoment;

            Assert.AreEqual(expectedEndMoment, actualEndMoment);
        }

        [TestMethod]
        public void testRecordType()
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            string expectedRecordType = Constants.ComponentsOrder;

            string actualRecordType = mock.Object.RecordType;

            Assert.AreEqual(expectedRecordType, actualRecordType);
        }

        [TestMethod]
        public void testComponentsIndex()
        {
            var expectedIndexes = new Dictionary<string, int>(); ;
            expectedIndexes.Add("Z", 0);
            expectedIndexes.Add("X", 1);
            expectedIndexes.Add("Y", 2);
            var mock = Helpers.GetMockBinarySeismicFile();

            var actualIndexes = mock.Object.ComponentsIndex;

            Assert.AreEqual(expectedIndexes["Z"], actualIndexes["Z"]);
            Assert.AreEqual(expectedIndexes["X"], actualIndexes["X"]);
            Assert.AreEqual(expectedIndexes["Y"], actualIndexes["Y"]);
        }

        [TestMethod]
        public void testShortFileInfo()
        {
            BinaryFileInfo expected = new BinaryFileInfo(
                Helpers.SomePath,
                Constants.SigmaFmt,
                Helpers.ZeroResampleFrequency,
                new DateTimeInterval(Helpers.NullDateTime, Helpers.NullDateTime.AddDays(1)),
                new Coordinate(0, 0));

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.GetPath).Returns(Helpers.SomePath);
            mock.As<IBinarySeismicFile>().Setup(p => p.FormatType).Returns(Constants.SigmaFmt);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(Helpers.ZeroResampleFrequency);
            mock.As<IBinarySeismicFile>().Setup(p => p.RecordDateTimeInterval).Returns(new DateTimeInterval(
                Helpers.NullDateTime,
                Helpers.NullDateTime.AddDays(1))
            );
            mock.As<IBinarySeismicFile>().Setup(p => p.Coordinate).Returns(new Coordinate(1, 2));

            var actual = mock.Object.ShortFileInfo;

            Assert.AreEqual(expected.path, actual.path);
            Assert.AreEqual(expected.formatType, actual.formatType);
            Assert.AreEqual(expected.frequency, actual.frequency);
            Assert.AreEqual(expected.datetimeInterval.stop, actual.datetimeInterval.stop);
            Assert.AreEqual(expected.datetimeInterval.start, actual.datetimeInterval.start);
            Assert.AreEqual(expected.coordinate.longitude, actual.coordinate.longitude);
            Assert.AreEqual(expected.coordinate.latitude, actual.coordinate.latitude);
        }

        [DataRow(100, -100, false)]
        [DataRow(100, 0, true)]
        [DataRow(1000, 100, true)]
        [DataRow(1000, 98, false)]
        [TestMethod]
        public void testIsCorrectResampleFrequency(int origin, int resample, bool expectedCorrectResampleFrequency)
        {
            var mock = Helpers.GetMockBinarySeismicFile(Helpers.RemoveMethod.IsCorrectResampleFrequency);
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(origin);

            bool actualCorrectResampleFrequency = mock.Object.IsCorrectResampleFrequency(resample);

            Assert.AreEqual(expectedCorrectResampleFrequency, actualCorrectResampleFrequency);
        }

        [DataRow(new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 2, new int[5] { 1, 3, 5, 7, 9 })]
        [DataRow(new int[6] { 1, 3, 4, 8, 10, 12 }, 2, new int[3] { 2, 6, 11 })]
        [DataRow(new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2, new int[4] { 1, 3, 5, 7 })]
        [DataRow(new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, new int[3] { 2, 5, 8 })]
        [TestMethod]
        public void testResampling(int[] signal, int resampleParam, int[] expectedSignal)
        {
            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(0);

            int[] actualSignal = mock.Object.Resampling(signal, resampleParam);

            for (int i = 0; i < expectedSignal.Length; i++)
            {
                Assert.AreEqual(expectedSignal[i], actualSignal[i]);
            }
        }

        [TestMethod]
        public void testGetComponentSignal()
        {
            string path = Path.Combine(Path.GetTempPath(), Helpers.SomeFileName);

            var random = new Random();
            Int32[] expectedArray = new Int32[100];

            for (int i = 0; i < expectedArray.Length; i++)
            {
                expectedArray[i] = random.Next(-32768, 32768);
            }

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    for (int i = 0; i < expectedArray.Length; i++)
                    {
                        bw.Write(BitConverter.GetBytes(expectedArray[i]), 0, 4);
                        bw.Seek(8, SeekOrigin.Current);
                    }
                }
            }

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.StartMoment).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.EndMoment).Returns(expectedArray.Length);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.HeaderMemorySize).Returns(0);
            mock.As<IBinarySeismicFile>().Setup(p => p.ChannelsCount).Returns(3);
            mock.Object._Path = path;

            int[] actualArray = mock.Object.GetComponentSignal("Z");
            
            for (int i = 0; i < expectedArray.Length; i++)
            {
                Assert.AreEqual(expectedArray[i], actualArray[i]);
            }

            File.Delete(path);
        }

        [DataRow(1, false)]
        [DataRow(2, true)]
        [DataRow(0, true)]
        [DataRow(1432, true)]
        [TestMethod]
        public void testResampleSignal(int ResampleParameter, bool isCalled)
        {
            int[] signal = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var mock = Helpers.GetMockBinarySeismicFile();
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
            string path = Path.Combine(Path.GetTempPath(), Helpers.SomeFileName);

            Int32[] signalArr = new Int32[10000];
            var random = new Random();

            for (int i = 0; i < signalArr.Length; i++)
            {
                signalArr[i] = random.Next(-32768, 32768);
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

            var mock = Helpers.GetMockBinarySeismicFile();
            mock.As<IBinarySeismicFile>().Setup(p => p.OriginFrequency).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.DiscreteAmount).Returns(1);
            mock.As<IBinarySeismicFile>().Setup(p => p.GetComponentSignal(It.IsAny<string>())).Returns(signalArr);
            mock.As<IBinarySeismicFile>().Setup(p => p.ResampleSignal(It.IsAny<int[]>())).Returns(signalArr);
            mock.Object._Path = path;
            mock.Object._IsUseAvgValues = true;

            int[] actualArray = mock.Object.ReadSignal("Z");

            if (isUseAvgValues == false)
            {
                for (int i = 0; i < signalArr.Length; i++)
                {
                    Assert.AreEqual(signalArr[i], actualArray[i]);
                }
            }

            int expectedArraySum = 0;
            for (int i = 0; i < signalArr.Length; i++)
            {
                expectedArraySum += signalArr[i];
            }
            expectedArraySum = expectedArraySum / signalArr.Length;
            int[] expectedArray = new int[signalArr.Length];
            for (int i = 0; i < expectedArray.Length; i++)
            {
                expectedArray[i] = signalArr[i] - expectedArraySum;
            }

            for (int i = 0; i < signalArr.Length; i++)
            {
                Assert.AreEqual(expectedArray[i], actualArray[i]);
            }

            File.Delete(path);
        }
    }
}
