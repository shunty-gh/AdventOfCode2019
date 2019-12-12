package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"math"
	"strconv"
	"strings"
)

func main() {
	originalInput, err := inputAsInt64Array("./day11-input.txt")
	if err != nil {
		log.Fatal(err)
	}

	input := make([]int64, len(originalInput))

	copy(input, originalInput)
	part1 := processProgram(input, int64(0))
	fmt.Println("Part 1: ", len(part1))

	copy(input, originalInput)
	part2 := processProgram(input, int64(1))
	fmt.Println("Part 2 (fixed pitch font required):")
	drawRegistration(part2)
}

func drawRegistration(content map[location]panel) {
	minX, maxX, minY, maxY := math.MaxInt32, 0, math.MaxInt32, 0
	for k := range content {
		if k.X < minX {
			minX = k.X
		}
		if k.X > maxX {
			maxX = k.X
		}
		if k.Y < minY {
			minY = k.Y
		}
		if k.Y > maxY {
			maxY = k.Y
		}
	}

	fmt.Println("")
	for y := minY; y <= maxY; y++ {
		for x := minX; x <= maxX; x++ {
			p, ok := content[location{x, y}]
			if ok && p.Colour == 1 {
				fmt.Print("##")
			} else {
				fmt.Print("  ")
			}
		}
		fmt.Println("")
	}
	fmt.Println("")
}

func processProgram(program []int64, initialInput int64) map[location]panel {
	m := make(map[location]panel)
	// Output channel needs room for 2 values (colour and direction)
	inQ := make(chan int64, 1)
	outQ := make(chan int64, 2)

	cX, cY, facing, done, colour, direction := 0, 0, 0, false, int64(0), int64(0)
	go runProgram(program, inQ, outQ)

	inQ <- initialInput
	for !done {
		colour = <-outQ
		if colour == 99 {
			done = true
			break
		}

		p, ok := m[location{cX, cY}]
		count := 1
		if ok {
			count += p.Count
		}
		m[location{cX, cY}] = panel{cX, cY, count, int(colour)}

		direction = <-outQ
		if direction == 99 {
			done = true
			break
		}

		// Directions: [^,>,v,<]
		if direction == 0 {
			facing = (facing + 3) % 4
		} else {
			facing = (facing + 1) % 4
		}

		if facing == 0 {
			cY--
		} else if facing == 1 {
			cX++
		} else if facing == 2 {
			cY++
		} else if facing == 3 {
			cX--
		}

		p, ok = m[location{cX, cY}]
		col := int64(0)
		if ok {
			col = int64(p.Colour)
		}
		inQ <- col
	}

	return m
}

type location struct {
	X int
	Y int
}

type panel struct {
	X      int
	Y      int
	Count  int
	Colour int
}

func resizeProgram(program []int64, newSize int64) ([]int64, int64) {
	result := make([]int64, newSize)
	copy(result, program)
	return result, int64(len(result))
}

func runProgram(program []int64, inQ chan int64, outQ chan int64) (result int64, err error) {
	ip, lastoutput, len := int64(0), int64(0), int64(len(program))
	relativebase := int64(0)
	for {
		instruction := program[ip]
		opcode := instruction % 100
		// Get the parameter modes
		m1 := (instruction / 100) % 10
		m2 := (instruction / 1000) % 10
		m3 := (instruction / 10000) % 10

		// Read the parameters - doesn't matter if we read too many for the current op
		p1, p2, p3 := int64(0), int64(0), int64(0)
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
		v1, v2, v3 := p1, p2, p3
		if m1 == 0 {
			v1 = 0
			if p1 < len {
				v1 = program[p1]
			}
		} else if m1 == 2 {
			v1 = 0
			if (p1 + relativebase) < len {
				v1 = program[p1+relativebase]
			}
		}
		if m2 == 0 {
			v2 = 0
			if p2 < len {
				v2 = program[p2]
			}
		} else if m2 == 2 {
			v2 = 0
			if (p2 + relativebase) < len {
				v2 = program[p2+relativebase]
			}
		}
		if m3 == 2 {
			v3 = p3 + relativebase
		}

		switch opcode {
		case 1: // Add
			if v3 >= len {
				program, len = resizeProgram(program, v3*2)
			}
			program[v3] = v1 + v2
			ip += 4
		case 2: // Multiply
			if v3 >= len {
				program, len = resizeProgram(program, v3*2)
			}
			program[v3] = v1 * v2
			ip += 4
		case 3: // Input
			if m1 == 2 {
				p1 = p1 + relativebase
			}
			if p1 >= len {
				program, len = resizeProgram(program, p1*2)
			}
			select {
			case input := <-inQ:
				program[p1] = input
				ip += 2
			default:
				// Do nothing and just carry on
			}
		case 4: // Output
			lastoutput = v1
			//log.Println("Test result: ", v1)
			ip += 2
			outQ <- v1
		case 5: // Jump true
			if v1 != 0 {
				ip = v2
			} else {
				ip += 3
			}

		case 6: // Jump false
			if v1 == 0 {
				ip = v2
			} else {
				ip += 3
			}

		case 7: // <
			if v3 >= len {
				program, len = resizeProgram(program, v3*2)
			}

			if v1 < v2 {
				program[v3] = 1
			} else {
				program[v3] = 0
			}
			ip += 4
		case 8: // ==
			if v3 >= len {
				program, len = resizeProgram(program, v3*2)
			}
			if v1 == v2 {
				program[v3] = 1
			} else {
				program[v3] = 0
			}
			ip += 4
		case 9:
			relativebase += v1
			ip += 2
			break
		case 99:
			outQ <- int64(99)
			return lastoutput, nil
		default:
			return 0, fmt.Errorf("Unexpected opcode %d", opcode)
		}
	}
}

func inputAsInt64Array(inputFile string) (nums []int64, err error) {
	txt, err := ioutil.ReadFile(inputFile)
	if err != nil {
		return nil, err
	}

	split := strings.Split(string(txt), ",")
	nums = make([]int64, len(split))
	for i, n := range split {
		if len(n) == 0 {
			continue
		}
		num, err := strconv.Atoi(n)
		if err != nil {
			return nil, err
		}
		nums[i] = int64(num)
	}

	return nums, nil
}
