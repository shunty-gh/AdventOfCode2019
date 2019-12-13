package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"strconv"
	"strings"
)

const Block int64 = 2
const Paddle int64 = 3
const Ball int64 = 4

type location struct {
	X, Y int64
}

type tile struct {
	X, Y, TileType int64
}

func main() {
	originalInput, err := inputAsInt64Array("./day13-input.txt")
	if err != nil {
		log.Fatal(err)
	}

	input := make([]int64, len(originalInput))

	copy(input, originalInput)
	m := make(map[location]*tile)
	runGame(input, m)
	part1 := 0
	for _, t := range m {
		if t.TileType == Block {
			part1++
		}
	}
	fmt.Println("Part 1: ", part1)

	m = make(map[location]*tile)
	copy(input, originalInput)
	input[0] = 2
	part2 := runGame(input, m)
	fmt.Println("Part 2: ", part2)
}

func runGame(program []int64, m map[location]*tile) int64 {
	inQ := make(chan int64, 6)
	outQ := make(chan int64, 6)

	go runProgram(program, inQ, outQ)

	x, y, id, done, blocks := int64(0), int64(0), int64(0), false, 0
	score, paddleX := int64(0), int64(-1)

	for !done {
		// Read 3 values
		x = <-outQ
		if x == 99 {
			done = true
			break
		}
		y = <-outQ
		if y == 99 {
			done = true
			break
		}
		id = <-outQ
		if id == 99 {
			done = true
			break
		}

		// Update score ?
		if x == -1 && y == 0 {
			score = id
			if blocks == 0 {
				break
			}
			continue
		}

		if id == Paddle {
			// Initial push of the stick
			if paddleX < 0 {
				inQ <- -1 // Doesn't actually matter if -1, 0 or 1
			}
			paddleX = x
		} else if id == Ball {
			// Track the ball with the paddle
			if paddleX >= 0 {
				if x < paddleX {
					inQ <- -1
				} else if x > paddleX {
					inQ <- 1
				} else {
					inQ <- 0
				}
			}
		}

		// Update the map
		key := location{x, y}
		t, ok := m[key]
		if ok {
			if t.TileType == Block && id != Block {
				blocks++
			} else if t.TileType != Block && id == Block {
				blocks--
			}
			t.TileType = id
		} else {
			if id == Block {
				blocks++
			}
			m[key] = &tile{x, y, id}
		}
	}

	return score
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
		if instruction == 99 {
			outQ <- int64(99)
			return lastoutput, nil
		}
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
		num, err := strconv.Atoi(strings.Trim(n, "\r\n"))
		if err != nil {
			return nil, err
		}
		nums[i] = int64(num)
	}

	return nums, nil
}
