import math

f = open("./day01-input.txt", "r")
input = [int(line) for line in f.readlines()]
f.close()

part1 = sum((math.floor(num / 3) - 2) for num in input)
print("Part 1: ", part1)

part2 = 0
for fuel in input:
    while fuel > 0:
        fuel = math.floor(fuel / 3) - 2
        part2 += fuel if fuel > 0 else 0

print("Part 2: ", part2)