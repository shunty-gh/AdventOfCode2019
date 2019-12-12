using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day12 : IAoCRunner
    {
        private static readonly int DayNumber = 12;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var moons = AocHelpers.GetDayLines(DayNumber)
            //var moons = GetTestInput1()  // P1 (10 steps) == 179; P2 == 2772
            //var moons = GetTestInput2()  // P1 (100 steps) == 1940; P2 == 4686774924
                .Select(l => Moon.FromInput(l))
                .ToList();
            //moons.ForEach(m => _log.Debug("Moon: {@Moon}", m));

            // Generate the pair combinations
            var pairs = new List<(int Moon1, int Moon2)>();
            for (var pairA = 0; pairA < moons.Count - 1; pairA++)
            {
                for (var pairB = pairA + 1; pairB < moons.Count; pairB++)
                {
                    pairs.Add((pairA, pairB));
                }
            }
            //_log.Debug("Pairs {@MoonPairs}", pairs);

            int part1steps = 1000, part1 = 0;
            int step = 1;
            int periodX = 0, periodY = 0, periodZ = 0;
            while (periodX == 0 || periodY == 0 || periodZ == 0)
            {
                foreach (var pair in pairs)
                {
                    // Apply gravity
                    var m1 = moons[pair.Moon1];
                    var m2 = moons[pair.Moon2];

                    if (m1.X > m2.X)
                    {
                        m1.Vx--;
                        m2.Vx++;
                    }
                    else if (m1.X < m2.X)
                    {
                        m1.Vx++;
                        m2.Vx--;
                    }

                    if (m1.Y > m2.Y)
                    {
                        m1.Vy--;
                        m2.Vy++;
                    }
                    else if (m1.Y < m2.Y)
                    {
                        m1.Vy++;
                        m2.Vy--;
                    }

                    if (m1.Z > m2.Z)
                    {
                        m1.Vz--;
                        m2.Vz++;
                    }
                    else if (m1.Z < m2.Z)
                    {
                        m1.Vz++;
                        m2.Vz--;
                    }
                }

                /* For part 2 we need to notice that the x, y, and z axes are independent
                 * of each other.
                 * We cannot run the sequence until completion as it will take an eternity
                 * so instead of waiting until all axes are back to their intial state we
                 * need to find out the number of steps for each individual axis to return
                 * to its initial state and then take the lowest common multiple of the
                 * three axis step values.
                 */
                bool checkX = true, checkY = true, checkZ = true; // For checking if each dimension is back
                                                                  // to initial state for all moons
                foreach (var moon in moons)
                {
                    // Apply velocity
                    moon.X += moon.Vx;
                    moon.Y += moon.Vy;
                    moon.Z += moon.Vz;

                    // Check if the axes are back to their initial state
                    if (!(moon.X == moon.InitialState.X && moon.Vx == 0))
                        checkX = false;
                    if (!(moon.Y == moon.InitialState.Y && moon.Vy == 0))
                        checkY = false;
                    if (!(moon.Z == moon.InitialState.Z && moon.Vz == 0))
                        checkZ = false;
                }

                // If a particular axis is back to initial state for all the moons
                // then record the number of steps taken (if we haven't already)
                if (checkX && (periodX == 0))
                {
                    periodX = step;
                    //_log.Debug("Match found for X at step {Step}", step);
                }
                if (checkY && periodY == 0)
                {
                    periodY = step;
                    //_log.Debug("Match found for Y at step {Step}", step);
                }
                if (checkZ && periodZ == 0)
                {
                    periodZ = step;
                    //_log.Debug("Match found for Z at step {Step}", step);
                }

                // Part 1 check
                if (step == part1steps)
                {
                    part1 = moons.Sum(m => m.TotalEnergy);
                }
                step++;
            }

            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine($"Part 2 period X: {periodX} (factors: {string.Join(',',GetFactors(periodX))})");
            Console.WriteLine($"       period Y: {periodY} (factors: {string.Join(',',GetFactors(periodY))})");
            Console.WriteLine($"       period Z: {periodZ} (factors: {string.Join(',',GetFactors(periodZ))})");
            Console.WriteLine($"Part 2: {GetLCM(new List<int> {periodX, periodY, periodZ})}");
        }

        /// <summary>
        /// Given a list of values, return the least common multiple for them.
        /// <a href="https://en.wikipedia.org/wiki/Least_common_multiple">https://en.wikipedia.org/wiki/Least_common_multiple</a>
        /// </summary>
        private Int64 GetLCM(List<int> values)
        {
            // * Get the factors for each value and group them by factor along
            //   with their power (ie number of occurrences in the list of factors).
            // * Union all the factor & power pairs toether
            // * Group the resulting list by factor again and pick the highest
            //   power for each factor.
            // * Multiply them together

            var allfactors = new List<(int Value, int Power)>();
            foreach (var p in values)
            {
                var factors = GetFactors(p);
                var g = factors.GroupBy(f => f)
                    .Select(f => (f.Key, f.Count()));
                allfactors.AddRange(g);
            }

            var highestpowers = allfactors.GroupBy(g => g.Value)
                .Select(g => new { Value = g.Key, Power = g.Max(v => v.Power) })
                .OrderByDescending(g => g.Value);

            var lcm = highestpowers.Aggregate(1L, (acc, v) => acc * (Int64)Math.Pow(v.Value, v.Power));
            return lcm;
        }

        /// <summary>
        /// Return a list of all the prime factors of the given value.
        /// <para>eg 8 -> {2,2,2};<br/></para>
        /// <para>    108 -> {2,2,3,3,3}</para>
        /// </summary>
        private List<int> GetFactors(int num)
        {
            var result = new List<int>();
            var max = (int)(Math.Ceiling(Math.Sqrt(num))) + 1;
            var current = num;
            var factor = 2;
            while (factor < max)
            {
                if (current == 1)
                {
                    break;
                }
                else if (factor > current)
                {
                    result.Add(current);
                    break;
                }

                if (current % factor == 0)
                {
                    result.Add(factor);
                    current = current / factor;
                }
                else
                {
                    factor++;
                }

                if (factor == max)
                {
                    result.Add(current);
                }
            }

            if (result.Count == 0)
            {
                // Add the original if no other factors
                result.Add(num);
            }
            return result;
        }

        private string[] GetTestInput1()
        {
            return new string[] {
                "<x=-1, y=0, z=2>",
                "<x=2, y=-10, z=-7>",
                "<x=4, y=-8, z=8>",
                "<x=3, y=5, z=-1>",
            };
        }

        private string[] GetTestInput2()
        {
            return new string[] {
                "<x=-8, y=-10, z=0>",
                "<x=5, y=5, z=10>",
                "<x=2, y=-7, z=3>",
                "<x=9, y=-8, z=-3>",
            };
        }
    }

    public class Moon
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;

        public int Vx { get; set; } = 0;
        public int Vy { get; set; } = 0;
        public int Vz { get; set; } = 0;

        public int PE => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
        public int KE => Math.Abs(Vx) + Math.Abs(Vy) + Math.Abs(Vz);
        public int TotalEnergy => PE * KE;

        public (int X, int Y, int Z) InitialState { get; } = (0,0,0);

        public Moon(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            InitialState = (x,y,z);
        }

        public static Moon FromInput(string s)
        {
            var s1 = s.Trim(new char[] { '<', '>', ' ' })
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Substring(s.IndexOf('=') + 1)))
                .ToArray();

            return new Moon(s1[0], s1[1], s1[2]);
        }
    }
}