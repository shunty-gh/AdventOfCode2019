using System;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    public class Day02 : IAoCRunner
    {
        private static readonly int DayNumber = 2;

        public void Run(ILogger log)
        {
            var initialinput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => int.Parse(s))
                .ToArray();

            bool p1found = false, p2found = false;
            var p2target = 19690720;

            for (var noun = 0; noun <= 99; noun++)
            {
                for (var verb = 0; verb <= 99; verb++)
                {
                    var input = initialinput.ToArray();
                    IntcodeCompute(input, noun, verb);
                    // Check it
                    if (noun == 12 && verb == 2) // Part 1 inputs
                    {
                        p1found = true;
                        Console.WriteLine($"Part 1: {input[0]}");
                    }
                    if (input[0] == p2target)
                    {
                        p2found = true;
                        Console.WriteLine($"Part 2: {input[0]}, Noun: {noun}, Verb: {verb}, Result: {(100 * noun) + verb}");
                    }

                    // Quit when we can
                    if (p1found && p2found)
                        return;
                }
            }
        }

        private void IntcodeCompute(int[] input, int noun, int verb)
        {
            var finished = false;
            var ip = 0;
            var skip = 4;
            var len = input.Length;
            input[1] = noun;
            input[2] = verb;

            while (!finished)
            {
                var opcode = input[ip];
                int p1 = ip + 1 < len ? input[ip + 1] : 0,
                    p2 = ip + 2 < len ? input[ip + 2] : 0,
                    p3 = ip + 3 < len ? input[ip + 3] : 0;

                switch (opcode)
                {
                    case 1: // Add
                        input[p3] = input[p1] + input[p2];
                        break;
                    case 2: // Multiply
                        input[p3] = input[p1] * input[p2];
                        break;
                    case 99:
                        finished = true;
                        break;
                    default:
                        throw new Exception($"Unknown instruction {opcode} at position {ip}");
                }
                ip += skip;
            }
        }
    }
}