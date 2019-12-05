package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"strconv"
	"strings"
)

func main() {
	originalInput, err := inputAsIntArray(".\\day05-input.txt")
	if err != nil {
		log.Fatal(err)
	}

	input := make([]int, len(originalInput))
	copy(input, originalInput)
	part1, err := runProgram(input, 1)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("Part 1", part1)

	copy(input, originalInput)
	part2, err := runProgram(input, 5)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("Part 2", part2)
}

func runProgram(program []int, input int) (result int, err error) {
	ip, lastoutput, skip, len := 0, 0, 0, len(program)
	done := false
	for !done {
		instruction := program[ip]
		opcode := instruction % 100
		// Get the parameter modes
		m1 := (instruction / 100) % 10
		m2 := (instruction / 1000) % 10
		//m3 := (instruction / 10000) % 10

		// Read the parameters - doesn't matter if we read too many for the current op
		p1, p2, p3 := 0, 0, 0
		if ip+1 < len {
			p1 = program[ip+1]
		}
		if ip+2 < len {
			p2 = program[ip+2]
		}
		if ip+3 < len {
			p3 = program[ip+3]
		}

		// Set the parameter values - mode dependent
		v1, v2 := p1, p2
		if m1 == 0 && p1 < len {
			v1 = program[p1]
		}
		if m2 == 0 && p2 < len {
			v2 = program[p2]
		}

		switch opcode {
		case 1: // Add
			program[p3] = v1 + v2
			skip = 4
		case 2: // Multiply
			program[p3] = v1 * v2
			skip = 4
		case 3: // Input
			program[p1] = input
			skip = 2
		case 4: // Output
			lastoutput = v1
			log.Println("Test result: ", v1)
			skip = 2
		case 5: // Jump if true
			skip = 3
			if v1 != 0 {
				ip = v2
				skip = 0
			}
		case 6: // Jump if false
			skip = 3
			if v1 == 0 {
				ip = v2
				skip = 0
			}
		case 7:
			if v1 < v2 {
				program[p3] = 1
			} else {
				program[p3] = 0
			}
			skip = 4
		case 8:
			if v1 == v2 {
				program[p3] = 1
			} else {
				program[p3] = 0
			}
			skip = 4
		case 99:
			done = true
		default:
			return 0, fmt.Errorf("Unexpected opcode %d", opcode)
		}

		ip += skip
	}

	return lastoutput, nil
}

func inputAsIntArray(inputFile string) (nums []int, err error) {
	txt, err := ioutil.ReadFile(inputFile)
	if err != nil {
		return nil, err
	}

	split := strings.Split(string(txt), ",")
	nums = make([]int, len(split))
	for i, n := range split {
		if len(n) == 0 {
			continue
		}
		num, err := strconv.Atoi(n)
		if err != nil {
			return nil, err
		}
		nums[i] = num
	}

	return nums, nil
}
