using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019.Day20
{
    public class GridLocation
    {
        public Point Point { get; private set; } = new Point(0,0);
        public char Content { get; private set; } = ' ';
        public string PortalId { get; set; } = "";
        public bool IsWall => Content == '#';
        public bool IsPath => Content == '.';
        public bool IsPortal => !string.IsNullOrWhiteSpace(PortalId);
        public bool IsPortalIdentifier => Content >= 'A' && Content <= 'Z';

        public GridLocation(int x, int y, char content)
        {
            Point = new Point(x, y);
            Content = content;
        }
    }

    public class Portal
    {
        public string Id { get; set; } = "";
        public Point Inner { get; set; } = new Point(0,0);
        public Point Outer { get; set; } = new Point(0,0);
    }

    public class Day20 : IAoCRunner
    {
        private static readonly int DayNumber = 20;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var input = AocHelpers.GetDayLines(DayNumber);
            //var input = GetTestInput(); // P2 == 396
            var map = BuildMap(input);
            var portals = FindPortals(map);
            //log.Debug("Portals: {@Portals}", portals);

            var part1 = ShortestPath(map, portals, false);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = ShortestPath(map, portals, true);
            Console.WriteLine($"Part 2: {part2}");
        }

        private int ShortestPath(Dictionary<Point, GridLocation> map, Dictionary<string, Portal> portals, bool useLevels)
        {
            var start = portals["AA"].Outer;
            var end = portals["ZZ"].Outer;

            var result = 0;
            var queue = new Queue<(Point, int, int)>();
            var visited = new Dictionary<(Point, int), int>();
            queue.Enqueue((start, 0, 0));

            while (queue.Count > 0)
            {
                var (visit, level, dist) = queue.Dequeue();
                if (result > 0 && dist >= result)
                    continue;
                if (visited.ContainsKey((visit, level)))
                    continue;
                visited.Add((visit, level), dist);

                // Jump through the portal if we haven't done before on the current level
                if (visit != start && map[visit].IsPortal)
                {
                    var portal = portals[map[visit].PortalId];
                    var nextlevel = !useLevels ? 0 : portal.Inner == visit ? level + 1 : level - 1;
                    // If this is an outer portal then go up a level, o/w go down a level
                    if (portal.Inner == visit && !visited.ContainsKey((portal.Outer, nextlevel)))
                    {
                        queue.Enqueue((portal.Outer, nextlevel, dist + 1));
                    }
                    else if (portal.Outer == visit && level > 0 && !visited.ContainsKey((portal.Inner, nextlevel))) // Can only go through outer portals on levels > 0
                    {
                        queue.Enqueue((portal.Inner, nextlevel, dist + 1));
                    }
                }

                foreach (var next in new Point[] {
                    new Point(visit.X, visit.Y - 1),
                    new Point(visit.X + 1, visit.Y),
                    new Point(visit.X, visit.Y + 1),
                    new Point(visit.X - 1, visit.Y)
                    })
                {
                    if (!map.ContainsKey(next) || !map[next].IsPath || visited.ContainsKey((next, level)))
                        continue;
                    // Check for AA and ZZ on inner levels - they're walls
                    if ((level > 0) && (next == start || next == end))
                        continue;

                    if (next == end) // Found it
                    {
                        result = dist + 1;
                        break;
                    }
                    queue.Enqueue((next, level, dist + 1));
                }
            }
            return result;
        }

        private Dictionary<Point, GridLocation> BuildMap(IList<string> input)
        {
            var result = new Dictionary<Point, GridLocation>();
            var y = 0;
            foreach (var line in input)
            {
                var x = 0;
                foreach (var c in line)
                {
                    if (c != ' ')
                    {
                        var loc = new GridLocation(x,y,c);
                        result[loc.Point] = loc;
                    }
                    x++;
                }
                y++;
            }
            return result;
        }

        private Dictionary<string, Portal> FindPortals(Dictionary<Point, GridLocation> map)
        {
            var pathElements = map.Where(k => k.Value.IsPath);
            var minX = pathElements.Min(k => k.Key.X);
            var minY = pathElements.Min(k => k.Key.Y);
            var maxX = pathElements.Max(k => k.Key.X);
            var maxY = pathElements.Max(k => k.Key.Y);

            var result = new Dictionary<string, Portal>();
            foreach (var (k1, g1) in map.Where(k => k.Value.IsPortalIdentifier).OrderBy(k => k.Key.Y).ThenBy(k => k.Key.X))
            {
                if (!string.IsNullOrWhiteSpace(g1.PortalId)) // Already processed
                    continue;

                // Due to the sorting of the map locations the second letter of the portal
                // will always be one space right or one line below
                foreach (var d in new (int dx, int dy)[] {(1,0), (0,1)})
                {
                    var k2 = new Point(k1.X + d.dx, k1.Y + d.dy);
                    if (!map.ContainsKey(k2))
                        continue;

                    if (map[k2].IsPortalIdentifier)
                    {
                        var g2 = map[k2];
                        var portalid = $"{g1.Content}{g2.Content}";
                        // Find the gateway - the only '.' next to either one of the portal letters
                        foreach (var gk in new Point[] {
                          new Point(k1.X, k1.Y - 1),
                          new Point(k2.X, k2.Y + 1),
                          new Point(k1.X - 1, k1.Y),
                          new Point(k2.X + 1, k2.Y) })
                        {
                            if (map.ContainsKey(gk) && map[gk].IsPath)
                            {
                                map[gk].PortalId = portalid;
                                // Inner or outer portal?
                                var isouter = gk.X == minX || gk.X == maxX || gk.Y == minY || gk.Y == maxY;

                                Portal portal;
                                if (result.ContainsKey(portalid))
                                {
                                    portal = result[portalid];
                                }
                                else
                                {
                                    portal = new Portal { Id = portalid };
                                    result.Add(portalid, portal);
                                }
                                if (isouter)
                                    portal.Outer = gk;
                                else
                                    portal.Inner = gk;
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private IList<string> GetTestInput() // For part 2
        {
            return new List<string> {
                "             Z L X W       C                 ",
                "             Z P Q B       K                 ",
                "  ###########.#.#.#.#######.###############  ",
                "  #...#.......#.#.......#.#.......#.#.#...#  ",
                "  ###.#.#.#.#.#.#.#.###.#.#.#######.#.#.###  ",
                "  #.#...#.#.#...#.#.#...#...#...#.#.......#  ",
                "  #.###.#######.###.###.#.###.###.#.#######  ",
                "  #...#.......#.#...#...#.............#...#  ",
                "  #.#########.#######.#.#######.#######.###  ",
                "  #...#.#    F       R I       Z    #.#.#.#  ",
                "  #.###.#    D       E C       H    #.#.#.#  ",
                "  #.#...#                           #...#.#  ",
                "  #.###.#                           #.###.#  ",
                "  #.#....OA                       WB..#.#..ZH",
                "  #.###.#                           #.#.#.#  ",
                "CJ......#                           #.....#  ",
                "  #######                           #######  ",
                "  #.#....CK                         #......IC",
                "  #.###.#                           #.###.#  ",
                "  #.....#                           #...#.#  ",
                "  ###.###                           #.#.#.#  ",
                "XF....#.#                         RF..#.#.#  ",
                "  #####.#                           #######  ",
                "  #......CJ                       NM..#...#  ",
                "  ###.#.#                           #.###.#  ",
                "RE....#.#                           #......RF",
                "  ###.###        X   X       L      #.#.#.#  ",
                "  #.....#        F   Q       P      #.#.#.#  ",
                "  ###.###########.###.#######.#########.###  ",
                "  #.....#...#.....#.......#...#.....#.#...#  ",
                "  #####.#.###.#######.#######.###.###.#.#.#  ",
                "  #.......#.......#.#.#.#.#...#...#...#.#.#  ",
                "  #####.###.#####.#.#.#.#.###.###.#.###.###  ",
                "  #.......#.....#.#...#...............#...#  ",
                "  #############.#.#.###.###################  ",
                "               A O F   N                     ",
                "               A A D   M                     ",
            };
        }
    }
}