package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"strings"
)

type location struct {
	X, Y, Z int
}

type mazeContent struct {
	Content  byte
	PortalId string
}

type portal []location

// Maze : A donut maze
type Maze struct {
	Map     map[location]mazeContent
	Portals map[string]portal
}

func main() {
	input, err := getInput("./day20-input.txt")
	//input, err := getTestInput()
	if err != nil {
		log.Fatal(err)
	}
	maze := buildMaze(input)
	part1 := shortestPath(maze, maze.Portals["AA"][0], maze.Portals["ZZ"][0])
	fmt.Println("Part 1: ", part1)
}

type locationDistance struct {
	Loc  location
	Dist int
}

func shortestPath(maze Maze, start location, target location) int {
	moves := []location{{0, 1, 0}, {1, 0, 0}, {0, -1, 0}, {-1, 0, 0}}
	result := 0
	q := []locationDistance{locationDistance{start, 0}}
	visited := make(map[location]int)
	for len(q) > 0 {
		ld := q[0]
		loc, dist := ld.Loc, ld.Dist

		// Check if we're on a portal
		if mc := maze.Map[loc]; mc.PortalId != "" && mc.PortalId != "AA" {
			port := maze.Portals[mc.PortalId]
			if port[0] == loc {
				q = append(q, locationDistance{port[1], dist + 1})
			} else {
				q = append(q, locationDistance{port[0], dist + 1})
			}
		}
		for _, move := range moves {
			nextloc := location{loc.X + move.X, loc.Y + move.Y, 0}
			if nextloc == target {
				return dist + 1
			}
			if _, ok := visited[nextloc]; ok { // Skip if we've already visited
				continue
			}

			// Check it is a valid place to visit
			if mc, ok := maze.Map[nextloc]; ok && mc.Content == '.' {
				visited[nextloc] = dist + 1
				q = append(q, locationDistance{Loc: nextloc, Dist: dist + 1})
			}
		}

		q = q[1:]
	}
	return result
}

func isPortalLetter(ch byte) bool {
	return ch >= 'A' && ch <= 'Z'
}

func buildMaze(input []string) Maze {
	maxY := len(input)
	maxX := len(input[2]) + 2
	result := Maze{make(map[location]mazeContent), make(map[string]portal)}
	for y, line := range input {
		for x := 0; x < len(line); x++ {
			c := line[x]
			if c == '.' {
				if _, ok := result.Map[location{x, y, 0}]; !ok {
					result.Map[location{x, y, 0}] = mazeContent{Content: c}
				}
			} else if isPortalLetter(c) {
				newportal := false
				var pkey string
				var ploc location
				// Vertical
				if y+1 < maxY && x < len(input[y+1]) && isPortalLetter(input[y+1][x]) {
					newportal = true
					pkey = string(c) + string(input[y+1][x])
					// Below or above?
					if y+2 < maxY && x < len(input[y+2]) && input[y+2][x] == '.' {
						ploc = location{x, y + 2, 0}
					} else {
						ploc = location{x, y - 1, 0}
					}
				} else if x+1 < maxX && isPortalLetter(input[y][x+1]) { // Horizontal
					newportal = true
					pkey = string(c) + string(input[y][x+1])
					// Right or left
					if x+2 < maxX && input[y][x+2] == '.' {
						ploc = location{x + 2, y, 0}
					} else {
						ploc = location{x - 1, y, 0}
					}
				}
				if newportal {
					if p, ok := result.Portals[pkey]; ok {
						result.Portals[pkey] = append(p, ploc)
					} else {
						result.Portals[pkey] = portal{ploc}
					}
					result.Map[ploc] = mazeContent{Content: input[ploc.Y][ploc.X], PortalId: pkey}
				}
			}
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
