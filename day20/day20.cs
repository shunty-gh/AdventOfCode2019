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
        public char PortalLetterA { get; set; } = ' ';
        public char PortalLetterB { get; set; } = ' ';
        public string PortalId { get; set; } = "";
        public bool IsWall => Content == '#';
        public bool IsPath => Content == '.';
        public bool IsPortal => !string.IsNullOrWhiteSpace(PortalId);
        public bool IsPortalIdentifier => Content >= 'A' && Content <= 'Z';

        public GridLocation(int x, int y, char content)
        {
            Point = new Point(x, y);
            Content = content;
            if (IsPortalIdentifier)
            {
                PortalLetterA = Content;
            }
        }
    }

    public class Portal
    {
        public string Id { get; set; } = "";
        public Point PointA { get; set; } = new Point(0,0);
        public Point PointB { get; set; } = new Point(0,0);
    }

    public class Day20 : IAoCRunner
    {
        private static readonly int DayNumber = 20;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var input = AocHelpers.GetDayLines(DayNumber);
            var map = BuildMap(input);
            var portals = FindPortals(map);
            //log.Debug("Portals: {@Portals}", portals);

            var part1 = Part1(portals["AA"].PointA, portals["ZZ"].PointA, map, portals);
            Console.WriteLine($"Part 1: {part1}");
        }

        private int Part1(Point start, Point end, Dictionary<Point, GridLocation> map, Dictionary<string, Portal> portals)
        {
            var result = 0;
            var queue = new Queue<(Point, int)>();
            var visited = new Dictionary<Point, int>();
            queue.Enqueue((start, 0));

            while (queue.Count > 0)
            {
                var (visit, dist) = queue.Dequeue();
                if (visited.ContainsKey(visit))
                    continue;
                visited.Add(visit, dist);

                // Jump through the portal if we haven't done before
                if (map[visit].IsPortal)
                {
                    var gl = map[visit];
                    var portal = portals[gl.PortalId];
                    var next = portal.PointA == visit ? portal.PointB : portal.PointA;
                    if (map.ContainsKey(next) && !visited.ContainsKey(next))
                    {
                        queue.Enqueue((next, dist + 1));
                    }
                }

                foreach (var next in new Point[] {
                    new Point(visit.X, visit.Y - 1),
                    new Point(visit.X + 1, visit.Y),
                    new Point(visit.X, visit.Y + 1),
                    new Point(visit.X - 1, visit.Y)
                    })
                {
                    if (!map.ContainsKey(next) || !map[next].IsPath || visited.ContainsKey(next))
                        continue;

                    if (next == end)
                    {
                        result = dist + 1;
                    }
                    queue.Enqueue((next, dist + 1));
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
            var result = new Dictionary<string, Portal>();
            foreach (var (k1, g1) in map.Where(k => k.Value.IsPortalIdentifier).OrderBy(k => k.Key.Y).ThenBy(k => k.Key.X))
            {
                if (g1.PortalLetterB != ' ') // Already processed
                    continue;

                // Due to the sorting of the map locations the second letter of the portal
                // will always be one space right or one below
                foreach (var d in new (int dx, int dy)[] {(1,0), (0,1)})
                {
                    var k2 = new Point(k1.X + d.dx, k1.Y + d.dy);
                    if (!map.ContainsKey(k2))
                        continue;

                    if (map[k2].IsPortalIdentifier)
                    {
                        var g2 = map[k2];
                        g1.PortalLetterB = g2.PortalLetterA;
                        g2.PortalLetterB = g1.PortalLetterA;

                        var portalid = $"{g1.PortalLetterA}{g1.PortalLetterB}";
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
                                if (result.ContainsKey(portalid))
                                {
                                    result[portalid].PointB = gk;
                                }
                                else
                                {
                                    result[portalid] = new Portal { Id = portalid, PointA = gk };
                                }
                                break;
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.Assert(result.Count(p => p.Value.PointB.IsEmpty) == 2, "Should be exactly 2 unmatched portals, AA and ZZ.");
            //_log.Debug("Unmatched portals: {@UnmatchedPortals}", result.Where(p => p.Value.PointB.IsEmpty));
            return result;
        }
    }

}