using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /* Advent of code 2019, day 22
     * https://adventofcode.com/2019/day/22
     *
     * With lots of help, specifically from:
     * https://www.reddit.com/r/adventofcode/comments/ee0rqi/2019_day_22_solutions/
     * https://www.reddit.com/r/adventofcode/comments/eepz2i/2019_day_22_part_2_clean_annotated_solution_in_c/
     * https://gist.github.com/romkatv/8ef7ea27ddce1de7b1b6f9b5a41838c4#file-day-22-2-cc
     * https://github.com/sasa1977/aoc/blob/master/lib/2019/201922.ex
     * https://www.reddit.com/r/adventofcode/comments/eeeixy/remember_the_challenges_arent_here_for_you_to/
     * https://www.reddit.com/r/adventofcode/comments/ee56wh/2019_day_22_part_2_so_whats_the_purpose_of_this/fbqctrr/?utm_source=share&utm_medium=web2x
     * https://codeforces.com/blog/entry/72593
     *
     * https://en.wikipedia.org/wiki/Modular_arithmetic
     *
     * Each shuffle instruction can be boiled down to a function in the form
     *   x => kx + b
     * ie the new position of card x is f(x) after the shuffle.
     * and all the shuffle functions together can be combined together to give
     * an overall k & b
     * All functions modulo m where m is the deck size
     *
     * A "cut" just moves the start offset
     * A "deal into new stack" reverses the order
     * An "increment" provides the k value as a multiple of the current value
     */

    public class Day22 : IAoCRunner
    {
        private static readonly int DayNumber = 22;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber).AsEnumerable();
            //input = GetTestInput();

            var part1 = Part1(input);
            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine();
            var part2 = Part2(input);
            Console.WriteLine($"Part 2: {part2}");
        }

        private Int64 Part1(IEnumerable<string> input)
        {
            //var (k, b) = ProcessInput(input, 10);
            //Console.WriteLine($"K: {k}; B: {b}");
            //
            //foreach (var idx in Enumerable.Range(0, 10))
            //{
            //    Console.WriteLine($"X={idx} => {((k * idx) + b) % 10}");
            //}

            var m = 10007;
            var (a, b) = ProcessInput(input, m);
            Console.WriteLine($"A: {a}; B: {b}");
            // Find where card 2019 has ended up
            //return (a * 2019 + b) % m;
            return modAdd(b, modMultiply(a, 2019, m), m);
        }

        public Int64 Part2(IEnumerable<string> input)
        {
            /* For part 2 we need the modular multiplicative inverse.
            * I had no idea what it was so reddit et al had to help out.
            *
            * The inverse of A modulo m is A^(m-2) modulo m
            * Because, for modulo m: a^m = a
            * Which implies:
            *   => a^(m-1) * a = a
            *   => a^(m-2) * a * a = a
            *   => a^(m-2) * a = 1
            *   => 1/a = a^(m-2)

            * We also need to apply the shuffle many, many times.
            */

            Int64 m = 119315717514047;
            Int64 shuffles = 101741582076661;

            // Bit of shorthand to save typing the modulo param
            Func<Int64, Int64, Int64> add = (a, b) => modAdd(a, b, m);
            Func<Int64, Int64, Int64> mul = (a, b) => modMultiply(a, b, m);
            Func<Int64, Int64, Int64> pow = (a, b) => modPower(a, b, m);

            var (a, b) = ProcessInput(input, m);
            Console.WriteLine($"A: {a}; B: {b}");

            var target = 2020;

            // Now we have a and b we need to apply it many times.
            // This turns out to be a geomteric series and
            // https://codeforces.com/blog/entry/72593
            // https://en.wikipedia.org/wiki/Geometric_progression#Geometric_series
            // shows this sum to be equivalent to a^kx + b(1-a^k)/(1-a) mod m
            // which we turn into aax + bb using the modular inverse of
            //   1/(1-a) = pow(1-a, m-2)
            // This gives us:
            var aa = pow(a, shuffles);
            var bb = mul(add(b, mul(-b, aa)), pow(1-a, m-2));

            // We can now re-arrange the formula x1 = aax + bb to give us
            //   x = (x1 - bb) / aa  where x1 is our target entry of 2020
            // and another application of the modular inverse of
            //   1/aa = pow(aa, m-2)
            // give
            var result = mul(add(target, -bb), pow(aa, m-2));
            return result;

            // This can also be done as follows, but it takes a bit more
            // mental agility which I haven't quite fathomed yet.
            //var x = mul(b, pow(a-1, m-2));  // == (ax+b)^-shuffles   (I think)
            //return add(-x, mul(add(x, target), pow(pow(a, m-2), shuffles)));
        }

        private (Int64 a, Int64 b) ProcessInput(IEnumerable<string> input, Int64 m)
        {
            Int64 a = 1, b = 0;
            foreach (var line in input)
            {
                if (line.StartsWith("deal with inc"))
                {
                    int inc = int.Parse(line.Split(' ').Last());
                    a = modMultiply(a, inc, m);
                    b = modMultiply(b, inc, m);
                }
                else if (line.StartsWith("deal into"))
                {
                    a = modAdd(0, -a, m);
                    b = modAdd(-1, -b, m);
                }
                else if (line.StartsWith("cut"))
                {
                    int cut = int.Parse(line.Split(' ').Last());
                    b = modAdd(b, - cut, m);
                }
                else
                    throw new Exception($"Unknown instruction: {line}");
            }
            return (a, b);
        }

        // As the numbers involved are 64bit and we are multiplying and exponentiating
        // we're going to need some better functions to handle the modulo arithmetic
        // otherwise we'll overflow.

        // C# (and modern C++ I believe) will return -ve values for -ve modulus but we
        // need +ve values for card indexes hence the extra m+ and %m in modAdd below:
        private static Int64 modAdd(Int64 a, Int64 b, Int64 m) => (m + ((a + b) % m)) % m;

        // See https://en.wikipedia.org/wiki/Modular_arithmetic#Example_implementations
        // and https://en.wikipedia.org/wiki/Exponentiation_by_squaring
        //
        // We could combine the modMultiply and modPower into a modCombine
        // and pass in the appropriate (modAdd or modMultiply) function
        // much like https://gist.github.com/romkatv/8ef7ea27ddce1de7b1b6f9b5a41838c4#file-day-22-2-cc
        // But separate functions make it clearer if I ever come back to it.
        private static Int64 modMultiply(Int64 a, Int64 b, Int64 m)
        {
            // Similar to the exponentiation by squaring method
            // if we have (a * b) and b is even then it is equivalent to (a + a) * b/2
            // for odd b we have a + ((a + a) * (b-1)/2)  (or somesuch!)
            if (a < 0) a = (a % m) + m;
            if (b < 0) b = (b % m) + m;
            if (a > m) a %= m;
            if (b > m) b %= m;

            var result = 0L;
            while (b > 0)
            {
                if ((b & 1) > 0)
                {
                    result = modAdd(result, a, m);
                }
                b = b >> 1;
                a = modAdd(a, a, m);
            }
            return result;
        }

        // https://en.wikipedia.org/wiki/Modular_exponentiation#Pseudocode
        private static Int64 modPower(Int64 a, Int64 b, Int64 m)
        {
            var result = 1L;
            while (b > 0)
            {
                if ((b & 1) > 0)
                {
                    result = modMultiply(result, a, m);
                }
                b = b >> 1;
                a = modMultiply(a, a, m);
            }
            return result;
        }
    }
}