using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day11 : IAoCRunner
    {
        private static readonly int DayNumber = 11;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            ConcurrentQueue<Int64> inQ = new ConcurrentQueue<Int64>(), outQ = new ConcurrentQueue<Int64>();
            var t = Task.Factory.StartNew(() => IntcodeCompute(input.ToArray(), inQ, outQ));
            var map = new Dictionary<(int X, int Y), (int Count, int Colour)>();
            (int X, int Y) current = (0, 0);
            var facing = 0;  // [^,>,v,<]
            inQ.Enqueue(0);
            while (!t.IsCompleted)
            {
                while (outQ.TryDequeue(out var colour))
                {
                    // Set the colour for the location
                    if (map.ContainsKey(current))
                    {
                        var item = map[current];
                        map[current] = (item.Count + 1, (int)colour);
                    }
                    else
                    {
                        map[current] = (1, (int)colour);
                    }

                    // Wait for a new direction
                    var done = false;
                    Int64 direction = 0;
                    while (!outQ.TryDequeue(out direction))
                    {
                        done = t.IsCompleted;
                    }

                    if (!done)
                    {
                        // Move on
                        switch ((int)direction)
                        {
                            case 0:  // left
                                facing = (facing + 3) % 4;
                                break;
                            case 1:  // right
                                facing = (facing + 1) % 4;
                                break;
                            default:
                                throw new Exception($"Unknown direction instruction {direction} while at ({current.X},{current.Y})");
                        }
                        switch (facing)
                        {
                            case 0: // up
                                current = (current.X, current.Y - 1);
                                break;
                            case 1: // right
                                current = (current.X + 1, current.Y);
                                break;
                            case 2: // down
                                current = (current.X, current.Y + 1);
                                break;
                            case 3: // left
                                current = (current.X - 1, current.Y);
                                break;
                            default:
                                throw new Exception($"Unknown facing direction {facing} while at ({current.X},{current.Y})");
                        }
                        // Put the current colour into the queue
                        if (!t.IsCompleted)
                        {
                            if (map.ContainsKey(current))
                                inQ.Enqueue(map[current].Colour);
                            else
                                inQ.Enqueue(0);
                        }
                    }
                }
            }

            Console.WriteLine($"Part 1: {map.Count}");
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