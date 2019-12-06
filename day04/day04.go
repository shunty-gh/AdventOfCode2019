package main

import "fmt"

func main() {
	// Puzzle input
	const Lower, Upper = 372304, 847060

	var part1, part2, plen = 0, 0, 6
	for i := Lower; i <= Upper; i++ {
		incrementing, haspair, hasdistinctpair := true, false, false
		pwd := fmt.Sprintf("%d", i)

		for i := 1; i < plen; i++ { // We know all the characters are 0..9 so we can ignore UTF-8/Unicode issues
			lch := pwd[i-1]
			rch := pwd[i]

			if rch < lch {
				// Fail. Not incrementing
				incrementing = false
				break
			}

			if lch == rch {
				haspair = true

				// Check if the left or right neighbours (if any) are the
				// same character. Skip if so.
				if i > 1 {
					lneigh := pwd[i-2]
					if lneigh == lch {
						continue
					}
				}
				if i < plen-1 {
					rneigh := pwd[i+1]
					if rneigh == rch {
						continue
					}
				}
				hasdistinctpair = true
			}
		}
		if incrementing && haspair {
			part1++
		}
		if incrementing && hasdistinctpair {
			part2++
		}
	}

	fmt.Println("Part 1: ", part1)
	fmt.Println("Part 2: ", part2)
}
