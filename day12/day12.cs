using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day12 : IAoCRunner
    {
        private static readonly int DayNumber = 12;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var input = AocHelpers.GetDayLines(DayNumber)
            //var input = GetTestInput1()  // P1 (10 steps) == 179
            //var input = GetTestInput2()  // P1 (100 steps) == 1940
                .Select(l => Moon.FromInput(l))
                .ToList();

            //input.ForEach(m => _log.Debug("Moon: {@Moon}", m));

            // Generate the pair combinations
            var pairs = new List<(int Moon1, int Moon2)>();
            for (var pairA = 0; pairA < input.Count - 1; pairA++)
            {
                for (var pairB = pairA + 1; pairB < input.Count; pairB++)
                {
                    pairs.Add((pairA, pairB));
                }
            }
            _log.Debug("Pairs {@MoonPairs}", pairs);

            for (var step = 1; step <= 1000; step++)
            {
                foreach (var pair in pairs)
                {
                    // Apply gravity
                    var m1 = input[pair.Moon1];
                    var m2 = input[pair.Moon2];

                    if (m1.X > m2.X)
                    {
                        m1.Vx--;
                        m2.Vx++;
                    }
                    else if (m1.X < m2.X)
                    {
                        m1.Vx++;
                        m2.Vx--;
                    }

                    if (m1.Y > m2.Y)
                    {
                        m1.Vy--;
                        m2.Vy++;
                    }
                    else if (m1.Y < m2.Y)
                    {
                        m1.Vy++;
                        m2.Vy--;
                    }

                    if (m1.Z > m2.Z)
                    {
                        m1.Vz--;
                        m2.Vz++;
                    }
                    else if (m1.Z < m2.Z)
                    {
                        m1.Vz++;
                        m2.Vz--;
                    }
                }
                foreach (var moon in input)
                {
                    // Apply velocity
                    moon.X = moon.X + moon.Vx;
                    moon.Y = moon.Y + moon.Vy;
                    moon.Z = moon.Z + moon.Vz;
                }
                if (step == 10)
                    input.ForEach(m => _log.Debug("Step {StepCount}: {@Moon}",step, m));
            }

            Console.WriteLine($"Part 1: {input.Sum(m => m.TotalEnergy)}");
        }

        private string[] GetTestInput1()
        {
            return new string[] {
                "<x=-1, y=0, z=2>",
                "<x=2, y=-10, z=-7>",
                "<x=4, y=-8, z=8>",
                "<x=3, y=5, z=-1>",
            };
        }

        private string[] GetTestInput2()
        {
            return new string[] {
                "<x=-8, y=-10, z=0>",
                "<x=5, y=5, z=10>",
                "<x=2, y=-7, z=3>",
                "<x=9, y=-8, z=-3>",
            };
        }
    }

    public class Moon
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;

        public int Vx { get; set; } = 0;
        public int Vy { get; set; } = 0;
        public int Vz { get; set; } = 0;

        public int PE => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
        public int KE => Math.Abs(Vx) + Math.Abs(Vy) + Math.Abs(Vz);
        public int TotalEnergy => PE * KE;

        public static Moon FromInput(string s)
        {
            var s1 = s.Trim(new char[] { '<', '>', ' ' })
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Substring(s.IndexOf('=') + 1)))
                .ToArray();

            var result = new Moon { X = s1[0], Y = s1[1], Z = s1[2] };
            return result;
        }
    }
}