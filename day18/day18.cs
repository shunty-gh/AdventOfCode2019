using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019.Day18
{
    // https://adventofcode.com/2019/day/18

    /* With help from
     * https://github.com/KanegaeGabriel/advent-of-code-2019/blob/master/day18.py
     * https://github.com/Starwort/advent-of-code-2019/commits/master/day18.py
     * https://github.com/orez-/Advent-of-Code-2019/blob/master/day18/cleanup.py
     */

    public class Day18 : IAoCRunner
    {
        private static readonly int DayNumber = 18;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber);

            var vault = new VaultMap(input);
            var part1 = vault.DistanceToAllKeys();
            Console.WriteLine($"Part 1: {part1}");

            // Part 2: Update the map source and generate a new vault
            var p1robot = vault.Robots.First();
            foreach (var (dx, dy) in new (int, int)[] { (1,-1), (1,1), (-1,1), (-1,-1) } )
            {
                var chs = input[p1robot.Y + dy].ToCharArray();
                chs[p1robot.X + dx] = '@';
                input[p1robot.Y + dy] = string.Concat(chs);
            }
            foreach (var (dx, dy) in new (int, int)[] { (0,0), (0,-1), (1,0), (0,1), (-1,0) } )
            {
                var chs = input[p1robot.Y + dy].ToCharArray();
                chs[p1robot.X + dx] = '#';
                input[p1robot.Y + dy] = string.Concat(chs);
            }
            vault = new VaultMap(input);
            var part2 = vault.DistanceToAllKeys();
            Console.WriteLine($"Part 2: {part2}");
        }
    }

    internal class VaultMap
    {
        private int _maxX;
        private int _maxY;
        private readonly Dictionary<Point, char> _locations = new Dictionary<Point, char>();
        private readonly Dictionary<char, Point> _keys = new Dictionary<char, Point>();
        public IList<Point> Robots { get; } = new List<Point>();
        private Dictionary<char, Dictionary<char, (int Distance, string Doors)>> _keyDistances;

        public char At(Point p) => p.X >= 0 && p.Y >= 0 && p.X < _maxX && p.Y < _maxY ? _locations[p] : ' ';

        public bool IsKey(char ch) => ch >= 'a' && ch <= 'z';
        public bool IsDoor(char ch) => ch >= 'A' && ch <= 'Z';
        public bool CanVisit(char ch) => ch != '#' && ch != ' ';
        public bool IsRobot(char ch) => ch == '@';
        public char KeyForDoor(char door) => Char.ToLower(door);

        public VaultMap(IList<string> input)
        {
            _maxX = input[0].Length;
            _maxY = input.Count;
            // Parse the map content
            foreach (var y in Enumerable.Range(0, _maxY))
            {
                foreach (var x in Enumerable.Range(0, _maxX))
                {
                    var ch = input[y][x];
                    var key = new Point(x,y);
                    _locations[key] = ch;
                    if (IsKey(ch))
                    {
                        _keys[ch] = key;
                    }
                    else if (IsRobot(ch))
                    {
                        // Because, in part 2, we have more than one robot ie more than one @ symbol
                        // we need to give the robots unique ids. A simple approach is jst to use
                        // the next robot array index as we know there will only be 4 max ie a single
                        // digit.
                        var robotid = (char)(Robots.Count + '0');
                        _keys[(char)robotid] = key;
                        Robots.Add(key);
                    }
                }
            }
        }

        public int DistanceToAllKeys()
        {
            _keyDistances = GetKeyDistances();
            var cache = new Dictionary<(string, string), int>();
            return InternalDistanceToAllKeys(Robots.ToArray(), "", cache);
        }

        /// <summary>
        /// Determine the distance between each key and every other key and include all the
        /// doors that we need to pass through to get there.
        /// </summary>
        private Dictionary<char, Dictionary<char, (int, string)>> GetKeyDistances()
        {
            var keyDistances = new Dictionary<char, Dictionary<char, (int, string)>>();
            foreach (var (thisKey, thisPoint) in _keys)
            {
                var tovisit = new Queue<(Point, int, string)>();
                var visited = new HashSet<Point>();
                keyDistances[thisKey] = new Dictionary<char, (int, string)>();

                tovisit.Enqueue((thisPoint, 0, ""));
                while (tovisit.Count > 0)
                {
                    var (visiting, dist, doors) = tovisit.Dequeue();

                    if (visited.Contains(visiting))
                        continue;
                    visited.Add(visiting);

                    var nextdoors = doors;
                    var ch = At(visiting);
                    if (IsKey(ch) && ch != thisKey)
                    {
                        keyDistances[thisKey][ch] = (dist, doors);
                        if (!keyDistances.ContainsKey(ch))
                            keyDistances[ch] = new Dictionary<char, (int, string)>();
                        keyDistances[ch][thisKey] = (dist, doors);
                    }
                    else if (IsDoor(ch))
                    {
                        nextdoors += ch;
                    }

                    foreach (var (dx, dy) in new (int, int)[] { (0, -1), (1, 0), (0, 1), (-1, 0) })
                    {
                        var next = new Point(visiting.X + dx, visiting.Y + dy);
                        var nextch = At(next);
                        if (CanVisit(nextch))
                        {
                            tovisit.Enqueue((next, dist + 1, nextdoors));
                        }
                    }
                }
            }
            return keyDistances;
        }

        private int InternalDistanceToAllKeys(Point[] currentRobotLocations, string collectedKeys, Dictionary<(string, string), int> cache)
        {
            // If we don't cache pos/keys => distance then this will take a *very* long time
            // Create appropriate cache keys. Assume collectedKeys is already sorted (it needs to be).
            var k1 = string.Join(";", currentRobotLocations.OrderBy(p => p.Y).ThenBy(p => p.X).Select(p => $"{p.Y},{p.X}"));
            if (cache.ContainsKey((k1, collectedKeys)))
                return cache[(k1, collectedKeys)];

            var distances = new List<int>();
            var robotid = 0;
            foreach (var robot in currentRobotLocations)
            {
                var ch = At(robot);
                var kdid = ch;
                if (IsRobot(ch)) // 'cos a robot is '@' but we need the id of the robot for the _keyDistances key
                    kdid = (char)(robotid + '0');

                foreach (var (key, (dist, doors)) in _keyDistances[kdid])
                {
                    if (!IsKey(key)) // it might be a robot location, not a key. If so, we're not interested
                        continue;

                    if (collectedKeys.Contains(key)) // We've already got this key, move on
                        continue;

                    // Check we have all the necessary keys to pass through all the doors to this next location
                    var requiredkeys = doors.Select(d => KeyForDoor(d));
                    if (!requiredkeys.All(k => collectedKeys.Contains(k)))
                        continue;

                    // Move to the location of this key and add the key to our list of collected keys and resort the keys
                    var newlocations = currentRobotLocations.ToArray(); // NB Take a COPY of the locations
                    newlocations[robotid] = _keys[key];
                    var newkeys = string.Concat((collectedKeys + key).OrderBy(c => c));

                    // Recurse
                    distances.Add(dist + InternalDistanceToAllKeys(newlocations, newkeys, cache));
                }
                robotid++;
            }

            // Find the minimum distance then cache and return it
            var result = distances.Count > 0 ? distances.Min() : 0;
            cache[(k1, collectedKeys)] = result;
            return result;
        }
    }
 }