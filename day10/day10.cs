using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day10 : IAoCRunner
    {
        private static readonly int DayNumber = 10;
        private ILogger _log;

        public string[] Input { get; private set; } = new string[] {};
        private int MaxX => Input[0].Length;
        private int MaxY => Input.Count();


        public void Run(ILogger log)
        {
            _log = log;
            Input = AocHelpers.GetDayLines(DayNumber).ToArray();
            int y = 0;
            foreach (var line in Input)
            {
                int x = 0;
                foreach (var c in line)
                {
                    if (c == '#')
                    {
                        Process(x, y);
                    }
                    x++;
                }
                y++;
            }

            var stationlocation = visibles.OrderByDescending(v => v.Value.Count).First();
            var part1 = stationlocation.Value.Count();
            Console.WriteLine($"Part 1: {part1} at ({stationlocation.Key.X},{stationlocation.Key.Y})");
        }

        private double Angle(int dx, int dy)
        {
            var result = Math.Atan2(dy, dx);
            // Make sure we're dealing with +ve angles only
            if (result < 0)
                result += (Math.PI * 2);
            // But, for this puzzle we want 0 to be pointing up, so we need to subtract 90°
            if (result >= (Math.PI / 2))
                result -= (Math.PI / 2);
            else
                result += (Math.PI * 3 / 2);
            return result;
        }

        private double Flip180(double rads)
        {
            if (rads < Math.PI)
                return rads + Math.PI;
            else
                return rads - Math.PI;
        }

        private Dictionary<(int X,int Y), List<(int X,int Y, double Angle)>> visibles = new Dictionary<(int X, int Y), List<(int X, int Y, double Angle)>>();

        private void CheckPoint(char ch, int x, int y, int x1, int y1, IList<(int X, int Y, double Angle)> seen)
        {
            if (ch != '#')
                return;

            // Have we already processed this point
            if (visibles.ContainsKey((x1, y1)))
            {
                // Can this point see us
                var v = visibles[(x1,y1)];
                var p = v.Where(vv => vv.X == x && vv.Y == y);
                if (p.Count() > 0)
                {
                    seen.Add((x1,y1, Flip180(p.First().Angle)));
                }
            }
            else
            {
                // Is there already something in the line of sight
                var dx = x1 - x;
                var dy = y1 - y;
                var ang = Angle(dy, dx);
                if (!seen.Any(i => i.Angle == ang))
                {
                    seen.Add((x1, y1, ang));
                }
            }
        }

        private void Process(int x, int y)
        {
            var cansee = new List<(int X, int Y, double Angle)>();
            for (var y1 = y; y1 >= 0; y1--)
            {
                var line = Input[y1];
                for (var x1 = x + 1; x1 < MaxX; x1++)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y - 1; y1 >= 0; y1--)
            {
                var line = Input[y1];
                for (var x1 = x; x1 >= 0; x1--)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y; y1 < MaxY; y1++)
            {
                var line = Input[y1];
                for (var x1 = x - 1; x1 >= 0; x1--)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y + 1; y1 < MaxY; y1++)
            {
                var line = Input[y1];
                for (var x1 = x; x1 < MaxX; x1++)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            visibles.Add((x,y), cansee);
        }
    }
}