// Define SHOW_GAMEBOARD if running in a normal shell/console window
// and you want to see the game being played in clunky ASCII.
// Leave it undefined for a speedier result or if running from
// within the VS/VSCode debugger/console.
//#define SHOW_GAMEBOARD

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;


namespace Shunty.AdventOfCode2019
{
    public struct Location
    {
        public Int64 X;
        public Int64 Y;

        public Location(Int64 x, Int64 y)
        {
            X = x;
            Y = y;
        }
    }

    public class Tile
    {
        public Int64 X { get; set; } = 0;
        public Int64 Y { get; set; } = 0;
        public Int64 Id { get; set; } = 0;
    }

    public class Day13 : IAoCRunner
    {
        const int Empty = 0;
        const int Wall = 1;
        const int Block = 2;
        const int Paddle = 3;
        const int Ball = 4;

        // Set this to true to see a running update of the score
        // when no showing the full game window
        private bool ShowScoreUpdates = false;

        private static readonly int DayNumber = 13;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var map = new Dictionary<Location, Tile>();
            var program = input.ToArray();
            RunGame(program, map);
            var grouped = map.GroupBy(m => m.Value.Id).Select(g => (g.Key, g.Count())).OrderBy(g => g.Key).ToList();
            log.Debug("Tile types: {@TileTypes}", grouped);
            var part1 = map.Count(kvp => kvp.Value.Id == Block);

            map.Clear();
            program = input.ToArray();
            program[0] = 2;
            var part2 = RunGame(program, map);

            Console.WriteLine("");
            Console.WriteLine($"Part 1: {part1}");
            Console.WriteLine($"Part 2: {part2}");
        }

        private Int64 RunGame(Int64[] program, Dictionary<Location, Tile> map)
        {
            ConcurrentQueue<Int64> inQ = new ConcurrentQueue<Int64>(), outQ = new ConcurrentQueue<Int64>();
            Int64 score = 0, paddleX = -1;

            InitGameBoard();
            InitScoreboard();
            var t = Task.Factory.StartNew(() => IntcodeCompute(program, inQ, outQ));
            int blockCount = 0;
            score = 0;
            try
            {
                while (!t.IsCompleted || outQ.Count() > 0)
                {
                    // Get 3 values from the queue
                    Int64 x = 0, y = 0, id = 0;
                    while (!outQ.TryDequeue(out x))
                    {}
                    while (!outQ.TryDequeue(out y))
                    {}
                    while (!outQ.TryDequeue(out id))
                    {}

                    if (x == -1 && y == 0)
                    {
                        score = id;
                        UpdateScore(score, blockCount);
                        if (blockCount == 0)
                            break;

                        continue;
                    }

                    if (id == Paddle)
                    {
                        if (paddleX < 0)
                        {
                            inQ.Enqueue(-1);
                        }
                        paddleX = x;
                    }
                    else if (id == Ball)
                    {
                        if (paddleX >= 0)
                        {
                            // Try and track the ball with the paddle
                            if (x < paddleX)
                                inQ.Enqueue(-1);
                            else if (x > paddleX)
                                inQ.Enqueue(1);
                            else
                                inQ.Enqueue(0);
                        }
                    }

                    var key = new Location(x, y);
                    Tile tile;
                    var delay = 10;
                    if (map.ContainsKey(key))
                    {
                        tile = map[key];
                        if (tile.Id == Block && id != Block)
                            blockCount--;
                        else if (tile.Id != Block && id == Block)
                            blockCount++;

                        tile.Id = id;
                    }
                    else
                    {
                        if (id == Block)
                            blockCount++;
                        tile = new Tile { X = x, Y = y, Id = id };
                        map[key] = tile;
                        delay = 0;
                    }

#if SHOW_GAMEBOARD
                    DrawTile(tile);
                    // Delay it a little
                    Thread.Sleep(delay);
#endif
                }

                return score;
            }
            finally
            {
                ResetScreen();
            }
        }

