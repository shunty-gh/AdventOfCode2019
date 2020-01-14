# Advent Of Code 2019

## Notes for each day
So that I can find specific types of approach if I ever need them again.

### Helpful Links And Notes
* [Breadth First Search](https://en.wikipedia.org/wiki/Breadth-first_search)
* [Depth First Search](https://en.wikipedia.org/wiki/Depth-first_search)
* [Dijkstra's Algorithm](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)

* [Modular Arithmetic](https://en.wikipedia.org/wiki/Modular_arithmetic)
* [Exponentiation by squaring](https://en.wikipedia.org/wiki/Exponentiation_by_squaring)
* [Go documentation](https://golang.org/doc/)
* [Reddit](https://www.reddit.com/r/adventofcode/)
* [Algorithm choice for each day (Reddit)](https://www.reddit.com/r/adventofcode/comments/ehks6w/algorithm_choice_for_each_day/fcjuyxn?utm_source=share&utm_medium=web2x)
* [C++ for day 22](https://www.reddit.com/r/adventofcode/comments/eepz2i/2019_day_22_part_2_clean_annotated_solution_in_c/) - this was what finally helped me *get it*
* [Tutorial for day 22](https://codeforces.com/blog/entry/72593)


### Other People's Solutions
Some of which, maybe, I might have had a wee peek at when I got stuck (but not before at least 24 hours of thinking).

| URL                                      | Language(s) |
|------------------------------------------|----------|
| https://github.com/Bogdanp/awesome-advent-of-code | Many, many... |
| https://github.com/orez-/Advent-of-Code-2019 | Python |
| https://github.com/KanegaeGabriel/advent-of-code-2019 | Python |
| https://github.com/Starwort/advent-of-code-2019 | Python |
| https://github.com/AxlLind/AdventOfCode2019/ | Rust |
| https://github.com/BenoitZugmeyer/RustyAdventOfCode | Rust |
| https://github.com/lizthegrey/adventofcode/tree/master/2019 |Go |
| https://github.com/kindermoumoute/adventofcode | Go |


### Day 1 - The Tyranny of the Rocket Equation
`C#, Python, Go`

### Day 2 - Program Alarm
First day of the `IntCode` machine.    
`C#, Python`

### Day 3 - Crossed Wires
Manhattan distance and shortest combined distance.    
`C#`

### Day 4 - Secure Container
Constraints on numeric passwords.    
`C#, Go`

### Day 5 - Sunny with a Chance of Asteroids
`IntCode` with I/O and modes.    
`C#, Go`

### Day 6 - Universal Orbit Map
Basic graph and tree traversal. Shortest path between two nodes.    
`C#, Go`

### Day 7 - Amplification Circuit
Multiple `IntCode` machines. Solved with threading but threads weren't necessary. Concurrency, pipelines and permutations.    
`C#`

### Day 8 - Space Image Format
Image layers.    
`C#`

### Day 9 - Sensor Boost
The `IntCode` reaches its final state. Add relative base, ability to resize the memory and large integers.    
`C#, Go`

### Day 10 - Monitoring Station
Atan2, rotational geometry, angles and grids.    
`C#`

### Day 11 - Space Police
`IntCode` program to direct a robot to paint an image that results in a serial number/registration number.    
`C#, Go`

### Day 12 - The N-Body Problem
LCM and factors. Notice that x, y, z dimensions are independent.    
`C#`

### Day 13 - Care Package
Use the `IntCode` machine to build a 'breakout' game and guide the paddle to 'win'/beat the game.    
`C#, Go`

### Day 14 - Space Stoichiometry
Binary search for part 2 but just did it by trial and error. Could have used recursive algorithms but went linear instead.    
`C#`

### Day 15 - Oxygen System
Map exploring (DFS) & shortest paths (BFS) with the `IntCode` machine to guide a robot.   
`C#, Go.`

### Day 16 - Flawed Frequency Transmission
Signal processing. Spot pattern in the output to shortcut the final calculations.    
`C#`

### Day 17 - Set and Forget
Use the `IntCode` machine to direct a robot over scaffolding detecting intersections. Use string compression to provide a 'compressed' version of input commands. Used brute-force/visual inspection rather than programming in the compression algorithm.    
`C#`

### Day 18 - Many-Worlds Interpretation
BFS for paths between keys (because it is a maze).    
`C#`

### Day 19 - Tractor Beam
`IntCode` program to send a robot exploring a tractor beam.    
`C#`

### Day 20 - Donut Maze
BFS on a maze with multiple levels.    
`C#, Go`

### Day 21 - Springdroid Adventure
Program the `IntCode` machine to use logical AND OR NOT to guide a jumping robot around holes in the hull.    
`C#`

### Day 22 - Slam Shuffle
The big, nasty, modular arithmetic one. Combining linear functions. Multiplicative inverse modulo m and all that.    
`C#`

### Day 23 - Category Six
Use the `IntCode` program to send 'network packets' back and forth between 50 `IntCode` instances.   
Used threading here but that probably made it harder (and, most likely, slower (for C# version)) in the log run. The Go version, with channels and go routines is fast (and quite neat, I think!) but the C# version is very slow due to some messed up threading.    
`C#, Go`

### Day 24 - Planet of Discord
Basic Game of life in part 1 but then extended to recursive levels within levels for part 2. No recursion in the solution though.    
`C#, Go`

### Day 25 - Cryostasis 
`IntCode` to explore ship (manually - like an old fashioned game), find items, find the right combination of items. By making the `IntCode` threaded I made the solution to this problem much harder and slower than it could have been.    
`C#`