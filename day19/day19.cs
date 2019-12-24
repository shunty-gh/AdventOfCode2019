using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/19
    public class Day19 : IAoCRunner
    {
        private static readonly int DayNumber = 19;

        public void Run(ILogger log)
        {
             var initialInput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var part1 = Part1(initialInput);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = Part2(initialInput);
            Console.WriteLine($"Part 2: {part2}");
       }

        private Int64 Part1(Int64[] initialInput)
        {
            // No need to read the output queue we only want the final answer
            var computer = new Intcode();
            Int64 count = 0;
            for (var y = 0; y < 50; y++)
            {
                for (var x = 0; x < 50; x++)
                {
                    computer.Enqueue(x);
                    computer.Enqueue(y);
                    count += computer.Run(initialInput.ToArray());
                    computer.ClearQueues();
                }
            }
            return count;
        }

        private Int64 Part2(Int64[] initialInput)
        {
            var computer = new Intcode();
            int xstart = 0, x = 0, y = 100; // Skip the first few rows
            var found = false;
            while (!found)
            {
                x = xstart;

                // Skip 0s at the beginning of the line
                Int64 state = 0;
                while (state == 0)
                {
                    xstart = x;
                    computer.Enqueue(x);
                    computer.Enqueue(y);
                    state = computer.Run(initialInput.ToArray());
                    computer.ClearQueues();
                    if (state == 0)
                    {
                        x++;
                    }
                }

                while (state == 1)
                {
                    computer.Enqueue(x + 99);
                    computer.Enqueue(y);
                    state = computer.Run(initialInput.ToArray());
                    computer.ClearQueues();

                    if (state == 0)
                    {
                        // Not wide enough, move down a line
                        break;
                    }
                    else
                    {
                        // See if it fits downwards
                        computer.Enqueue(x);
                        computer.Enqueue(y + 99);
                        var ystate = computer.Run(initialInput.ToArray());
                        computer.ClearQueues();
                        if (ystate == 1)
                        {
                            found = true;
                            break;
                        }
                    }

                    x++;
                    computer.Enqueue(x);
                    computer.Enqueue(y);
                    state = computer.Run(initialInput.ToArray());
                    computer.ClearQueues();
                }
                if (!found)
                {
                    y++;
                }
            }

            return (x * 10_000) + y;
        }
    }
}