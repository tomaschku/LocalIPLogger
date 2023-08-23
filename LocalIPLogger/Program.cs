using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace LocalIPLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No path specified.");
                return;
            }

            IPCSV csv = new IPCSV(args[0]);

            string readTime = csv.getLatestEntry().time.ToShortTimeString();
            if (readTime != DateTime.Now.ToShortTimeString()) //DateTime.Now.ToString("HH:mm"))
            {
                string externalIP = new WebClient().DownloadString("http://icanhazip.com");

                IPAddress ip = new IPAddress(new byte[] {
                    Convert.ToByte(externalIP.Split('.')[0]),
                    Convert.ToByte(externalIP.Split('.')[1]),
                    Convert.ToByte(externalIP.Split('.')[2]),
                    Convert.ToByte(externalIP.Split('.')[3]),
                });

                csv.writeEntry(new IPCSV.Data(DateTime.Now, ip));
            }
        }
    }

    public class IPCSV
    {
        string path = "";

        public IPCSV(string _Path)
        {
            path = _Path;
        }

        public Data getLatestEntry()
        {
            string[] lines = File.ReadAllLines(path);

            int reverseIdx = 1;
            string latestEntry = lines[lines.Length - reverseIdx];

            while(latestEntry.Length < 28) //28 is the shortest possible entry
            {
                reverseIdx++;
                latestEntry = lines[lines.Length - reverseIdx];
            }

            DateTime time = new DateTime(
                Convert.ToInt32(latestEntry.Substring(6, 4)), /* Year */
                Convert.ToInt32(latestEntry.Substring(3, 2)), /* Month */
                Convert.ToInt32(latestEntry.Substring(0, 2)), /* Day */
                Convert.ToInt32(latestEntry.Substring(11, 2)), /* Hour */
                Convert.ToInt32(latestEntry.Substring(14, 2)), /* Minute */
                0); /* Second */

            string ipStr = latestEntry.Substring(17);
            IPAddress ip = new IPAddress(new byte[] {
                    Convert.ToByte(ipStr.Split('.')[0]),
                    Convert.ToByte(ipStr.Split('.')[1]),
                    Convert.ToByte(ipStr.Split('.')[2]),
                    Convert.ToByte(ipStr.Split('.')[3]),
                });

           return new Data(time,ip);
        }

        public void writeEntry(Data entry)
        {
            File.AppendAllText(path, Environment.NewLine + entry.ToString(), Encoding.ASCII);
        }

        public class Data
        {
            public DateTime time;
            public IPAddress ip;

            public Data()
            {
                time = new DateTime(0);
                ip = new IPAddress(new byte[] {0,0,0,0});
            }

            public Data(DateTime _Time, IPAddress _Ip)
            {
                time = _Time;
                ip = _Ip;
            }

            override public string ToString()
            {
                return time.ToShortDateString() + "," + time.ToShortTimeString() + "," + ip.ToString();
            }
        }
    }
}