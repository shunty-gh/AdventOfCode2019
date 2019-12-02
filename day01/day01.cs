using System;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Day01 : IAoCRunner
    {
        private static readonly int DayNumber = 1;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber)
                .Select(l => int.Parse(l));

            Func<int, int> FuelRequired = (mass) => (int)(Math.Truncate(mass / 3.0)) - 2;
            var part1 = 0;
            var part2 = 0;
            foreach (var i in input)
            {
                var fuel = FuelRequired(i);
                part1 += fuel;
                part2 += fuel;

                while (fuel > 0)
                {
                    fuel = FuelRequired(fuel);
                    if (fuel > 0)
                    part2 += fuel;
                }
            }
            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine($"Part 2: {part2}");
        }
    }
}