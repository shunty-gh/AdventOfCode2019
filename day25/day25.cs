using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019.Day25
{
    // https://adventofcode.com/2019/day/25

    /* To roam around manually use the commands n,s,w,e,i (for inventory),
     * drop ..., take ...
     * To make it run automatically you need to first have discovered the
     * items manually and then created a list of them and the routes required
     * to reach them.
     * Once set then run the program and use the 'go' command to make it
     * collect the items, go to the door and try all combinations until it
     * gets in.
     */

    public class Day25 : IAoCRunner
    {
        private static readonly int DayNumber = 25;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var initialInput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            RunManual(initialInput.ToArray());
        }

        private void RunManual(Int64[] program)
        {
            var pc = new Intcode();
            var t = Task.Factory.StartNew(() => pc.Run(program));
            while (!t.IsCompleted)
            {
                var output = GetOutput(pc);
                Console.Write(output);

                var cont = SendCommand(pc);
                if (!cont)
                {
                    break;
                }
            }
        }

        private string GetOutput(Intcode pc, bool waitAll = false)
        {
            var sb = new StringBuilder();
            var wait1 = false;
            Int64 outch = 0;
            var result = "";

            while (true)
            {
                while (pc.TryDequeue(out outch))
                {
                    wait1 = false;
                    sb.Append((char)outch);
                    Thread.Sleep(1);
                }
                if (sb.ToString().Contains("Command?") && !waitAll)
                {
                    result = sb.ToString();
                    break;
                }
                else if (!wait1)
                {
                    wait1 = true;
                    Thread.Sleep(50);
                }
                else
                {
                    result = sb.ToString();
                    break;
                }
            }
            return result;
        }

        private bool SendCommand(Intcode pc)
        {
            string command = "";
            var sent = false;
            while (!sent)
            {
                var input  = Console.ReadLine();
                switch (input)
                {
                    case "n":
                        command = "north";
                        break;
                    case "s":
                        command = "south";
                        break;
                    case "w":
                        command = "west";
                        break;
                    case "e":
                        command = "east";
                        break;
                    case "i":
                        command = "inv";
                        break;
                    case "items":
                        GetItems(pc);
                        return true;
                    case "try":
                        TryAllItems(pc);
                        return true;
                    case "go":
                        GetItems(pc);
                        TryAllItems(pc);
                        return true;
                    case "x":
                        return false;
                    default:
                        command = input;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(command))
                {
                    QueueCommand(command, pc);
                    sent = true;
                }
            }
            return true;
        }

        private void GetItems(Intcode pc)
        {
            // A manual process of discovery (DFS) led to finding
            // the following route commands to get all the useful items
            // and then going to the cockpit door.
            var commands = new List<string> {
                "south",
                "take mutex",
                "south",
                "take manifold",
                "west",
                "west",
                "take klein bottle",
                "east",
                "east",
                "north",
                "east",
                "take mug",
                "east",
                "take polygon",
                "north",
                "take loom",
                "north",
                "take hypercube",
                "south",
                "south",
                "east",
                "east",
                "east",
                "take pointer",
                "south",
                "west",
            };
            foreach (var cmd in commands)
            {
                QueueCommand(cmd, pc);
            }
        }

        private void TryAllItems(Intcode pc)
        {
            /* By a process of manual discovery we have the following items:
             *   klein bottle, loom, mutex, pointer, polygon, hypercube, mug, manifold
             * By a similar process of trial and error we discovered NOT to pick up:
             *   molten lava, giant electromagnet, infinite loop, escape pod, photons
             */

            var items = new List<string> {
                "klein bottle",
                "loom",
                "mutex",
                "pointer",
                "polygon",
                "hypercube",
                "mug",
                "manifold",
            };

            // Need to try all combinations of items to get into the room
            // to the west
            var combos = GetCombinations(items);
            var ci = 0;
            var output = "";
            Console.Write("Trying combinations");
            foreach (var combo in combos)
            {
                // Drop all items then pick up the ones in this combination
                items.ForEach(i => QueueCommand($"drop {i}", pc));
                combo.ForEach(i => QueueCommand($"take {i}", pc));
                // Clear the output queue
                output = GetOutput(pc, true);
                // Try and move through the door
                QueueCommand("west", pc);

                output = GetOutput(pc);
                // if (output.Contains("and you are ejected"))
                // {
                //     if (output.Contains("heavier"))
                //         _log.Debug("Combination {CombinationIndex} {@Combination} is too light", ci, combo);
                //     else if (output.Contains("lighter"))
                //         _log.Debug("Combination {CombinationIndex} {@Combination} is too heavy", ci, combo);
                //     else
                //         _log.Debug("Combination {CombinationIndex} {@Combination} not understood: {Output}", ci, combo, output);
                // }
                // else
                // {
                //     _log.Debug("Combination {CombinationIndex} {@Combination} has worked", ci, combo);
                //     Console.Write(output);
                //     Console.WriteLine();
                //     break;
                // }
                if (output.Contains("You may proceed"))
                {
                    Console.WriteLine();
                    _log.Debug("Combination {CombinationIndex} {@Combination} has worked", ci, combo);
                    Console.Write(output);
                    break;
                }
                else
                {
                    Console.Write(".");
                }
                ci++;
            }
        }

        private List<List<string>> GetCombinations(List<string> items)
        {
            // We know there are 8 items. Therefore there are 256 combinations.
            var result = new List<List<string>>();
            for (var i = 0; i < 256; i++)
            {
                var comb = new List<string>();
                var idx = i;
                var offset = 0;
                while (idx > 0)
                {
                    if ((idx & 1) > 0)
                    {
                        comb.Add(items[offset]);
                    }
                    idx >>= 1;
                    offset++;
                }

                result.Add(comb);
            }
            return result;
        }

        private void QueueCommand(string command, Intcode pc)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                foreach (var ch in command)
                {
                    if ((int)ch >= 32 && (int)ch <= 255)
                        pc.Enqueue((int)ch);
                }
                pc.Enqueue(10);
            }
        }
    }
}
