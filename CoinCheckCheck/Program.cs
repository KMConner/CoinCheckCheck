using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

using static System.Console;

namespace CoinCheckCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            object oo = new object();
            long lag = GetTimeLag();
            WriteLine(lag);

            WriteLine("Hello World!");
            var timer = new Timer((object __) =>
            {
                var wc = new WebClient();
                long i = 0;
                var start = DateTimeOffset.UtcNow;
                string timeJson = "ERROR!!";
                Dictionary<string, string> parsed = null;
                try
                {
                    timeJson = wc.DownloadString("https://coincheck.com/api/rate/btc_jpy");
					parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(timeJson);
                }
                catch (Exception e)
                {
                    WriteLine($"{start.ToUnixTimeMilliseconds() + lag}\n{e.Message}");
                }
                //WriteLine(start.ToUnixTimeMilliseconds());
                lock (oo)
                {
                    try
                    {
                        using (var writer = new StreamWriter("./out.csv", true))
                        {
                            writer.WriteLine($"{start.ToUnixTimeMilliseconds() + lag},{parsed["rate"]}");
                            i++;
                            if (i % 10000 == 0)
                            {
                                WriteLine($"{i} items!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLine($"{start.ToUnixTimeMilliseconds() + lag}\n{e.Message}");
                    }
                }
            });

            timer.Change(TimeSpan.FromMilliseconds(1000 - ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + lag) % 1000)), TimeSpan.FromMilliseconds(100));

            while (ReadLine()!="Exit")
            {
                WriteLine("Try Again!");
            }
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        static long GetTimeLag()
        {
            var reg = new Regex(@"\<BODY\>(.*?)\<\/body\>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var ww = new WebClient();
            var start = DateTimeOffset.UtcNow;
            var timeStr = ww.DownloadString("http://ntp-b1.nict.go.jp/cgi-bin/jst");
            var end = DateTimeOffset.UtcNow;
            long middle = (start.ToUnixTimeMilliseconds() + end.ToUnixTimeMilliseconds()) / 2;
            var mm = reg.Match(timeStr);
            timeStr = mm.Groups[1].Value;
            long actual = (long)(double.Parse(timeStr) * 1000);
            return actual - middle;
        }

    }
}
