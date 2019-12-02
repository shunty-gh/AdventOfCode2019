package main

import (
	"bufio"
	"fmt"
	"log"
	"math"
	"os"
	"strconv"
)

func main() {
	file, err := os.Open(".\\day01-input.txt")
	if err != nil {
		log.Fatal(err)
	}
	defer file.Close()

	// Get a list of integer values
	var input []int
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		val, _ := strconv.Atoi(scanner.Text())
		input = append(input, val)
	}

	p1 := 0
	for _, num := range input {
		p1 += int(math.Floor(float64(num)/3.0)) - 2
	}
	fmt.Println("Part 1:", p1)

	p2 := 0
	for _, fuel := range input {
		for fuel > 0 {
			fuel = int(math.Floor(float64(fuel)/3.0)) - 2
			p2 += int(math.Max(0, float64(fuel)))
		}
	}
	fmt.Println("Part 2:", p2)
}
