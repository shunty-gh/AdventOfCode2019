using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/16

    public class Day16 : IAoCRunner
    {
        private static readonly int DayNumber = 16;

        public void Run(ILogger log)
        {
            var initialInput = AocHelpers.GetDayText(DayNumber)
            //var initialInput = "12345678"
            //var initialInput = "80871224585914546619083218645595" // P1 == 24176176
            //var initialInput = "19617804207202209144916044189917" // P1 == 73745418
            //var initialInput = "69317163492948606335995924319873" // P1 == 52432133
            //var initialInput = "03036732577212944063491565474664"  // P2 == 84462026
            //var initialInput = "02935109699940807407585447034323" // P2 == 78725270
            //var initialInput = "03081770884921959731165446850517" // P2 == 53553731
                .Where(c => c >= '0' && c <= '9')  // To remove extra spaces/blank lines etc
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            var part1 = Part1(initialInput);
            Console.WriteLine($"Part 1: {part1}");
            var part2 = Part2(initialInput);
            Console.WriteLine($"Part 2: {string.Join("", part2)}");
        }

        private string Part1(int[] initialInput)
        {
            var input = initialInput.ToArray();
            var output = new int[input.Length];
            var basepattern = new int[] { 0, 1, 0, -1 };
            var phase = 1;
            var PhaseTotal = 100;

            while (phase <= PhaseTotal)
            {
                var eindex = 0;
                while (eindex < input.Length)
                {
                    var sum = 0;

                    for (var i = 0; i < input.Length; i++)
                    {
                        var pindex = ((i + 1) % ((eindex + 1) * 4)) / (eindex + 1);
                        sum += (input[i] * basepattern[pindex]);
                    }

                    output[eindex] = Math.Abs(sum % 10);
                    eindex++;
                }
                phase++;
                // Swap input and output
                var tmp = input;
                input = output;
                output = tmp;
            }

            return string.Join("", input.Take(8));
        }

        private string Part2(int[] initialInput)
        {
            var len = initialInput.Length;
            var input = new int[len * 10_000];
            for (var ii = 0; ii < 10_000; ii++)
            {
                Array.Copy(initialInput, 0, input, ii * len, len);
            }
            var ilen = input.Length;
            var output = new int[ilen];
            var phase = 1;
            var PhaseTotal = 100;

            var offset = int.Parse(string.Join("", initialInput.Take(7)));
            /* Points to note:
             * * The transformation of any character at position i is dependent
             * only on characters at positions >= i  as, for each element, the
             * pattern starts with i 0s.
             *
             * * Also, output[len-1] = input[len-1]
             *         output[len-2] = input[len-1] + input[len-2]
             *         output[len-3] = input[len-1] + input[len-2] + input[len-3]
             *         ..
             * and so on for element indexes >= len/2
             * (because the first half+ of the pattern will be 0 and the next half will be +1)
             * and, in this puzzle, the offset > half the length so we can ignore
             * all elements up to the offset.
             */
            while (phase <= PhaseTotal)
            {
                var prevsum = 0;
                for (var i = ilen - 1; i >= offset; i--)
                {
                    var sum = (input[i] + prevsum) % 10;
                    output[i] = sum;
                    prevsum = sum;
                }
                phase++;
                // Swap input and output
                var tmp = input;
                input = output;
                output = tmp;
            }

            return string.Join("", input.Skip(offset).Take(8));
        }
    }
}