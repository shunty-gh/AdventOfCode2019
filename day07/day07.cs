using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
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

            // Tests, part 1
            //initialinput = new int[] {3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0}; // == 43210
            //initialinput = new int[] { 3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0}; // == 54321
            //initialinput = new int[] {3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0}; // == 65210

            var combinations = GeneratePhases(new int[] {0,1,2,3,4});
            var part1 = 0;
            foreach (var phase in combinations)
            {
                ConcurrentQueue<int> inQ = new ConcurrentQueue<int>(), outQ = new ConcurrentQueue<int>();
                var output = 0;
                for (var i = 0; i < 5; i++)
                {
                    inQ.Enqueue(phase[i]);
                    inQ.Enqueue(output);
                    output = IntcodeCompute(initialinput.ToArray(), inQ, outQ);
                }

                if (output > part1)
                {
                    part1 = output;
                }
            }
            Console.WriteLine($"Part 1: {part1}");

            // Part 2

            // Tests, part 2
            //initialinput = new int[] {3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5}; // == 139629729
            //initialinput = new int[] {3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10}; // == 18216

            combinations = GeneratePhases(new int[] {5,6,7,8,9});
            var part2 = 0;
            foreach (var phase in combinations)
            {
                var inQ = Enumerable.Range(0, 5)
                    .Select((_,i) => {
                        var q = new ConcurrentQueue<int>();
                        q.Enqueue(phase[i]);
                        return q;
                    }).ToArray();

                inQ[0].Enqueue(0);
                var tasks = new List<Task<int>>();
                for (var i = 0; i < 5; i++)
                {
                    // Can't do the following due to either timing and/or closure issues - not sure which. But we always get an index out of range exception.
                    //var task = Task.Factory.StartNew(() => IntcodeCompute(initialinput.ToArray(), inQ[i], inQ[(i + 1) % 5]));
                    var inq = inQ[i];
                    var outq = inQ[(i + 1) % 5];
                    var task = Task.Factory.StartNew(() => IntcodeCompute(initialinput.ToArray(), inq, outq));
                    tasks.Add(task);
                }
                //Task.WaitAll(tasks.ToArray());
                var lastoutput = tasks.Last().Result;
                if (lastoutput > part2)
                {
                    part2 = lastoutput;
                }
            }
            Console.WriteLine($"Part 2: {part2}");
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

        private int IntcodeCompute(int[] program, ConcurrentQueue<int> inQ, ConcurrentQueue<int> outQ)
        {
            int lastoutput = 0;
            int ip = 0, len = program.Length;

            while (true)
            {
                int instruction = program[ip];
                int opcode = instruction % 100;
                int m1 = (instruction / 100) % 10,
                    m2 = (instruction / 1000) % 10,
                    m3 = (instruction / 10000) % 10;
                int p1 = ip + 1 < len ? program[ip + 1] : 0,
                    p2 = ip + 2 < len ? program[ip + 2] : 0,
                    p3 = ip + 3 < len ? program[ip + 3] : 0;

                int v1 = p1, v2 = p2;
                if (m1 == 0)
                    v1 = p1 < len ? program[p1] : 0;
                if (m2 == 0)
                    v2 = p2 < len ? program[p2] : 0;

                switch (opcode)
                {
                    case 1: // Add
                        program[p3] = v1 + v2;
                        ip += 4;
                        break;
                    case 2: // Multiply
                        program[p3] = v1 * v2;
                        ip += 4;
                        break;
                    case 3: // Input
                        if (inQ.TryDequeue(out var input))
                        {
                            program[p1] = input;
                            ip += 2;
                        }
                        break;
                    case 4: // Output
                        lastoutput = v1;
                        //Console.WriteLine($"Test result: {v1}");
                        ip += 2;
                        outQ.Enqueue(v1);
                        break;
                    case 5:  // Jump if true
                        if (v1 != 0)
                        {
                            ip = v2;
                        }
                        else
                        {
                            ip += 3;
                        }
                        break;
                    case 6:  // Jump if false
                        if (v1 == 0)
                        {
                            ip = v2;
                        }
                        else
                        {
                            ip += 3;
                        }
                        break;
                    case 7:  // Less than
                        program[p3] = (v1 < v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 8:  // Equals
                        program[p3] = (v1 == v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 99:
                        return lastoutput;
                    default:
                        throw new Exception($"Unknown instruction {opcode} at position {ip}");
                }
            }
        }
    }
}