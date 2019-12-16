using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Shunty.AdventOfCode2019
{
    // https://adventofcode.com/2019/day/15

    public class GridLocation
    {
        public Point GridRef { get; set; }
        public List<(Point Path, int Direction)> Path { get; } = new List<(Point, int)>();
        public int PathCount => Path.Count;
        public char Status { get; set; } = ' ';

        public bool IsWall => Status == 'W';
        public int X => GridRef.X;
        public int Y => GridRef.Y;

        public GridLocation(int x, int y, List<(Point, int)> path, char status = ' ')
        {
            GridRef = new Point(x, y);
            UpdatePath(path);
            Status = status;
        }

        public GridLocation(Point loc, List<(Point, int)> path, char status = ' ')
        {
            GridRef = loc;
            UpdatePath(path);
            Status = status;
        }

        public void UpdatePath(List<(Point, int)> newPath)
        {
            if ((GridRef.X == 0 && GridRef.Y == 0) || newPath == null)
                return;

            if ((newPath.Count < Path.Count - 1) || (Path.Count == 0))
            {
                Path.Clear();
                Path.AddRange(newPath);
            }
        }
    }

    public class Day15 : IAoCRunner
    {
        private static readonly int DayNumber = 15;

        public void Run(ILogger log)
        {
            var input = AocHelpers.GetDayText(DayNumber)
                .Split(',')
                .Select(s => Int64.Parse(s))
                .ToArray();

            var directions = new (int dX, int dY)[] { (0,0), (0,1), (0,-1), (-1,0), (1,0) }; // N=1, S=2, W=3, E=4

            var map = new Dictionary<Point, GridLocation>();
            int direction = 1;
            var gr = new Point(0, 0);
            map[gr] = new GridLocation(gr, null);
            var current = map[gr];
            var path = new List<(Point Path, int Direction)>();
            bool backtrack = false;

            var inQ = new ConcurrentQueue<Int64>();
            var outQ = new ConcurrentQueue<Int64>();
            var t = Task.Factory.StartNew(() => IntcodeCompute(input.ToArray(), inQ, outQ));
            while(!t.IsCompleted)
            {
                inQ.Enqueue(direction);
                Int64 status;
                while (!outQ.TryDequeue(out status))
                {}
                if (status == 99)
                    break;

                var dir = directions[direction];
                switch (status)
                {
                    case 0:  // Wall
                        var wall = new GridLocation(gr.X + dir.dX, gr.Y + dir.dY, null, 'W');
                        map[wall.GridRef] = wall;
                        break;
                    case 1:  // Moved
                        gr = new Point(gr.X + dir.dX, gr.Y + dir.dY);
                        if (!backtrack)
                            path.Add((gr, direction));
                        if (!map.ContainsKey(gr))
                        {
                            current = new GridLocation(gr, path);
                            map[gr] = current;
                        }
                        else
                        {
                            current = map[gr];
                            // Update path if this one is shorter
                            current.UpdatePath(path);
                        }
                        break;
                    case 2:  // Found it
                        gr = new Point(gr.X + dir.dX, gr.Y + dir.dY);
                        if (!backtrack)
                            path.Add((gr, direction));
                        if (!map.ContainsKey(gr))
                        {
                            current = new GridLocation(gr, path, 'O');
                            map[gr] = current;
                        }
                        log.Debug("Found oxygen system at {@OSPos}. Path length: {PathLen} MinX: {MinX}; MinY: {MinY}; MaxX: {MaxX}; MaxY: {MaxY}", current.GridRef, current.PathCount, map.Min(kvp => kvp.Key.X), map.Min(kvp => kvp.Key.Y), map.Max(kvp => kvp.Key.X), map.Max(kvp => kvp.Key.Y));
                        break;
                    default:
                        throw new Exception($"Unknown status returned: {status}");
                }

                backtrack = true;
                for (var nextdirection = 1; nextdirection <=4; nextdirection++)
                {
                    var next = new Point(current.X + directions[nextdirection].dX, current.Y + directions[nextdirection].dY);
                    if (map.ContainsKey(next))
                    {
                        var nx = map[next];
                        if (nx.IsWall)
                            continue;
                        var p = nx.Path;
                        if (p.Count < current.PathCount - 1)
                        {
                            p.Add((current.GridRef, direction));
                            current.UpdatePath(p);
                        }
                    }
                    else
                    {
                        backtrack = false;
                        direction = nextdirection;
                        break;
                    }
                }

                if (backtrack)
                {
                    if (path.Count == 0)
                    {
                        inQ.Enqueue(99);
                        break;
                    }
                    // Knock the last entry off the path and reverse the direction
                    var prev = path.Last();
                    path.RemoveAt(path.Count - 1);
                    if (prev.Direction == 1 || prev.Direction == 3)
                        direction = prev.Direction + 1;
                    else
                        direction = prev.Direction - 1;
                }
            }

            DrawMap(map);
        }

        private void DrawMap(Dictionary<Point, GridLocation> map)
        {
            int minX = map.Min(p => p.Key.X);
            int maxX = map.Max(p => p.Key.X);
            int minY = map.Min(p => p.Key.Y);
            int maxY = map.Max(p => p.Key.Y);

            Console.WriteLine("");
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var p = new Point(x, y);
                    if (map.ContainsKey(p))
                    {
                        if (x == 0 && y ==0)
                        {
                            Console.Write("X");
                        }
                        else
                        {
                            var gl = map[p];
                            var s = gl.Status == 'W'
                                ? "#"
                                : gl.Status == 'O'
                                    ? "O"
                                    : ".";
                            Console.Write(s);
                        }
                    }
                    else
                    {
                        Console.Write("?");
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
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