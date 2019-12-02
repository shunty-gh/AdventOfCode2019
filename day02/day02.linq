<Query Kind="Program" />

void Main()
{
    var input = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"day02-input.txt"))
        .Split(',')
        .Select(s => int.Parse(s))
        .ToArray();

    // Tests, part 1
    //input = new int[] {1,0,0,0,99}; // == [2,0,0,0,99]
    //input = new int[] {2,3,0,3,99}; // == [2,3,0,6,99]
    //input = new int[] {2,4,4,5,99,0}; // == [2,4,4,5,99,9801]
    //input = new int[] {1,1,1,4,99,5,6,0,99}; // == [30,1,1,4,2,5,6,0,99]
    
    var initialstate = new int[input.Length];
    input.CopyTo(initialstate, 0);
    
    var target = 19690720;

    bool p1found = false, p2found = false;
    int noun = 0, verb = 0;
    for (noun = 0; noun <= 99; noun++)
    {
        for (verb = 0; verb <= 99; verb++)
        {
            // Reset
            initialstate.CopyTo(input, 0);
            // Run it
            IntcodeCompute(input, noun, verb);
            // Check it
            if (noun == 12 && verb == 2) // Part 1 inputs
            {
                p1found = true;
                Console.WriteLine($"Part 1: {input[0]}");
            }
            if (input[0] == target)
            {
                p2found = true;
                Console.WriteLine($"Part 2: {input[0]}, Noun: {noun}, Verb: {verb}, Result: {(100 * noun) + verb}");
            }

            // Quit when we can
            if (p1found && p2found)
                return;
        }
        
    }
}

public void IntcodeCompute(int[] input, int noun, int verb)
{
    var finished = false;
    var ip = 0;
    var skip = 4;
    var len = input.Length;
    input[1] = noun;
    input[2] = verb;

    while (!finished)
    {
        var opcode = input[ip];
        int p1 = ip + 1 < len ? input[ip + 1] : 0, 
            p2 = ip + 2 < len ? input[ip + 2] : 0,
            p3 = ip + 3 < len ? input[ip + 3] : 0;
            
        switch (opcode)
        {
            case 1: // Add
                input[p3] = input[p1] + input[p2];
                break;
            case 2: // Multiply
                input[p3] = input[p1] * input[p2];
                break;
            case 99:
                finished = true;
                break;
            default:
                throw new Exception($"Unknown instruction {opcode} at position {ip}");
        }
        ip += skip;
    }
}