using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using BinReader;
using System.Collections.Generic;

namespace BinaryReaderLibraryTest
{
    [TestClass]
    public class TestLibrary
    {
        [TestMethod]
        [DataRow(555, "00:09:15,000")]
        [DataRow(1337, "00:22:17,000")]
        [DataRow(50000, "13:53:20,000")]        
        [DataRow(115851, "1 days 08:10:51,000")]
        [DataRow(82485484, "954 days 16:38:04,000")]
        public void testFormattedDuration(int secondsAll, string expected)
        {
            var mock = new Mock<BinaryFileInfo>("", "", 0, new DateTime(), new DateTime(), 0, 0) { CallBase = true };
            mock.As<IBinaryFileInfo>().Setup(p => p.DurationInSeconds).Returns(secondsAll);
            string actual = mock.Object.FormattedDuration;
            Assert.AreEqual(expected, actual);
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

            var mock = new Mock<BinarySeismicFile>("", 0, false) { CallBase = true };            
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(0);
           //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStart).Returns(new DateTime());
           // mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStop).Returns(new DateTime());
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            //mock.As<IBinarySeismicFile>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.As<IBinarySeismicFile>().Setup(p => p.GetFileHeader).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));            

            //var actual = mock.Object.GetDatetimeStartBaikal7((ulong)timeBegin);
                      
            //Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void testReadBaikal7Header()
        {
            //var expected = new FileHeader(0, 0, new DateTime(), 0, 0);

            var mock = new Mock<BinarySeismicFile>("", 0, false) { CallBase = true };
            mock.As<IBinarySeismicFile>().Setup(p => p.SecondsDuration).Returns(0);
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStart).Returns(new DateTime());
            //mock.As<IBinarySeismicFile>().Setup(p => p.DatetimeStop).Returns(new DateTime());
            mock.As<IBinarySeismicFile>().Setup(p => p.IsBinaryFileAtPath(It.IsAny<string>())).Returns(true);
            //mock.As<IBinarySeismicFile>().Setup(p => p.ReadBaikal7Header(It.IsAny<string>())).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.As<IBinarySeismicFile>().Setup(p => p.GetFileHeader).Returns(new FileHeader(0, 0, new DateTime(), 0, 0));
            //mock.SetupSequence(f => f.BinaryRead(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            //.Returns(1)
            //.Returns(2)
            //.Returns(new DateTime())
            //.Returns(4)
            //.Returns(5);

            ////var actual = mock.Object.ReadBaikal7Header("");

            //Assert.AreEqual(expected.longitude, actual.longitude);
            //Assert.AreEqual(expected.latitude, actual.latitude);
            //Assert.AreEqual(expected.channelCount, actual.channelCount);
            //Assert.AreEqual(expected.frequency, actual.frequency);
            //Assert.AreEqual(expected.datetimeStart, actual.datetimeStart);
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
