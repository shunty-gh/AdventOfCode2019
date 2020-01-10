package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"math"
	"strings"
)

type location struct {
	X, Y, Z int
}

// Planet : A simple map of a planet
type Planet map[location]byte

func main() {
	input, err := getInput("./day24-input.txt")
	//input, err := getTestInput()
	if err != nil {
		log.Fatal(err)
	}

	// Part 1
	eris := initPlanet(input)
	part1 := 0
	ratings := make(map[int]bool) // To completely anal about memory saving we could use map[int]struct{} instead
	for part1 == 0 {              // Assume result isn't 0
		eris = eris.evolve(false)
		rating := eris.rating()
		// Look for the first repeated rating
		if ratings[rating] { // Maps return the zero value (ie false in this case) if key not present. So don't need _,ok :=...
			part1 = rating
		}
		ratings[rating] = true
	}
	//drawWorld(eris)
	fmt.Println("Part 1: ", part1)

	// Part 2
	eris = initPlanet(input)
	for i := 1; i <= 200; i++ {
		eris = eris.evolve(true)
	}
	// Count all bugs
	part2 := 0
	for _, content := range eris {
		if content == '#' {
			part2++
		}
	}
	//drawWorld(eris)
	fmt.Println("Part 2: ", part2)

}

func initPlanet(input []string) Planet {
	result := make(Planet)
	for y, line := range input {
		for x := 0; x < len(line); x++ {
			result[location{x, y, 0}] = line[x]
		}
	}
	return result
}

func (planet Planet) rating() int {
	result := 0
	for l, b := range planet {
		if b == '#' {
			result += int(math.Pow(2, float64(5*l.Y+l.X)))
		}
	}
	return result
}
func (planet Planet) levelBounds() (minLevel int, maxLevel int) {
	minZ, maxZ := 1024, -1024 // Arbitrary starting bounds
	for l := range planet {
		if l.Z < minZ {
			minZ = l.Z
		}
		if l.Z > maxZ {
			maxZ = l.Z
		}
	}
	return minZ, maxZ
}

func (planet Planet) evolve(useLevels bool) Planet {
	minZ, maxZ := 0, 0
	if useLevels {
		minZ, maxZ = planet.levelBounds()
		minZ--
		maxZ++
	}
	result := make(Planet) // Create/return a new copy of the planet with the new positions
	for z := minZ; z <= maxZ; z++ {
		for y := 0; y < 5; y++ {
			for x := 0; x < 5; x++ {
				// Skip the centre if we're using it as the portal to the next level
				if useLevels && x == 2 && y == 2 {
					continue
				}
				loc := location{x, y, z}
				b, ok := planet[loc]
				neighbours := getNeighbourCount(loc, planet)
				// Apply rules
				if b == '#' && neighbours != 1 {
					result[loc] = '.'
				} else if (b == '.' || !ok) && (neighbours == 1 || neighbours == 2) { // if !ok then this will be on another, currently unused, level
					result[loc] = '#'
				} else if ok { // Don't create empty locations on currently unused levels. Just saves a little bit of memory and time.
					result[loc] = b
				}
			}
		}
	}
	return result
}

func (planet Planet) bugsAt(loc location) int {
	b, ok := planet[loc]
	if ok && b == '#' {
		return 1
	}
	return 0
}

func getNeighbourCount(loc location, planet Planet) int {
	// This level neighbours
	neighbours := []location{
		location{0, -1, 0},
		location{1, 0, 0},
		location{0, 1, 0},
		location{-1, 0, 0},
	}
	result := 0
	for _, next := range neighbours {
		result += planet.bugsAt(location{loc.X + next.X, loc.Y + next.Y, loc.Z})
	}

	// Outer (lower) levels
	x, y := loc.X, loc.Y
	if x == 0 || y == 0 || x == 4 || y == 4 {
		z := loc.Z - 1
		if x == 0 {
			result += planet.bugsAt(location{1, 2, z})
		} else if x == 4 {
			result += planet.bugsAt(location{3, 2, z})
		}

		if y == 0 {
			result += planet.bugsAt(location{2, 1, z})
		} else if y == 4 {
			result += planet.bugsAt(location{2, 3, z})
		}
	} else if x == 2 && (y == 1 || y == 3) { // Inner (upper) levels
		yy := 0
		if y == 3 {
			yy = 4
		}
		for xx := range []int{0, 1, 2, 3, 4} {
			result += planet.bugsAt(location{xx, yy, loc.Z + 1})
		}
	} else if y == 2 && (x == 1 || x == 3) {
		xx := 0
		if x == 3 {
			xx = 4
		}
		for yy := range []int{0, 1, 2, 3, 4} {
			result += planet.bugsAt(location{xx, yy, loc.Z + 1})
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

/*
func getTestInput() ([]string, error) {
	return []string{
		"....#",
		"#..#.",
		"#..##",
		"..#..",
		"#....",
	}, nil
}

func drawWorld(planet Planet) {
	min, max := planet.levelBounds()
	for z := min; z <= max; z++ {
		drawWorldLevel(planet, z)
	}
}

func drawWorldLevel(planet Planet, level int) {
	for y := 0; y < 5; y++ {
		for x := 0; x < 5; x++ {
			if b, ok := planet[location{x, y, level}]; ok {
				fmt.Print(string(b))
			} else {
				fmt.Print(".")
			}
		}
		fmt.Println()
	}
	fmt.Println()
}
*/
