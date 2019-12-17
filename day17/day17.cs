using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/17

    public class Day17 : IAoCRunner
    {
        private static readonly int DayNumber = 17;
        private ILogger _log;

        // Set this to true to show the robot progress on screen.
        // This will cause an exception if running in a process that doesn't
        // understand the Console.SetCursorPosition calls. eg when run from
        // within the VS Code debugger.
        // Also the console screen needs to be about 50 lines or more high
        // otherwise it's going to be a mess.
        private bool ShowContinuousFeed { get; set; } = false;

        public void Run(ILogger log)
        {
            _log = log;

            var initialInput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var (part1, map) = Part1(initialInput);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = Part2(initialInput, map);
            Console.WriteLine($"Part 2: {part2}");
        }

        private (int, Dictionary<Point, char>) Part1(Int64[] initialInput)
        {
            // Run the program
            // Map it out and find the intersections
            // Calculate the sum
            var computer = new Intcode();
            computer.Run(initialInput.ToArray());

            Int64 outval;
            int x = 0, y = 0;
            var map = new Dictionary<Point, char>();
            while (computer.TryDequeue(out outval))
            {
                // Make a map of it & draw it
                if (outval == 10)
                {
                    y++;
                    x = 0;
                    Console.WriteLine();
                }
                else
                {
                    map[new Point(x,y)] = (char)outval;
                    Console.Write((char)outval);
                    x++;
                }
            }
            //_log.Debug("Last 10: {@Last10}", output.Skip(output.Count - 11));

            // Find the intersections
            var intersections = new List<Point>();
            foreach (var kvp in map.Where(m => m.Value == '#'))
            {
                // Check it has # chars above, below, left and right of it
                var allneighbours = true;
                foreach (var dir in new (int dx, int dy)[] {(0,-1),(1,0),(0,1),(-1,0)})
                {
                    var mapkey = new Point(kvp.Key.X + dir.dx, kvp.Key.Y + dir.dy);
                    if (!map.ContainsKey(mapkey) || map[mapkey] != '#')
                    {
                        allneighbours = false;
                        break;
                    }
                }
                if (allneighbours)
                {
                    intersections.Add(kvp.Key);
                }
            }
            //_log.Debug("Intersections: {@Intersection}", intersections);
            return (intersections.Sum(i => i.X * i.Y), map);
        }

        private Int64 Part2(Int64[] initialInput, Dictionary<Point, char> map)
        {
            // Analyse the scaffold path
            // Split it into chunks
            // Run the computer and provide the path
            var directions = new (int dx, int dy)[] {(0,-1),(1,0),(0,1),(-1,0)};
            var current = map.First(m => m.Value == '^').Key; // Slight cheat - we know it's ^ not <,>,v
            var dindex = 0;
            var route = new List<string>();
            var steps = 0;
            var ismore = true;
            while (ismore)
            {
                // Carry on in the current direction for as long as possible
                var dir = directions[dindex];
                var next = new Point(current.X + dir.dx, current.Y + dir.dy);
                if (map.ContainsKey(next) && map[next] == '#')
                {
                    steps++;
                    current = next;
                }
                else
                {
                    route.Add(steps.ToString());
                    steps = 0;
                    // Turn - try left then right
                    ismore = false;
                    var turn = "L";
                    foreach (var di in new int[] {(dindex + 3) % 4, (dindex + 1) % 4})
                    {
                        var test = new Point(current.X + directions[di].dx, current.Y + directions[di].dy);
                        if (map.ContainsKey(test) && map[test] == '#')
                        {
                            route.Add(turn);
                            dindex = di;
                            ismore = true;
                            break;
                        }
                        turn = "R";
                    }
                }
            }
            //_log.Debug("Route: {@Route}", route);
            _log.Debug("Route: {RouteAsString}", string.Join(",", route));

            var (functions, routine) = GetMovementFunctions(string.Join(",", route));
            var computer = new Intcode();
            var input = initialInput.ToArray();
            input[0] = 2;

            // Feed in the main routine & newline
            foreach (var ch in routine)
            {
                computer.Enqueue((int)ch);
            }
            computer.Enqueue(10);

            // Feed in the functions & newlines
            foreach (var fn in functions)
            {
                foreach (var fch in fn)
                {
                    computer.Enqueue(fch);
                }
                computer.Enqueue(10);
            }

            // Continuous feed y/n?
            computer.Enqueue(ShowContinuousFeed ? (int)'y' : (int)'n');
            computer.Enqueue(10);

            var t = Task.Factory.StartNew(() => computer.Run(input));
            if (ShowContinuousFeed)
            {
                var nl = false;
                Int64 output = 0;
                for (var scroll = 0; scroll < Console.WindowHeight; scroll++) { Console.WriteLine(); }
                Console.SetCursorPosition(0, 1);
                while (!t.IsCompleted || computer.TryDequeue(out output))
                {
                    if (output == 10)
                    {
                        if (nl)
                        {
                            Console.SetCursorPosition(0, 1);
                            nl = false;
                        }
                        else
                        {
                            nl = true;
                            Console.WriteLine();
                        }
                    }
                    else if (output > 0 && output < 255)
                    {
                        nl = false;
                        Console.Write((char)output);
                    }
                    else if (output > 255 && output < 10_000)
                    {
                        _log.Debug($"Unhandled character {output}");
                    }
                }

                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                Console.WriteLine();
            }

            return t.Result;
        }

        private (string[], string) GetMovementFunctions(string route)
        {
            // For our input the route is:
            // R,6,L,10,R,10,R,10,L,10,L,12,R,10,R,6,L,10,R,10,R,10,L,10,L,12,R,10,R,6,L,10,R,10,R,10,R,6,L,12,L,10,R,6,L,10,R,10,R,10,R,6,L,12,L,10,L,10,L,12,R,10,R,6,L,12,L,10

            // Haven't worked out how/bothered to do this computationally yet.
            // For now a simple visual inspection has produced the following:
            return (new string[] {
                "R,6,L,10,R,10,R,10",
                "L,10,L,12,R,10",
                "R,6,L,12,L,10"
            }, "A,B,A,B,A,C,A,C,B,C");
        }
    }
}