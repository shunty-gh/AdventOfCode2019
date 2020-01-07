package intcode

import (
	"fmt"
	"io/ioutil"
	"strconv"
	"strings"
)

// PC : Our little intcode computer
type PC struct {
	InQ, OutQ       chan int64
	WaitingForInput bool
}

// InputToIntcodeProgram : Read in the input file and produce
// an int64 array to be used as the "program" for the Intcode machine
func InputToIntcodeProgram(inputFile string) (nums []int64, err error) {
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

// Enqueue : Push a value onto the PC input queue
func (pc *PC) Enqueue(value int64) {
	pc.InQ <- value
}

// RunProgram : Creates a copy of the programSource and then runs it
func (pc *PC) RunProgram(programSource []int64) (result int64, err error) {
	program := make([]int64, len(programSource))
	copy(program, programSource)

	inQ := pc.InQ
	outQ := pc.OutQ

	ip, lastoutput, len := int64(0), int64(0), int64(len(program))
	relativebase := int64(0)
	for {
		instruction := program[ip]
		opcode := instruction % 100
		if opcode == 99 {
			outQ <- int64(99)
			return lastoutput, nil
		}
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
			pc.WaitingForInput = true
			input := <-inQ // This will block until we get some input
			pc.WaitingForInput = false
			program[p1] = input
			ip += 2
		case 4: // Output
			lastoutput = v1
			ip += 2
			outQ <- v1 // This may or may not block. Depends if it is a buffered channel.
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

func resizeProgram(program []int64, newSize int64) ([]int64, int64) {
	result := make([]int64, newSize)
	copy(result, program)
	return result, int64(len(result))
}
