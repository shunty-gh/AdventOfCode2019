using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019.Day18x
{
    // https://adventofcode.com/2019/day/18

    public class Day18x : IAoCRunner
    {
        private static readonly int DayNumber = 18;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber);
            //input = GetTestInput1();  // P1 == 8
            //input = GetTestInput2();  // P1 == 86
            //input = GetTestInput3();  // P1 == 132
            input = GetTestInput4();  // P1 == 136
            //input = GetTestInput5();  // P1 == 81
            var map = BuildMap(input);
            //log.Debug("Map: {@Map}", map);
            //log.Debug("Map: {@Map}", map.Where(k => k.Value.IsDoor));
            //log.Debug("Map: {@Map}", map.Where(k => k.Value.IsKey));

            // Build an index of the key locations
            var keys = map.Where(k => k.Value.IsKey)
                .ToDictionary(k => k.Value.Content, k => k.Key);
            var keycount = keys.Count;
            log.Debug("Keys: {@Keys}", keys);

            var current = map.First(k => k.Value.IsEntrance).Key;
            var tovisit = new Queue<(Point Point, List<PathItem> Path)>();
            tovisit.Enqueue((current, new List<PathItem> { new PathItem { Point = current, KeyCount = 0 }}));

            var directions = new (int dx, int dy)[] {(0,-1), (1,0), (0,1), (-1,0)}; // N,E,S,W with origin top-left and +ve Y goes downwards
            while (tovisit.Count > 0)
            {
                var visiting = tovisit.Dequeue();
                var kc = visiting.Path?.LastOrDefault().KeyCount ?? 0;
                // If this location is a key we need to check if we have them all yet
                var loc = map[visiting.Point];
                if (loc.IsKey)
                {
                    if (kc == keycount)
                    {
                        log.Debug($"Got all keys after {visiting.Path.Count} steps");
                        // No need to continue searching from this location
                        continue;
                    }
                }

                foreach (var dir in directions)
                {
                    var nextp = new Point(loc.Point.X + dir.dx, loc.Point.Y + dir.dy);
                    // Can't visit walls or points that aren't on the map
                    if (map.ContainsKey(nextp) && !map[nextp].IsWall)
                    {
                        var next = map[nextp];
                        var nextkc = kc;

                        // Have we already visited it on this path
                        if (visiting.Path.Any(p => p.Point == nextp))
                        {
                            // If we still have the same number of keys as last time we
                            // were here then don't go back
                            var pp = visiting.Path.Last(p => p.Point == nextp);
                            if (pp.KeyCount >= kc)
                                continue;
                        }
                        else if (next.IsKey)
                        {
                            // Only increment the key count if this is a key we
                            // haven't already collected
                            nextkc += 1;
                        }

                        // If it is a door then we need to check that we already have the key
                        // ie if our path has visited the location containing the key
                        if (next.IsDoor)
                        {
                            var keyloc = keys[next.RequiredKey];
                            // if (!visiting.Path.Any(p => p.Point == keyloc))
                            // {
                            //     // Can't visit this place, we haven't got the key yet
                            //     continue;
                            // }
                        }

                        // ...otherwise add it to the places to visit
                        var path = new List<PathItem>();
                        path.AddRange(visiting.Path);
                        path.Add(new PathItem { Point = next.Point, KeyCount = nextkc });
                        tovisit.Enqueue((next.Point, path));
                    }
                }
            }
        }

        private Dictionary<Point, Location> BuildMap(IList<string> input)
        {
            var result = new Dictionary<Point, Location>();
            var y = 0;
            foreach (var line in input)
            {
                var x = 0;
                foreach (var c in line)
                {
                    var loc = new Location(x, y, c);
                    result.Add(loc.Point, loc);
                    x++;
                }
                y++;
            }
            return result;
        }

        private IList<string> GetTestInput1()
        {
            return new List<string> {
                "#########",
                "#b.A.@.a#",
                "#########",
            };
        }

        private IList<string> GetTestInput2()
        {
            return new List<string> {
                "########################",
                "#f.D.E.e.C.b.A.@.a.B.c.#",
                "######################.#",
                "#d.....................#",
                "########################",
            };
        }

        private IList<string> GetTestInput3()
        {
            return new List<string> {
                "########################",
                "#...............b.C.D.f#",
                "#.######################",
                "#.....@.a.B.c.d.A.e.F.g#",
                "########################",
            };
        }

        private IList<string> GetTestInput4()
        {
            return new List<string> {
                "#################",
                "#i.G..c...e..H.p#",
                "########.########",
                "#j.A..b...f..D.o#",
                "########@########",
                "#k.E..a...g..B.n#",
                "########.########",
                "#l.F..d...h..C.m#",
                "#################",
            };
        }

        private IList<string> GetTestInput5()
        {
            return new List<string> {
                "########################",
                "#@..............ac.GI.b#",
                "###d#e#f################",
                "###A#B#C################",
                "###g#h#i################",
                "########################",
            };
        }
    }

    public struct PathItem
    {
        public Point Point;
        public int KeyCount;
    }

    public class Location
    {
        public Point Point { get; private set; } = new Point(0,0);
        public char Content { get; private set; } = ' ';

        public bool IsEntrance => Content == '@';
        public bool IsDoor => Content >= 'A' && Content <= 'Z';
        public bool IsKey => Content >= 'a' && Content <= 'z';
        public bool IsWall => Content == '#';
        public char RequiredKey => IsDoor ? (char)(Content + 32) : ' ';

        public Location(int x, int y, char content)
        {
            Point = new Point(x,y);
            Content = content;
        }
    }
}