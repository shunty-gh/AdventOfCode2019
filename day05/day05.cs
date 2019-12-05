using System;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day05 : IAoCRunner
    {
        private static readonly int DayNumber = 5;

        public void Run(ILogger log)
        {
            var initialinput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => int.Parse(s))
                .ToArray();

            // Part 1 test
            //IntcodeCompute(new int[] {1002,4,3,4,33}, 0);
            // Part 2 test
            // var test = IntcodeCompute(new int[] {
            //     3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,
            //     1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,
            //     999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99 }, 9); // == 999 if input < 8; 1000 if 8; 1001 if > 8
            // Console.WriteLine($"Test: {test}");

            var part1 = IntcodeCompute(initialinput.ToArray(), 1);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = IntcodeCompute(initialinput.ToArray(), 5);
            Console.WriteLine($"Part 2: {part2}");
        }

        private int IntcodeCompute(int[] program, int input)
        {
            var lastoutput = 0;
            var finished = false;
            int ip = 0, skip = 0, len = program.Length;

            while (!finished)
            {
                int instruction = program[ip];
                int opcode = instruction % 100;
                int m1 = (instruction / 100) % 10,
                    m2 = (instruction / 1000) % 10,
                    m3 = (instruction / 10000) % 10;
                int p1 = ip + 1 < len ? program[ip + 1] : 0,
                    p2 = ip + 2 < len ? program[ip + 2] : 0,
                    p3 = ip + 3 < len ? program[ip + 3] : 0;

                int v1 = m1 == 0 && p1 < len ? program[p1] : p1,
                    v2 = m2 == 0 && p2 < len ? program[p2] : p2;

                switch (opcode)
                {
                    case 1: // Add
                        program[p3] = v1 + v2;
                        skip = 4;
                        break;
                    case 2: // Multiply
                        program[p3] = v1 * v2;
                        skip = 4;
                        break;
                    case 3: // Input
                        program[p1] = input;
                        skip = 2;
                        break;
                    case 4: // Output
                        lastoutput = v1;
                        Console.WriteLine($"Test result: {v1}");
                        skip = 2;
                        break;
                    case 5:  // Jump if true
                        if (v1 != 0)
                        {
                            ip = v2;
                            skip = 0;
                        }
                        else
                        {
                            skip = 3;
                        }
                        break;
                    case 6:  // Jump if false
                        if (v1 == 0)
                        {
                            ip = v2;
                            skip = 0;
                        }
                        else
                        {
                            skip = 3;
                        }
                        break;
                    case 7:  // Less than
                        program[p3] = (v1 < v2) ? 1 : 0;
                        skip = 4;
                        break;
                    case 8:  // Equals
                        program[p3] = (v1 == v2) ? 1 : 0;
                        skip = 4;
                        break;
                    case 99:
                        finished = true;
                        break;
                    default:
                        throw new Exception($"Unknown instruction {opcode} at position {ip}");
                }
                ip += skip;
            }

            return lastoutput;
        }
    }
}