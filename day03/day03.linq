<Query Kind="Program" />

void Main()
{
    var input = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"day03-input.txt"))
        .Select(l => l.Split(',').ToList())
        .ToList();

    //var input = new List<List<string>> {   // P1=6; P2=30
    //    "R8,U5,L5,D3".Split(',').ToList(),
    //    "U7,R6,D4,L4".Split(',').ToList(),
    //};

    //var input = new List<List<string>> { // P1=159; P2=610
    //    "R75,D30,R83,U83,L12,D49,R71,U7,L72".Split(',').ToList(),
    //    "U62,R66,U55,R34,D71,R55,D58,R83".Split(',').ToList(),
    //};

    //var input = new List<List<string>> {  // P1=135; P2=410
    //    "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51".Split(',').ToList(),
    //    "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7".Split(',').ToList(),
    //};
    
    var p1 = MapPath(input[0]);
    var p2 = MapPath(input[1]);

    var intersection = p1
        .Join(
            p2, 
            p => (p.X, p.Y), 
            p => (p.X, p.Y), 
            (pp1, pp2) => new { X = pp1.X, Y = pp1.Y, Distance = pp1.Distance, TotalSteps = pp1.Steps + pp2.Steps }
        );
    
    var part1 = intersection.OrderBy(p => p.Distance).First();
    Console.WriteLine($"Part 1: {part1.Distance} (X={part1.X}; Y={part1.Y})");
    var part2 = intersection.OrderBy(p => p.TotalSteps).First();
    Console.WriteLine($"Part 2: {part2.TotalSteps} (X={part2.X}; Y={part2.Y})");
}

public struct Location
{
    public int X;
    public int Y;
    public int Steps;
    public int Distance => Math.Abs(X) + Math.Abs(Y);
}

public List<Location> MapPath(IEnumerable<string> path)
{
    var result = new List<Location>();
    int cx = 0, cy = 0, steps = 0;
    foreach (var inst in path)
    {
        var direction = inst[0];
        var distance = int.Parse(inst.Substring(1));
        foreach (var d in Enumerable.Range(1, distance))
        {
            switch (direction)
            {
                case 'D':
                    cy--;
                    break;
                case 'U':
                    cy++;
                    break;
                case 'L':
                    cx--;
                    break;
                case 'R':
                    cx++;
                    break;
                default:
                    throw new Exception($"Unknown direction \"{direction}\"");
            }
            result.Add(new Location { X = cx, Y = cy, Steps = ++steps });
        }
    }
    return result;
}