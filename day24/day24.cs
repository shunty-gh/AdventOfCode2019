using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/24
    public class Day24 : IAoCRunner
    {
        private static readonly int DayNumber = 24;
        private ILogger _log;

        public void Run(ILogger log)
        {
            _log = log;
            var initialInput = AocHelpers.GetDayLines(DayNumber);
            //var initialInput = GetTestInput();

            //_log.Debug("Start state: {State}", WorldRating(cells));
            //_log.Debug("World: {@World}", cells);

            var cells = CreateInitialState(initialInput);
            var part1 = DoPart1(cells);
            Console.WriteLine($"Part 1: {part1}");

            cells = CreateInitialState(initialInput);
            var part2 = DoPart2(cells);
            Console.WriteLine($"Part 2: {part2}");
        }

        private Int64 DoPart1(Dictionary<(int, int, int), Cell> cells)
        {
            var ratings = new HashSet<int>();
            while (true)
            {
                Evolve(cells, 1);
                var wr = WorldRating(cells);
                if (ratings.Contains(wr))
                {
                    DrawWorld(cells);
                    return wr;
                }
                ratings.Add(WorldRating(cells));
            }
        }

        private Int64 DoPart2(Dictionary<(int, int, int), Cell> cells)
        {
            int rounds = 200, round = 0;
            while (round++ < rounds)
            {
                Evolve(cells, 2);
            }
            //DrawWorld(cells);
            return cells.Count(k => k.Value.State == true);
        }

        private Dictionary<(int, int, int), Cell> CreateInitialState(IList<string> input)
        {
            var result = new Dictionary<(int, int, int), Cell>();
            int y = 0;
            foreach (var line in input)
            {
                int x = 0;
                foreach (var ch in line)
                {
                    result[(x,y,0)] = new Cell { X = x, Y = y, State = ch == '#' };
                    x++;
                }
                y++;
            }
            return result;
        }

        private int WorldRating(Dictionary<(int, int, int), Cell> cells)
        {
            return cells.Sum(k => k.Value.Rating);
        }

        private void Evolve(Dictionary<(int X, int Y, int Level), Cell> cells, int part)
        {
            // For part 2 we need to check any active levels plus 1 more above and below
            var minLevel = part == 2 ? cells.Min(k => k.Key.Level) - 1 : 0;
            var maxLevel = part == 2 ? cells.Max(k => k.Key.Level) + 1 : 0;

            for (var level = minLevel; level <= maxLevel; level++)
            {
                foreach (var y in Enumerable.Range(0,5))
                {
                    foreach (var x in Enumerable.Range(0,5))
                    {
                        // Skip centre cell for part 2 - it's the gateway to the next level
                        if (x == 2 && y == 2 && part == 2)
                            continue;

                        var neighbours = GetActiveNeighbours(cells, x, y, level, part);
                        Cell cell = null;
                        if (cells.ContainsKey((x,y,level)))
                            cell = cells[(x,y,level)];
                        var state = cell?.State ?? false;

                        // Apply rules
                        // Bug dies unless exactly 1 bug next to it
                        if (state)
                        {
                            cell.NextState = neighbours == 1;
                        }
                        // Empty space becomes infected if 1 or 2 bugs adjacent
                        else
                        {
                            if (neighbours == 1 || neighbours == 2)
                            {
                                if (cell == null)
                                {
                                    cells[(x,y,level)] = new Cell { X = x, Y = y, Level = level, NextState = true };
                                }
                                else
                                {
                                    cell.NextState = true;
                                }
                            }
                        }
                    }
                }
            }

            // Update all cells to their new state
            foreach (var cell in cells)
                cell.Value.State = cell.Value.NextState;
        }

        private int GetActiveNeighbours(Dictionary<(int X, int Y, int Level), Cell> cells, int cellX, int cellY, int cellLevel, int part)
        {
            var result = 0;

            // For part 2:
            // If we're on the outer edge of our block then we need to check the inner cells
            // plus one (or two, for corners) side(s) in the upper level
            // If we're at any of (2,1), (1,2), (3,2), (2,3) then we need to check the 5 extra
            // neighbours in the lower level

            if (part == 2)
            {
                // Lower level checks
                if (cellX == 2 && cellY == 1)
                {
                    foreach (var ix in Enumerable.Range(0, 5))
                        result += GetCellState(cells, ix, 0, cellLevel + 1) ? 1 : 0;
                }
                else if (cellX == 2 && cellY == 3)
                {
                    foreach (var ix in Enumerable.Range(0, 5))
                        result += GetCellState(cells, ix, 4, cellLevel + 1) ? 1 : 0;
                }
                else if (cellY == 2 && cellX == 1)
                {
                    foreach (var iy in Enumerable.Range(0, 5))
                        result += GetCellState(cells, 0, iy, cellLevel + 1) ? 1 : 0;
                }
                else if (cellY == 2 && cellX == 3)
                {
                    foreach (var iy in Enumerable.Range(0, 5))
                        result += GetCellState(cells, 4, iy, cellLevel + 1) ? 1 : 0;
                }
            }

            foreach (var (dx, dy) in new (int, int)[] { (0,-1), (1,0), (0,1), (-1,0) })
            {
                int x = cellX + dx, y = cellY + dy;
                if (part == 2)
                {
                    // Upper level checks
                    if (y == -1)
                    {
                        result += GetCellState(cells, 2, 1, cellLevel - 1) ? 1 : 0;
                    }
                    else if (y == 5)
                    {
                        result += GetCellState(cells, 2, 3, cellLevel - 1) ? 1 : 0;
                    }
                    if (x == -1)
                    {
                        result += GetCellState(cells, 1, 2, cellLevel - 1) ? 1 : 0;
                    }
                    else if (x == 5)
                    {
                        result += GetCellState(cells, 3, 2, cellLevel - 1) ? 1 : 0;
                    }
                }

                // This level
                result += GetCellState(cells, x, y, cellLevel) ? 1 : 0;
            }

            return result;
        }

        private bool GetCellState(Dictionary<(int X, int Y, int Level), Cell> cells, int x, int y, int level)
        {
            if (cells.ContainsKey((x, y, level)))
            {
                return cells[(x, y, level)].State;
            }
            return false;
        }

        private void DrawWorld(Dictionary<(int x, int y, int level), Cell> cells)
        {
            const char DeadChar = '.';
            const char LiveChar = '#';

            if (cells.Count == 0)
                return;

            var minLevel = cells.Min(kvp => kvp.Key.level);
            var maxLevel = cells.Max(kvp => kvp.Key.level);

            Console.WriteLine();
            for (var level = minLevel; level <= maxLevel; level++)
            {
                Console.WriteLine($" Level {level}: ");
                foreach (var y in Enumerable.Range(0,5))
                {
                    foreach (var x in Enumerable.Range(0,5))
                    {
                        if (cells.ContainsKey((x,y,level)))
                        {
                            var cell = cells[(x,y,level)];
                            Console.Write(cell.State ? LiveChar : DeadChar);
                        }
                        else
                        {
                            Console.Write(DeadChar);
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        private IList<string> GetTestInput()
        {
            return new List<string> {
                "....#",
                "#..#.",
                "#..##",
                "..#..",
                "#....",
            };
        }
    }

    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Level { get; set; } = 0;
        public bool State { get; set; } = false;
        public bool NextState { get; set; } = false;

        public int Rating => State ? (int)Math.Pow(2, X + (Y * 5)) : 0;
    }
}