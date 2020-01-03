using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shunty.AdventOfCode2019
{
    public static class AocHelpers
    {
        public static string DayPath(int day)
        {
            return $"day{day:D2}";
        }

        public static IList<string> GetDayLines(int day, int part = 0)
        {
            var parttxt = part > 0 ? $"-part{part}" : "";
            var fname = $"day{day:D2}{parttxt}-input.txt";
            var inputname = Path.Combine(DayPath(day), fname);

            var result = File.ReadAllLines(inputname)
                .ToList();
            return result;
        }

        public static string GetDayText(int day, int part = 0)
        {
            var parttxt = part > 0 ? $"-part{part}" : "";
            var fname = $"day{day:D2}{parttxt}-input.txt";
            var inputname = Path.Combine($"day{day:D2}", fname);

            var result = File.ReadAllText(inputname);
            return result;
        }
    }
}
