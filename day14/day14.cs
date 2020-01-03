using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day14 : IAoCRunner
    {
        private static readonly int DayNumber = 14;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayLines(DayNumber);
            //input = GetTestInput1(); // P1 == 165
            //input = GetTestInput2(); // P1 == 13312
            //input = GetTestInput3(); // P1 == 180697
            //input = GetTestInput4(); // P1 == 2210736
            var reactions = new Dictionary<string, Reaction>();
            foreach (var line in input)
            {
                var s = line.Trim().Split(new string[] { "=>"}, StringSplitOptions.RemoveEmptyEntries);
                var rhs = Compound.FromInput(s[1]);
                var lhs = s[0].Trim().Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                var compounds = new List<Compound>();
                foreach (var ss in lhs)
                {
                    var c = Compound.FromInput(ss);
                    compounds.Add(c);
                }
                reactions.Add(rhs.Name, new Reaction(rhs, compounds));
            }
            log.Debug("Reactions {@Reactions}", reactions);

            var thisround = new List<(string Name, Int64 Quantity)> { ("FUEL", 4_052_920) };
            var nextround = new List<(string Name, Int64 Quantity)>();
            var ores = new Dictionary<string, Int64>();
            var overs = new Dictionary<string, Int64>();
            while (thisround.Count > 0)
            {

                foreach (var req in thisround.Select(r => r.Name).Distinct())
                {
                    (string Name, Int64 Quantity) requirement = (req, thisround.Where(r => r.Name == req).Sum(r => r.Quantity));
                    if (overs.ContainsKey(requirement.Name))
                    {
                        var spare = overs[requirement.Name];
                        if (requirement.Quantity >= spare)
                        {
                            overs[requirement.Name] = 0;
                            requirement.Quantity -= spare;
                        }
                        else
                        {
                            overs[requirement.Name] = spare - requirement.Quantity;
                            continue;
                        }
                    }
                    var reaction = reactions[requirement.Name];
                    var mult = (Int64)Math.Ceiling((double)requirement.Quantity / (double)reaction.Result.Quantity);
                    foreach (var comp in reaction.Compounds)
                    {
                        if (comp.Name == "ORE")
                        {
                            if (ores.ContainsKey(reaction.Result.Name))
                                ores[reaction.Result.Name] += requirement.Quantity;
                            else
                                ores[reaction.Result.Name] = requirement.Quantity;
                            continue;
                        }
                        else
                        {
                            nextround.Add((comp.Name, mult * comp.Quantity));
                        }
                    }
                    if (reaction.Compounds.First().Name != "ORE")
                    {
                        var over = (mult * reaction.Result.Quantity) - requirement.Quantity;
                        if (over > 0)
                        {
                            if (overs.ContainsKey(requirement.Name))
                                overs[requirement.Name] += over;
                            else
                                overs[requirement.Name] = over;
                        }
                    }
                }

                thisround.Clear();
                thisround.AddRange(nextround);
                nextround.Clear();

            }
            log.Debug("Ores {@Ores}", ores);
            Int64 orerequired = 0;
            foreach (var o in ores)
            {
                var reaction = reactions[o.Key];
                var need = o.Value;
                var processProduces = reaction.Result.Quantity;
                var mults = (Int64)Math.Ceiling((double)need / (double)processProduces);
                orerequired += mults * reaction.Compounds.First().Quantity;
            }
            var target = 1_000_000_000_000;
            if (orerequired > target)
                Console.Write("Too high");

            Console.WriteLine($"Part 1: {orerequired}");

            Console.WriteLine("For part 2 I just did a trial and error 'guess too high'/'guess too low' binary search by hand but didn't bother to program it.");
        }

        private string[] GetTestInput1()
        {
            return new string[] {
                "9 ORE => 2 A",
                "8 ORE => 3 B",
                "7 ORE => 5 C",
                "3 A, 4 B => 1 AB",
                "5 B, 7 C => 1 BC",
                "4 C, 1 A => 1 CA",
                "2 AB, 3 BC, 4 CA => 1 FUEL",
            };
        }

        private string[] GetTestInput2() {
            return new string[] {
                "157 ORE => 5 NZVS",
                "165 ORE => 6 DCFZ",
                "44 XJWVT, 5 KHKGT, 1 QDVJ, 29 NZVS, 9 GPVTF, 48 HKGWZ => 1 FUEL",
                "12 HKGWZ, 1 GPVTF, 8 PSHF => 9 QDVJ",
                "179 ORE => 7 PSHF",
                "177 ORE => 5 HKGWZ",
                "7 DCFZ, 7 PSHF => 2 XJWVT",
                "165 ORE => 2 GPVTF",
                "3 DCFZ, 7 NZVS, 5 HKGWZ, 10 PSHF => 8 KHKGT",
            };
        }

       private string[] GetTestInput3() {
            return new string[] {
                "2 VPVL, 7 FWMGM, 2 CXFTF, 11 MNCFX => 1 STKFG",
                "17 NVRVD, 3 JNWZP => 8 VPVL",
                "53 STKFG, 6 MNCFX, 46 VJHF, 81 HVMC, 68 CXFTF, 25 GNMV => 1 FUEL",
                "22 VJHF, 37 MNCFX => 5 FWMGM",
                "139 ORE => 4 NVRVD",
                "144 ORE => 7 JNWZP",
                "5 MNCFX, 7 RFSQX, 2 FWMGM, 2 VPVL, 19 CXFTF => 3 HVMC",
                "5 VJHF, 7 MNCFX, 9 VPVL, 37 CXFTF => 6 GNMV",
                "145 ORE => 6 MNCFX",
                "1 NVRVD => 8 CXFTF",
                "1 VJHF, 6 MNCFX => 4 RFSQX",
                "176 ORE => 6 VJHF",
            };
       }

       private string[] GetTestInput4()
       {
           return new string[] {
                "171 ORE => 8 CNZTR",
                "7 ZLQW, 3 BMBT, 9 XCVML, 26 XMNCP, 1 WPTQ, 2 MZWV, 1 RJRHP => 4 PLWSL",
                "114 ORE => 4 BHXH",
                "14 VRPVC => 6 BMBT",
                "6 BHXH, 18 KTJDG, 12 WPTQ, 7 PLWSL, 31 FHTLT, 37 ZDVW => 1 FUEL",
                "6 WPTQ, 2 BMBT, 8 ZLQW, 18 KTJDG, 1 XMNCP, 6 MZWV, 1 RJRHP => 6 FHTLT",
                "15 XDBXC, 2 LTCX, 1 VRPVC => 6 ZLQW",
                "13 WPTQ, 10 LTCX, 3 RJRHP, 14 XMNCP, 2 MZWV, 1 ZLQW => 1 ZDVW",
                "5 BMBT => 4 WPTQ",
                "189 ORE => 9 KTJDG",
                "1 MZWV, 17 XDBXC, 3 XCVML => 2 XMNCP",
                "12 VRPVC, 27 CNZTR => 2 XDBXC",
                "15 KTJDG, 12 BHXH => 5 XCVML",
                "3 BHXH, 2 VRPVC => 7 MZWV",
                "121 ORE => 7 VRPVC",
                "7 XCVML => 6 RJRHP",
                "5 BHXH, 4 VRPVC => 5 LTCX",
           };

       }

    }

    public class Reaction
    {
        public Compound Result { get; set; }
        public List<Compound> Compounds  { get; } = new List<Compound>();

        public Reaction(Compound result, IEnumerable<Compound> compounds)
        {
            Result = result;
            Compounds.AddRange(compounds);
        }
    }

    public class Compound
    {
        public string Name { get; set; }
        public int Quantity { get; set; }

        public static Compound FromInput(string source)
        {
            var s = source.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            return new Compound { Name = s[1].Trim(), Quantity = int.Parse(s[0]) };
        }
    }
}