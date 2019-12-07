using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day07 : IAoCRunner
    {
        private static readonly int DayNumber = 7;

        public void Run(ILogger log)
        {
            var initialinput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => int.Parse(s))
                .ToArray();

            // Tests
            //initialinput = new int[] {3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0}; // == 43210
            //initialinput = new int[] { 3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0}; // == 54321
            //initialinput = new int[] {3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0}; // == 65210

            List<int[]> combinations = GeneratePhases(new int[] {0,1,2,3,4});

            var part1 = 0;
            int[] bestphase = null;
            foreach (var phase in combinations)
            {
                var ampa = IntcodeCompute(initialinput.ToArray(), new int[] { phase[0], 0 });
                var ampb = IntcodeCompute(initialinput.ToArray(), new int[] { phase[1], ampa });
                var ampc = IntcodeCompute(initialinput.ToArray(), new int[] { phase[2], ampb });
                var ampd = IntcodeCompute(initialinput.ToArray(), new int[] { phase[3], ampc });
                var ampe = IntcodeCompute(initialinput.ToArray(), new int[] { phase[4], ampd });
                if (ampe > part1)
                {
                    part1 = ampe;
                    bestphase = phase;
                }
            }
            Console.WriteLine($"Part 1: {part1}");
            log.Debug("Best phase (part 1): {BestPhase}", bestphase);

            // Part 2
            combinations = GeneratePhases(new int[] {5,6,7,8,9});
        } 

        private static List<int[]> GeneratePhases(int[] phases)
        {
            var combinations = new List<int[]>();
            foreach (var pa in phases)
            {
                foreach (var pb in phases)
                {
                    foreach (var pc in phases)
                    {
                        foreach (var pd in phases)
                        {
                            foreach (var pe in phases)
                            {
                                if ((pa == pb || pa == pc || pa == pd || pa == pe)
                                  || (pb == pc || pb == pd || pb == pe)
                                  || (pc == pd || pc == pe)
                                  || (pd == pe))
                                {
                                    continue;
                                }
                                var combo = new int[] { pa, pb, pc, pd, pe };
                                combinations.Add(combo);
                            }
                        }
                    }
                }
            }

            return combinations;
        }

        private int IntcodeCompute(int[] program, int[] inputs)
        {
            int lastoutput = 0, inputIndex = 0;
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
                        program[p1] = inputs[inputIndex++];
                        skip = 2;
                        break;
                    case 4: // Output
                        lastoutput = v1;
                        //Console.WriteLine($"Test result: {v1}");
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