using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    /// Simple template for the AoC day class
    public class Day08 : IAoCRunner
    {
        private static readonly int DayNumber = 8;

        public void Run(ILogger log)
        {
            const int Width = 25;
            const int Height = 6;
            const int LayerSize = Width * Height;

            var input = AocHelpers.GetDayText(DayNumber);
            var len = input.Length;
            var layercount = len / LayerSize;
            int lowestzerocount = int.MaxValue, lowestzerolayer = 0, part1 = 0;
            var layers = new List<string>();
            for (var l = 0; l < layercount; l++)            
            {
                var layer = input.Substring(l * LayerSize, LayerSize);
                layers.Add(layer);
                var zerocount = layer.Count(c => c == '0');
                if (zerocount < lowestzerocount)
                {
                    lowestzerocount = zerocount;
                    lowestzerolayer = l;
                    var num1 = layer.Count(c => c == '1');
                    var num2 = layer.Count(c => c == '2');
                    part1 = num1 * num2;
                }

            }
            Console.WriteLine($"Part 1: {part1}");

            Console.WriteLine("Part 2: (You'd better have a fixed pitch font for this)");
            Console.WriteLine("");
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var l = 0; l < layercount; l++)
                    {
                        var offset = x + (y * Width);
                        var layer = layers[l];
                        var p = layer[offset];
                        if (p == '1') // White
                        {
                            Console.Write("**");
                            break;
                        }
                        else if (p == '0') // Black
                        {
                            Console.Write("  ");
                            break;
                        }
                        // else // Transparent
                    }
                }
                Console.WriteLine("");
            }
        }
    }
}