        private void DrawTile(Tile tile)
        {
#if !SHOW_GAMEBOARD
            return;
#else
            string s = "";
            switch ((int)tile.Id)
            {
                case Empty:
                    Console.ForegroundColor = ConsoleColor.Black;
                    s = " ";
                    break;
                case Wall:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    s = tile.X == 0 ? "▌" : "▐"; //"█";
                    break;
                case Block:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    s = "░";
                    break;
                case Paddle:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    s = "▀";
                    break;
                case Ball:
                    Console.ForegroundColor = ConsoleColor.Red;
                    s = "■";
                    break;
                default:
                    return;
            }
            Console.SetCursorPosition((int)tile.X, (int)tile.Y);
            Console.Write(s);
#endif
        }

#if SHOW_GAMEBOARD
        private int cursorTop = 0;
        private ConsoleColor orgFG = ConsoleColor.White;
        private ConsoleColor orgBG = ConsoleColor.Black;
#endif
        private void InitGameBoard()
        {
#if SHOW_GAMEBOARD
            cursorTop = Console.CursorTop;
            orgFG = Console.ForegroundColor;
            orgBG = Console.BackgroundColor;
            Console.Clear();
#endif
        }

        private void ResetScreen()
        {
#if SHOW_GAMEBOARD
            Console.ForegroundColor = orgFG;
            Console.BackgroundColor = orgBG;
            Console.SetCursorPosition(0, cursorTop);
            Console.ResetColor();
#endif
        }
        private void InitScoreboard()
        {
        }
        private void UpdateScore(Int64 score, int blockCount)
        {
#if SHOW_GAMEBOARD
            var h = Console.WindowHeight;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, 26);
            Console.Write($"Score: {score:#,##0};    Blocks: {blockCount:#,##0}  ");
#else
            if (ShowScoreUpdates)
                Console.WriteLine($"Score: {score}; Blocks: {blockCount}");
#endif
        }

        private Int64 ResizeProgram(ref Int64[] program, Int64 newSize)
        {
            Array.Resize(ref program, (int)newSize);
            return newSize;
        }

        private Int64 IntcodeCompute(Int64[] program, ConcurrentQueue<Int64> inQ, ConcurrentQueue<Int64> outQ)
        {
            Int64 lastoutput = 0, relativebase = 0, outaddr = 0;
            Int64 ip = 0, len = program.Length;

            while (true)
            {
                var instruction = program[ip];
                if (instruction == 99)
                    return lastoutput;

                Int64 opcode = instruction % 100;
                Int64 m1 = (instruction / 100) % 10,
                    m2 = (instruction / 1000) % 10,
                    m3 = (instruction / 10000) % 10;
                Int64 p1 = ip + 1 < len ? program[ip + 1] : 0,
                    p2 = ip + 2 < len ? program[ip + 2] : 0,
                    p3 = ip + 3 < len ? program[ip + 3] : 0;

                Int64 v1 = p1, v2 = p2;
                if (m1 == 0)
                    v1 = (p1 < len) ? program[p1] : 0;
                else if (m1 == 2)
                    v1 = (p1 + relativebase) < len ? program[p1 + relativebase] : 0;

                if (m2 == 0)
                    v2 = p2 < len ? program[p2] : 0;
                else if (m2 == 2)
                    v2 = (p2 + relativebase) < len ? program[p2 + relativebase] : 0;

                switch (opcode)
                {
                    case 1: // Add
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = v1 + v2;
                        ip += 4;
                        break;
                    case 2: // Multiply
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = v1 * v2;
                        ip += 4;
                        break;
                    case 3: // Input
                        if (inQ.TryDequeue(out var input))
                        {
                            outaddr = m1 == 0 ? p1 : p1 + relativebase;
                            if (outaddr >= len)
                                len = ResizeProgram(ref program, outaddr * 2);
                            program[outaddr] = input;
                            ip += 2;
                        }
                        break;
                    case 4: // Output
                        lastoutput = v1;
                        //Console.WriteLine($"Test result: {v1}");
                        ip += 2;
                        outQ.Enqueue(v1);
                        break;
                    case 5:  // Jump if true
                        if (v1 != 0)
                        {
                            ip = v2;
                        }
                        else
                        {
                            ip += 3;
                        }
                        break;
                    case 6:  // Jump if false
                        if (v1 == 0)
                        {
                            ip = v2;
                        }
                        else
                        {
                            ip += 3;
                        }
                        break;
                    case 7:  // Less than
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = (v1 < v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 8:  // Equals
                        outaddr = m3 == 0 ? p3 : p3 + relativebase;
                        if (outaddr >= len)
                            len = ResizeProgram(ref program, outaddr * 2);
                        program[outaddr] = (v1 == v2) ? 1 : 0;
                        ip += 4;
                        break;
                    case 9: // Update relative base
                        relativebase += v1;
                        ip += 2;
                        break;
                    case 99:
                        return lastoutput;
                    default:
                        throw new Exception($"Unknown instruction {opcode} at position {ip}");
                }
            }
       }
    }
}
