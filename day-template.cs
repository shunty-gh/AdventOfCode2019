using System;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day00 : IAoCRunner
    {
        private static readonly int DayNumber = 0;

        public void Run(ILogger log)
        {
            var inputlines = AocHelpers.GetDayLines(DayNumber);
            foreach (var line in inputlines)
            {
                // Do some stuff
            }

            // or
            var inputtext = AocHelpers.GetDayText(DayNumber);
            foreach (var ch in inputtext)
            {
                // Do some stuff
            }

            log.Information("Day {DayNumber} : Result is {Result}", DayNumber, "UNDEFINED");
        }
    }
}