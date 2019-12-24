using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/23
    public class Day23 : IAoCRunner
    {
        // Takes a random amount of time to complete. Not sure why but it's
        // probably something to do with the threading and a race issue somewhere.
        // But it does appear to give the right result after a few minutes.
        // Usually somewhere between 2 and 5 minutes.

        private static readonly int DayNumber = 23;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var initialInput = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var (part1, part2) = DoPart(initialInput);
            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine($"Part 2: {part2}");
        }

        private Int64 part2 = 0;
        private List<(Int64 packetX, Int64 packetY)> _address0InputMonitor = new List<(long, long)>();
        private void OnAddress0Input((int SenderId, Int64 packetX, Int64 packetY) data)
        {
            if (data.packetX == 0 && data.packetY == 0)
                return;

            if (_address0InputMonitor.Count > 0 && _address0InputMonitor.Last().packetY == data.packetY)
            {
                _log.Debug($"Found 2 in a row: {data.packetY}");
                part2 = data.packetY;
            }
            //_log.Debug("Address 0 monitoring: {@Data}", data);
            _address0InputMonitor.Add((data.packetX, data.packetY));
        }

        private (Int64, Int64) DoPart(Int64[] initialInput)
        {
            var lan = new ConcurrentQueue<(int, Int64)>();
            var outputs = new Dictionary<Int64, List<Int64>>();
            var part1 = 0L;
            (Int64 X, Int64 Y) nat = (0L, 0L);

            var pcs = new Dictionary<int, Intcode2>();
            for (var i = 0; i < 50; i++)
            {
                var pc = new Intcode2(i);
                outputs[i] = new List<Int64>();
                pc.SetOutputQueue(lan); // Use the same output queue for all of them
                pc.Enqueue(i);
                if (i == 0)
                {
                    pc.Enqueue(-1);
                }
                var t = Task.Factory.StartNew(() => pc.Run(initialInput.ToArray()));
                pcs[i] = pc;
            }

            var done = false;
            while (!done)
            {
                (int SenderId, Int64 Fragment) lanfragment;
                if (lan.TryDequeue(out lanfragment))
                {
                    outputs[lanfragment.SenderId].Add(lanfragment.Fragment);
                    if (outputs[lanfragment.SenderId].Count == 3)
                    {
                        var pcindex = outputs[lanfragment.SenderId][0];
                        var x = outputs[lanfragment.SenderId][1];
                        var y = outputs[lanfragment.SenderId][2];

                        if (pcindex == 255)
                        {
                            _log.Debug("Found packet for address 255: X = {PacketX}; Y = {PacketY}", x, y);
                            if (part1 == 0)
                                part1 = y;
                            nat = (x, y);
                        }
                        else if (pcindex >= pcs.Count)
                        {
                            _log.Warning("Packet sent from {Id} to address {PacketAddress}: X = {PacketX}; Y = {PacketY}", lanfragment.SenderId, pcindex, x, y);
                        }
                        else
                        {
                            _log.Debug("Packet sent from {Id} to address {PacketAddress}: X = {PacketX}; Y = {PacketY}", lanfragment.SenderId, pcindex, x, y);
                            var pc = pcs[(int)pcindex];
                            pc.Enqueue(new Int64[] { x, y });
                        }

                        outputs[lanfragment.SenderId].Clear();
                    }
                }

                // Check if part 2 is done
                if (part1 != 0 && part2 != 0)
                {
                    done = true;
                }

                // If they're stalled send -1
                if (lan.Count == 0)
                {
                    if (pcs.All(pc => !pc.Value.HasInput))
                    {
                        // Idle
                        pcs[0].Enqueue(new Int64[] { nat.X, nat.Y });
                        OnAddress0Input((255, nat.X, nat.Y));
                    }
                    else
                    {
                        foreach (var pc in pcs)
                        {
                            if (!pc.Value.HasInput)
                                pc.Value.Enqueue(-1);
                        }
                    }
                }
            }

            return (part1, part2);
        }

    }
}