using System;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Day04 : IAoCRunner
    {
        public void Run(ILogger log)
        {
            // Puzzle input
            const int Lower = 372304;
            const int Upper = 847060;

            int part1 = 0, part2 = 0;
            var range = Enumerable.Range(Lower, Upper - Lower);
            foreach (var num in range)
            {
                var pwd = num.ToString();

                bool incrementing = true, haspair = false, hasdistinctpair = false;
                for (var index = 1; index < 6; index++)
                {
                    var left = pwd[index - 1];
                    var right = pwd[index];

                    // Check incrementing digits
                    if (right < left)
                    {
                        incrementing = false;
                        break;
                    }

                    // Check that L/R pair do not have neighbours that are the same
                    if (left == right && !hasdistinctpair)
                    {
                        haspair = true;
                        if (index > 1)
                        {
                            var leftneighbour = pwd[index - 2];
                            if (leftneighbour == left)
                                continue;
                        }
                        if (index < 5)
                        {
                            var rightneighbour = pwd[index + 1];
                            if (rightneighbour == right)
                                continue;
                        }
                        hasdistinctpair = true;
                    }
                }

                part1 += incrementing && haspair ? 1 : 0;
                part2 += incrementing && hasdistinctpair ? 1 : 0;
            }

            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine($"Part 2: {part2}");
        }
    }
}