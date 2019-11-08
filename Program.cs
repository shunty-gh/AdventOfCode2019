using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = InitialiseLogging();
            //log.Debug("Args: {Args}", args);

            try
            {
                var days = new List<string>();
                if (args.Length <= 1)
                {
                    var dt = DateTime.Today;
                    // If during the AoC event period then default to the current day
                    if (dt.Year == 2019 && dt.Month == 12)
                    {
                        days.Add(dt.Day.ToString());
                    }
                    else
                    {
                        // otherwise then just add day one
                        days.Add("1");
                    }
                }
                else
                {
                    days.AddRange(args.Skip(1));
                }

                foreach (var day in days)
                {
                    if (int.TryParse(day, out var dayno))
                    {
                        log.Debug("Attempting to run day {AoCDay}", dayno);
                        var dayclass = (IAoCRunner)Activator.CreateInstance(Type.GetType($"Shunty.AdventOfCode2019.Day{dayno:D2}"));
                        dayclass.Run(log);
                    }
                    else
                    {
                        log.Warning("Invalid command line input {InvalidArg}. Ignoring it.", day);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error running AoC with args: {@Args}", args);
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        private static Serilog.ILogger InitialiseLogging(bool writeToSeq = true)
        {
            const string SeqLocal = "http://localhost:5341";

            var sconfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.Udp("localhost", 7071, System.Net.Sockets.AddressFamily.InterNetwork, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                ;

            if (writeToSeq)
                sconfig.WriteTo.Seq(SeqLocal);

            Serilog.Log.Logger = sconfig.CreateLogger();
            return Serilog.Log.Logger;
        }
    }
}
