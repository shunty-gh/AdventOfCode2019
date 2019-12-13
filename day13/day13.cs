using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public struct Location
    {
        public int X;
        public int Y;

        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Tile
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Id { get; set; } = 0;
    }

    public class Day13 : IAoCRunner
    {
        const int Empty = 0;
        const int Wall = 1;
        const int Block = 2;
        const int Paddle = 3;
        const int Ball = 4;

        private static readonly int DayNumber = 13;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            ConcurrentQueue<Int64> inQ = new ConcurrentQueue<Int64>(), outQ = new ConcurrentQueue<Int64>();
            var t = Task.Factory.StartNew(() => IntcodeCompute(input.ToArray(), inQ, outQ));
            var map = new Dictionary<Location, Tile>();
            while (!t.IsCompleted || outQ.Count() > 0)
            {
                // Get 3 values from the queue
                Int64 x = 0, y = 0, id = 0;
                while (!outQ.TryDequeue(out x))
                {}
                while (!outQ.TryDequeue(out y))
                {}
                while (!outQ.TryDequeue(out id))
                {}

                var key = new Location((int)x, (int)y);
                if (map.ContainsKey(key))
                {
                    var tile = map[key];
                    tile.Id = (int)id;
                }
                else
                {
                    map[key] = new Tile { X = (int)x, Y = (int)y, Id = (int)id };
                }
            }

            var grouped = map.GroupBy(m => m.Value.Id).Select(g => (g.Key, g.Count())).OrderBy(g => g.Key).ToList();
            log.Debug("Tile types: {@TileTypes}", grouped);
            var part1 = map.Count(kvp => kvp.Value.Id == Block);
            Console.WriteLine($"Part 1: {part1}");
        }

        private Int64 ResizeProgram(ref Int64[] program, Int64 newSize)
        {
            Array.Resize(ref program, (int)newSize);
            return newSize;
        }

        private Int64 IntcodeCompute(Int64[] program, ConcurrentQueue<Int64> inQ, ConcurrentQueue<Int64> outQ)
        {
            Int64 lastoutput = 0, relativebase = 0, outaddr = 0;
            Int64 ip = 0, len = program.Length;

            while (true)
            {
                var instruction = program[ip];
                if (instruction == 99)
                    return lastoutput;

                Int64 opcode = instruction % 100;
                Int64 m1 = (instruction / 100) % 10,
                    m2 = (instruction / 1000) % 10,
                    m3 = (instruction / 10000) % 10;
                Int64 p1 = ip + 1 < len ? program[ip + 1] : 0,
                    p2 = ip + 2 < len ? program[ip + 2] : 0,
                    p3 = ip + 3 < len ? program[ip + 3] : 0;

                Int64 v1 = p1, v2 = p2;
                if (m1 == 0)
                    v1 = (p1 < len) ? program[p1] : 0;
                else if (m1 == 2)
                    v1 = (p1 + relativebase) < len ? program[p1 + relativebase] : 0;

                if (m2 == 0)
                    v2 = p2 < len ? program[p2] : 0;
                else if (m2 == 2)
                    v2 = (p2 + relativebase) < len ? program[p2 + relativebase] : 0;

                switch (opcode)
                {
                    case 1: // Add
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = v1 + v2;
                        ip += 4;
                        break;
                    case 2: // Multiply
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = v1 * v2;
                        ip += 4;
                        break;
                    case 3: // Input
                        if (inQ.TryDequeue(out var input))
                        {
                            outaddr = m1 == 0 ? p1 : p1 + relativebase;
                            if (outaddr >= len)
                                len = ResizeProgram(ref program, outaddr * 2);
                            program[outaddr] = input;
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
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = (v1 < v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 8:  // Equals
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = (v1 == v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 9: // Update relative base
                        relativebase += v1;
                        ip += 2;
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
