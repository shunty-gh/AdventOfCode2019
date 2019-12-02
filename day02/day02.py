
## compute function
def compute(input: list, noun: int, verb: int):
    input[1] = noun
    input[2] = verb
    ip = 0
    complete = False
    while complete == False:
        opcode = input[ip]
        p1 = input[ip + 1]
        p2 = input[ip + 2]
        p3 = input[ip + 3]

        if opcode == 1:
            input[p3] = input[p1] + input[p2]
        elif opcode == 2:
            input[p3] = input[p1] * input[p2]
        elif opcode == 99:
            complete = True

        ip += 4

## main
f = open("./day02-input.txt", "r")
initialinput = [int(num) for num in f.read().split(",")]
f.close()

# Part 1
p1 = initialinput.copy()
compute(p1, 12, 2)
print("Part 1:", p1[0])

# Part 2
target = 19690720
found = False
for noun in range(100):
    for verb in range(100):
        input = initialinput.copy()
        compute(input, noun, verb)
        if (input[0] == target):
            found = True
            print("Part 2:", (input[1] * 100) + input[2])
            break
    if found:
        break
