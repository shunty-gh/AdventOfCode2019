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
                var days = new List<int>();
                if (args.Length > 1 && (args[1] == "*" || args[1] == "-*" || args[1] == "--all")) // Show all. Can't use '*' by itself on Linux. Doing do passes a directory listing to the program!
                {
                    days.AddRange(Enumerable.Range(1, 25));
                }
                else if ((args.Length <= 1))
                {
                    var dt = DateTime.Today;
                    // If during the AoC event period then default to the current day
                    if (dt.Year == 2019 && dt.Month == 12)
                    {
                        days.Add(dt.Day);
                    }
                    else
                    {
                        // otherwise just add day one
                        days.Add(1);
                    }
                }
                else
                {
                    foreach (var arg in args.Skip(1))
                    {
                        if (int.TryParse(arg, out var day) && day >= 1 && day <= 25)
                            days.Add(day);
                        else
                            log.Warning("Invalid command line input {InvalidArg}. Ignoring it.", arg);
                    }
                }

                //foreach (var day in new int[] {15})
                foreach (var day in days)
                {

                    var typ = Type.GetType($"Shunty.AdventOfCode2019.Day{day:D2}");
                    if (typ != null)
                    {
                        log.Debug("Attempting to run day {AoCDay}", day);
                        var dayclass = (IAoCRunner)Activator.CreateInstance(typ);
                        dayclass.Run(log);
                        Console.WriteLine();
                    }
                    else
                    {
                        var dt = DateTime.Today;
                        if (dt.Year == 2019 && dt.Month == 12 && day > dt.Day)
                        {
                            // Haven't got this far in the festive season yet. Do nothing.
                        }
                        else
                        {
                            log.Debug($"Code for day {day} does not exist.");
                        }
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
