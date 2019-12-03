using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Day03 : IAoCRunner
    {
        private static readonly int DayNumber = 3;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber)
                .Select(l => l.Split(',').ToList())
                .ToList();

            var path1 = MapPath(input[0]);
            var path2 = MapPath(input[1]);

            var intersection = path1       // Could use .Intersect() here with a custom
                .Join(                     // IEqualityComparer<Location>. But why bother
                    path2,                 // when .Join() works fine.
                    p => (p.X, p.Y),
                    p => (p.X, p.Y),
                    (p1, p2) => new { X = p1.X, Y = p1.Y, Distance = p1.Distance, Steps = p1.Steps + p2.Steps }
                );

            var part1 = intersection.OrderBy(p => p.Distance).First();
            Console.WriteLine($"Part 1: {part1.Distance} (X={part1.X}; Y={part1.Y})");
            var part2 = intersection.OrderBy(p => p.Steps).First();
            Console.WriteLine($"Part 2: {part2.Steps} (X={part2.X}; Y={part2.Y})");
        }

        internal struct Location
        {
            public int X;
            public int Y;
            public int Steps;
            public int Distance => Math.Abs(X) + Math.Abs(Y);
        }

        private List<Location> MapPath(IEnumerable<string> path)
        {
            // Walk through each instruction and add a location to the result for every
            // grid point visited, including number of steps to get there.
            var result = new List<Location>();
            int cx = 0, cy = 0, steps = 0;
            foreach (var inst in path)
            {
                var direction = inst[0];
                var distance = int.Parse(inst.Substring(1));
                foreach (var d in Enumerable.Range(1, distance))
                {
                    switch (direction)
                    {
                        case 'D':
                            cy--;
                            break;
                        case 'U':
                            cy++;
                            break;
                        case 'L':
                            cx--;
                            break;
                        case 'R':
                            cx++;
                            break;
                        default:
                            throw new Exception($"Unknown direction \"{direction}\"");
                    }
                    result.Add(new Location { X = cx, Y = cy, Steps = ++steps });
                }
            }
            return result;
        }
    }
}