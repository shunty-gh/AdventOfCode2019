using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Intcode2
    {
        private static int NextId = 0;
        private static readonly ILogger _log = Log.Logger;

        public int Id { get; private set;}
        public bool HasOutput => OutQueue.Count > 0;
        public bool HasInput => InQueue.Count > 0;

        private ConcurrentQueue<Int64> InQueue { get; } = new ConcurrentQueue<Int64>();
        private ConcurrentQueue<(int, Int64)> OutQueue { get; set; } = new ConcurrentQueue<(int, Int64)>();

        public Intcode2()
        {
            Id = NextId++;
        }

        public Intcode2(int id)
        {
            Id = id;
        }

        public void ClearQueues()
        {
            InQueue.Clear();
            OutQueue.Clear();
        }

        public void SetOutputQueue(ConcurrentQueue<(int, Int64)> queue)
        {
            OutQueue = queue;
        }

        public void Enqueue(Int64 value)
        {
            InQueue.Enqueue(value);
        }

        public void Enqueue(IEnumerable<Int64> values)
        {
            foreach (var v in values)
            {
                InQueue.Enqueue(v);
            }
        }

        public bool TryDequeue(out (int, Int64) result)
        {
            return OutQueue.TryDequeue(out result);
        }

        public Int64 Run(Int64[] program)
        {
            try
            {
                return InternalRun(program, InQueue, OutQueue);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error executng Intcode for Id {}", Id);
                throw;
            }
        }

        private Int64 ResizeProgram(ref Int64[] program, Int64 newSize)
        {
            Array.Resize(ref program, (int)newSize);
            return newSize;
        }

        private Int64 InternalRun(Int64[] program, ConcurrentQueue<Int64> inQ, ConcurrentQueue<(int, Int64)> outQ)
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
                        Int64 input = 0;
                        bool dq = false;
                        dq = inQ.TryDequeue(out input);
                        if (!dq)
                        {
                            Thread.Sleep(1);
                        }
                        else
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
                        Output(v1);
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

       private void Output(Int64 value)
       {
            OutQueue.Enqueue((Id, value));
       }
    }
}
