using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinReader;

namespace MiniProjectForTestLibrary
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "D:/Binaryfiles/HF_0006_2020-02-22_00-00-00_6247_258.00";
            BinarySeismicFile binfile = new BinarySeismicFile(path);
            Int32[] signalArray = binfile.GetComponentSignal("Z");
            FileHeader fileHat = binfile._FileHeader;
            Console.WriteLine(fileHat.channelCount);
            Console.WriteLine(fileHat.datetimeStart);
            Console.WriteLine(fileHat.frequency);
            Console.WriteLine(fileHat.latitude);
            Console.WriteLine(fileHat.longitude);
            Console.WriteLine(signalArray[1]);
            Console.WriteLine(binfile.DatetimeStop);            

            binfile._ReadDatetimeStop = binfile.DatetimeStart.AddMinutes(1);

            Int32[] signalArrayForOneMinute = binfile.GetComponentSignal("Z");

            binfile._ResampleFrequency = 100;
            Int32[] resampledarrAY = binfile.ResampleSignal(signalArrayForOneMinute);

            Console.WriteLine(signalArray.Length);

            Console.ReadLine();
        }
    }    
}
