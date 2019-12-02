<Query Kind="Program" />

void Main()
{
    var input = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"day01-input.txt"))
        .Select(l => int.Parse(l));

    // Tests, part 2
    //input = new List<int> { 1969 }; // == 966
    //input = new List<int> { 100756 }; // == 50346
    
    Func<int, int> FuelRequired = (mass) => (int)(Math.Truncate(mass / 3.0)) - 2;
    var part1 = 0L;
    var part2 = 0L;
    foreach (var i in input)
    {
        var fuel = FuelRequired(i);
        part1 += fuel;
        part2 += fuel;
        
        while (fuel > 0)
        {
            fuel = FuelRequired(fuel);
            if (fuel > 0)
              part2 += fuel;
        }
    }
    Console.WriteLine($"Part 1: {part1}");
    Console.WriteLine($"Part 2: {part2}");
}
