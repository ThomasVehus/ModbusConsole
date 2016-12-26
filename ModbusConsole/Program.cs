using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ModbusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TimerCallback callback = new TimerCallback(Tick);

            Console.WriteLine($"Creating timer: {DateTime.Now.ToString("h:mm:ss")}");

            // create a 3 second timer tick
            Timer stateTimer = new Timer(callback, null, 0, 3000);

            // loop here forever
            for (;;)
            {
                // add a sleep for 100 mSec to reduce CPU usage
                Thread.Sleep(100);
            }
        }

        static public void Tick(object sender)
        {
            // For testing
            /*
                        Random rnd = new Random();
                        int tell;
                        tell = rnd.Next(0, 1000);
            */

            ushort[] data;
            string dat = "";

            try
            {
                using (var tcp = new TcpClient("127.0.0.1", 502))
                {
                    using (ModbusIpMaster master = ModbusIpMaster.CreateIp(tcp))
                    {
                        master.Transport.WriteTimeout = 3000;
                        master.Transport.ReadTimeout = 3000;
                        master.Transport.Retries = 3;
                        data = master.ReadInputRegisters(1, 1, 1);
                        dat = data[0].ToString();

                        Console.WriteLine($"Modbus data read: {dat}");

                        // Post data to Web Service
                        string result = PostSingle(dat);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }

        private static string PostSingle(string value)
        {
            try
            {
                string s = "http://gurusoft.northeurope.cloudapp.azure.com:8080/report";
                var url = $"{ s }/logService/timeSeriesReg?k=TEST_KEY_1&v={value}";

                System.Net.WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential("test", "Oneco2016");

                var result = wc.DownloadString(url);
                Console.WriteLine($"Data posted for TEST_KEY_1: {value}");
                Console.WriteLine($"Result: {result}");
                return result;
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Web exception: {ex}");
                return null;
            }
        }
    }
}
