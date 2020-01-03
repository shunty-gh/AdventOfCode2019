using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/22
    public class Day22Original : IAoCRunner
    {
        private static readonly int DayNumber = 22;

        public void Run(ILogger log)
        {
            const int DeckLength = 10007;
            var input = AocHelpers.GetDayLines(DayNumber);
            //var input = GetTestInput();
            var deck = new int[DeckLength];
            var tmp = new int[DeckLength];
            for (var i = 0; i < DeckLength; i++) { deck[i] = i; };

            var forward = 1;
            var start = 0;
            foreach (var shuffle in input)
            {
                if (shuffle.StartsWith("deal into"))
                {
                    start -= forward;
                    start = ((start % DeckLength) + DeckLength) % DeckLength;
                    forward = -forward;
                }
                else if (shuffle.StartsWith("cut"))
                {
                    var cut = int.Parse(shuffle.Split(' ')[1]);
                    start += (cut * forward);
                    start = ((start % DeckLength) + DeckLength) % DeckLength;
                }
                else if (shuffle.StartsWith("deal with"))
                {
                    var inc = int.Parse(shuffle.Split(' ')[3]);
                    int di = start, ti = 0;
                    for (var i = 0; i < DeckLength; i++)
                    {
                        tmp[ti] = deck[di];
                        di = (((di + forward) % DeckLength) + DeckLength) % DeckLength;
                        ti = (ti + inc) % DeckLength;
                    }

                    start = 0;
                    forward = 1;
                    deck = tmp.ToArray();
                }
                else
                    throw new Exception($"Unknown shuffle method \"{shuffle}\"");
            }

            var result = new int[DeckLength];
            var part1 = 0;
            for (var i = 0; i < DeckLength; i++)
            {
                var di = (((start + (i * forward)) % DeckLength) + DeckLength) % DeckLength;
                result[i] = deck[di];
                if (deck[di] == 2019)
                {
                    part1 = i;
                }
            }
            //log.Debug("Deck: {@Deck}", result);
            Console.WriteLine($"Part 1: {part1}");
        }

        private IList<string> GetTestInput()
        {
            return new List<string> {
                // "deal with increment 7",
                // "deal into new stack",
                // "deal into new stack",

                // "cut 6",
                // "deal with increment 7",
                // "deal into new stack",

                // "deal with increment 7",
                // "deal with increment 9",
                // "cut -2",

                "deal into new stack",
                "cut -2",
                "deal with increment 7",
                "cut 8",
                "cut -4",
                "deal with increment 7",
                "cut 3",
                "deal with increment 9",
                "deal with increment 3",
                "cut -1",
            };
        }
    }
}
