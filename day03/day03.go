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
	input, err := getInput("./day03-input.txt")
	if err != nil {
		log.Fatal(err)
	}

	path1 := plotPath(strings.Split(input[0], ","))
	path2 := plotPath(strings.Split(input[1], ","))
	part1, part2 := math.MaxInt32, math.MaxInt32

	for gr, steps1 := range path1 {
		if steps2, ok := path2[gr]; ok {
			// Part 1 - Find the closest (Manhattan distance) common point
			dist := int(math.Abs(float64(gr.X)) + math.Abs(float64(gr.Y)))
			if dist < part1 {
				part1 = dist
			}
			// Part 2 - Find the shortest total distance to a common point
			totalSteps := steps1 + steps2
			if totalSteps < part2 {
				part2 = totalSteps
			}
		}
	}
	fmt.Println("Part 1: ", part1)
	fmt.Println("Part 2: ", part2)
}

// GridRef : A 2D grid reference
type GridRef struct {
	X, Y int
}

func plotPath(path []string) map[GridRef]int {
	result := make(map[GridRef]int)
	x, y, dx, dy := 0, 0, 0, 0
	steps := 0
	for _, instruction := range path {
		if instruction == "" {
			continue
		}
		dir := string(instruction[0])
		dist, _ := strconv.Atoi(instruction[1:])
		switch dir {
		case "R":
			dx, dy = 1, 0
		case "L":
			dx, dy = -1, 0
		case "U":
			dx, dy = 0, 1
		case "D":
			dx, dy = 0, -1
		default:
			// Error
		}

		for d := 0; d < dist; d++ {
			steps++
			x += dx
			y += dy
			result[GridRef{x, y}] = steps
		}
	}
	return result
}

func getInput(inputFile string) ([]string, error) {
	txt, err := ioutil.ReadFile(inputFile)
	if err != nil {
		return nil, err
	}
	return strings.Split(string(txt), "\n"), nil
}
