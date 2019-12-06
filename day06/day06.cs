using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Day06 : IAoCRunner
    {
        private static readonly int DayNumber = 6;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber)
            // Part 1 test == 42
            // var input = new List<string> { "COM)B","B)C","C)D","D)E","E)F","B)G","G)H","D)I","E)J","J)K","K)L" }
            // Part 2 test == 4
            // var input = new List<string> { "COM)B","B)C","C)D","D)E","E)F","B)G","G)H","D)I","E)J","J)K","K)L","K)YOU","I)SAN" }
                .Select(l => l.Split(')'));

            // Build the map/tree
            var map = new Dictionary<string, Node>();
            foreach (var pair in input)
            {
                string pname = pair[0], cname = pair[1];

                if (!map.TryGetValue(pname, out var parent))
                {
                    parent = new Node { Name = pname };
                    map[pname] = parent;
                }

                if (!map.TryGetValue(cname, out var child))
                {
                    child = new Node { Name = cname };
                    map[cname] = child;
                }
                child.Parent = parent;
            }

            // Part 1 - Count the number of parents back up the tree for each node
            var part1 = 0;
            foreach (var kvp in map)
            {
                var parent = kvp.Value.Parent;
                while (parent != null)
                {
                    part1++;
                    parent = parent.Parent;
                }
            }
            Console.WriteLine($"Part 1: {part1}");

            // Part 2
            // Find the first common node along their indirect orbits back to COM
            // Count SAN + YOU moves to the common node
            var sorbits = new List<string>();
            var sparent = map["SAN"].Parent;
            while (sparent != null)
            {
                sorbits.Add(sparent.Name);
                sparent = sparent.Parent;
            }
            int ycount = 0, scount = 0;
            var yparent = map["YOU"].Parent;
            while (true)
            {
                if (sorbits.Contains(yparent.Name))
                {
                    // Found the common node
                    scount = sorbits.IndexOf(yparent.Name);
                    break;
                }
                ycount++;
                yparent = yparent.Parent;
            }
            Console.WriteLine($"Part 2: {ycount + scount}");
        }

        public class Node
        {
            public Node Parent { get; set; }
            public string Name { get; set; }
        }
    }
}