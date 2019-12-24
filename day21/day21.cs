using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/21
    public class Day21 : IAoCRunner
    {
        private static readonly int DayNumber = 21;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var initialInput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var part1 = DoPart(initialInput, 1);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = DoPart(initialInput, 2);
            Console.WriteLine($"Part 2: {part2}");
       }

        private Int64 DoPart(Int64[] initialInput, int part)
        {
            var computer = new Intcode();
            var t = Task.Factory.StartNew(() => computer.Run(initialInput.ToArray()));
            Int64 output = 0;
            var sb = new StringBuilder();

            InputInstructions(computer, GetInstructions(part));
            while (!t.IsCompleted)
            {
                while (!computer.TryDequeue(out output) && !t.IsCompleted) { }
                if (output > 0 && output <= 255)
                {
                    sb.Append((char)output);
                    if (output == 10)
                    {
                        _log.Debug("$> {Prompt}", sb.ToString());
                        sb.Clear();
                    }
                }
            }

            return t.Result;
        }

        private void InputInstructions(Intcode computer, IList<string> instructions)
        {
            foreach (var cmd in instructions)
            {
                foreach (var ch in cmd)
                {
                    computer.Enqueue((int)ch);
                }
                computer.Enqueue(10);
            }
        }

        private IList<string> GetInstructions(int part)
        {
            if (part == 1)
            {
                // if !A then J
                // if !D then !J
                // if !C then J   (&& D implied from above)
                // !A || !C && D
                // => !(A && C) && D
                return new List<string> {
                    "OR A J",
                    "AND C J",
                    "NOT J J",
                    "AND D J",
                    "WALK",
                };
            }
            else if (part == 2)
            {
                // if !A then J
                // if !H && !E then !J
                // if !D || !(E || H) then !J
                // if !B || !C => J
                //
                // (!A || !B || !C) && D && (E || H)
                // !(A && B && C) && D && (E || H)

                return new List<string> {
                    "OR A J",
                    "AND B J",
                    "AND C J",
                    "NOT J J",
                    "AND D J",
                    "OR E T",
                    "OR H T",
                    "AND T J",
                    "RUN",
                };
            }
            else
                throw new Exception($"Unknown part id {part}");
        }
    }
}