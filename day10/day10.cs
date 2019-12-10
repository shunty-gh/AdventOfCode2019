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
            //Input = GetTestInput();  // p1 == 210; p2 == 802
            int y = 0;
            foreach (var line in Input)
            {
                int x = 0;
                foreach (var c in line)
                {
                    if (c == '#')
                    {
                        var cansee = Process(x, y, Input);
                        visibles.Add((x, y), cansee);
                    }
                    x++;
                }
                y++;
            }

            var stationlocation = visibles.OrderByDescending(v => v.Value.Count).First();
            var part1 = stationlocation.Value.Count();
            Console.WriteLine($"Part 1: {part1} at ({stationlocation.Key.X},{stationlocation.Key.Y})");

            // Part 2
            // Because the number of asteroids in sight is greater than our target of 200 then
            // we'll not need to bother with a second pass of the laser.
            // So this very abridged version will do:
            var ast = stationlocation.Value.OrderBy(v => v.Angle).ToArray()[199];
            var part2 = (ast.X * 100) + ast.Y;

            // But I didn't twig until I'd written the whole processing loop... duh!

            // visibles.Clear();
            // int part2 = 0;
            // int orgX = stationlocation.Key.X, orgY = stationlocation.Key.Y, destructionCount = 0;
            // var p2input = Input.ToArray();
            // var asteroids = stationlocation.Value;
            // while (destructionCount < 200)
            // {
            //     // Remove each asteroid from the input then re-process the station location
            //     var ordered = asteroids.OrderBy(a => a.Angle);
            //     foreach (var asteroid in ordered)
            //     {
            //         destructionCount++;
            //         if (destructionCount == 200)
            //         {
            //             part2 = (asteroid.X * 100) + asteroid.Y;
            //             break;
            //         }
            //         var line = p2input[asteroid.Y];
            //         var chs = line.ToCharArray();
            //         chs[asteroid.X] = '-';
            //         line = new string(chs);
            //         p2input[asteroid.Y] = line;
            //     }
            //     asteroids = Process(orgX, orgY, p2input);
            // }

            Console.WriteLine($"Part 2: {part2}");
        }

        private double Angle(int x, int y, int x1, int y1)
        {
            // For this puzzle we need 0° to be up the vertical, y axis, increasing upwards
            // and angles to increase clockwise
            // However our grid uses an upside down y axis with origin at top left

            var dx = x1 - x;
            var dy = y - y1;

            var result = Math.Atan2(dx, dy);
            // Make sure we're dealing with +ve angles only
            if (result < 0)
                result += (Math.PI * 2);
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

        private void CheckPoint(char ch, int x, int y, int x1, int y1, IList<(int X, int Y, double Angle)> cansee)
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
                    cansee.Add((x1,y1, Flip180(p.First().Angle)));
                }
            }
            else
            {
                // Is there already something in the line of sight
                var ang = Angle(x, y, x1, y1);
                if (!cansee.Any(i => i.Angle == ang))
                {
                    cansee.Add((x1, y1, ang));
                }
            }
        }

        private List<(int X, int Y, double Angle)> Process(int x, int y, string[] input)
        {
            var cansee = new List<(int X, int Y, double Angle)>();
            for (var y1 = y; y1 >= 0; y1--)
            {
                var line = input[y1];
                for (var x1 = x + 1; x1 < MaxX; x1++)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y - 1; y1 >= 0; y1--)
            {
                var line = input[y1];
                for (var x1 = x; x1 >= 0; x1--)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y; y1 < MaxY; y1++)
            {
                var line = input[y1];
                for (var x1 = x - 1; x1 >= 0; x1--)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            for (var y1 = y + 1; y1 < MaxY; y1++)
            {
                var line = input[y1];
                for (var x1 = x; x1 < MaxX; x1++)
                {
                    CheckPoint(line[x1], x, y, x1, y1, cansee);
                }
            }

            return cansee;
        }

        private string[] GetTestInput()
        {
            return new string[] {
                ".#..##.###...#######",
                "##.############..##.",
                ".#.######.########.#",
                ".###.#######.####.#.",
                "#####.##.#.##.###.##",
                "..#####..#.#########",
                "####################",
                "#.####....###.#.#.##",
                "##.#################",
                "#####.##.###..####..",
                "..######..##.#######",
                "####.##.####...##..#",
                ".#####..#.######.###",
                "##...#.##########...",
                "#.##########.#######",
                ".####.#.###.###.#.##",
                "....##.##.###..#####",
                ".#.#.###########.###",
                "#.#.#.#####.####.###",
                "###.##.####.##.#..##",
            };
        }
    }
}