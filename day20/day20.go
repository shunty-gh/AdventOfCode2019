package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"strings"
)

// GridRef : A 2D grid reference
type GridRef struct {
	X, Y int
}

type mazeContent struct {
	Content  byte
	PortalId string
}

// Maze : A donut maze
type Maze struct {
	Map                    map[GridRef]mazeContent
	PortalIds              map[string][]GridRef
	Portals                map[GridRef]string
	MinX, MaxX, MinY, MaxY int
}

func main() {
	input, err := getInput("./day20-input.txt")
	//input, err := getTestInput()
	if err != nil {
		log.Fatal(err)
	}
	maze := buildMaze(input)
	part1 := shortestPath(maze, maze.PortalIds["AA"][0], maze.PortalIds["ZZ"][0], false)
	fmt.Println("Part 1: ", part1)

	part2 := shortestPath(maze, maze.PortalIds["AA"][0], maze.PortalIds["ZZ"][0], true)
	fmt.Println("Part 2: ", part2)
}

type locationToVisit struct {
	Location        GridRef
	Level, Distance int
}

type visitedLocation struct {
	X, Y, Z int
}

func (maze Maze) isPortal(loc GridRef, level int, useLevels bool) bool {
	mc := maze.Map[loc]

	if mc.PortalId == "" || mc.PortalId == "AA" || mc.PortalId == "ZZ" {
		return false
	}

	// If it's an outer portal AND we're on level 0 then it's really a wall
	if useLevels && level == 0 && maze.isOuterPortal(loc) {
		return false
	}
	return true
}

func (maze Maze) isOuterPortal(loc GridRef) bool {
	return loc.X == maze.MinX ||
		loc.Y == maze.MinY ||
		loc.X == maze.MaxX ||
		loc.Y == maze.MaxY
}

func (maze Maze) getPortalExit(loc GridRef, level int, useLevels bool) (exit GridRef, exitLevel int) {
	pkey := maze.Portals[loc]
	exits := maze.PortalIds[pkey]
	exitLevel = level
	if exits[0] == loc {
		exit = exits[1]
	} else {
		exit = exits[0]
	}
	if useLevels {
		// Are we on an inner or outer portal
		if maze.isOuterPortal(loc) {
			// An outer portal
			exitLevel = level - 1
		} else {
			// An inner portal
			exitLevel = level + 1
		}
	}

	return exit, exitLevel
}

func shortestPath(maze Maze, start GridRef, target GridRef, useLevels bool) int {
	moves := []GridRef{{0, 1}, {1, 0}, {0, -1}, {-1, 0}}
	result := 0
	q := []locationToVisit{locationToVisit{start, 0, 0}}
	visited := make(map[visitedLocation]bool)
	for len(q) > 0 {
		vl := q[0]
		loc, lvl, dist := vl.Location, vl.Level, vl.Distance

		// Check if we're on a portal and go to the portal exit if so.
		if maze.isPortal(loc, lvl, useLevels) {
			port, newlevel := maze.getPortalExit(loc, lvl, useLevels)
			visited[visitedLocation{port.X, port.Y, newlevel}] = true
			q = append(q, locationToVisit{port, newlevel, dist + 1})
		}
		for _, move := range moves {
			nextloc := GridRef{loc.X + move.X, loc.Y + move.Y}
			if nextloc == target && lvl == 0 {
				return dist + 1
			}
			if _, ok := visited[visitedLocation{nextloc.X, nextloc.Y, lvl}]; ok { // Skip if we've already visited
				continue
			}

			// Check it is a valid place to visit
			if mc, ok := maze.Map[nextloc]; ok && mc.Content == '.' {
				visited[visitedLocation{nextloc.X, nextloc.Y, lvl}] = true
				q = append(q, locationToVisit{nextloc, lvl, dist + 1})
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
	ylen := len(input)
	xlen := len(input[2]) + 2
	result := Maze{
		Map:       make(map[GridRef]mazeContent),
		PortalIds: make(map[string][]GridRef),
		Portals:   make(map[GridRef]string)}

	for y, line := range input {
		for x := 0; x < len(line); x++ {
			c := line[x]
			if c == '.' {
				if _, ok := result.Map[GridRef{x, y}]; !ok {
					result.Map[GridRef{x, y}] = mazeContent{Content: c}
				}
			} else if isPortalLetter(c) {
				newportal := false
				var pkey string
				var pref GridRef
				// Vertical
				if y+1 < ylen && x < len(input[y+1]) && isPortalLetter(input[y+1][x]) {
					newportal = true
					pkey = string(c) + string(input[y+1][x])
					// Below or above?
					if y+2 < ylen && x < len(input[y+2]) && input[y+2][x] == '.' {
						pref = GridRef{x, y + 2}
					} else {
						pref = GridRef{x, y - 1}
					}
				} else if x+1 < xlen && isPortalLetter(input[y][x+1]) { // Horizontal
					newportal = true
					pkey = string(c) + string(input[y][x+1])
					// Right or left
					if x+2 < xlen && input[y][x+2] == '.' {
						pref = GridRef{x + 2, y}
					} else {
						pref = GridRef{x - 1, y}
					}
				}
				if newportal {
					if p, ok := result.PortalIds[pkey]; ok {
						result.PortalIds[pkey] = append(p, pref)
					} else {
						result.PortalIds[pkey] = []GridRef{pref}
					}
					if _, ok := result.Portals[pref]; !ok {
						result.Portals[pref] = pkey
					}
					result.Map[pref] = mazeContent{Content: input[pref.Y][pref.X], PortalId: pkey}
				}
			}
		}
	}
	// Find the grid bounds
	minx, miny, maxx, maxy := xlen, ylen, -1, -1
	for gr := range result.Map {
		if gr.X > maxx {
			maxx = gr.X
		} else if gr.X < minx {
			minx = gr.X
		}
		if gr.Y > maxy {
			maxy = gr.Y
		} else if gr.Y < miny {
			miny = gr.Y
		}
	}
	result.MinX = minx
	result.MinY = miny
	result.MaxX = maxx
	result.MaxY = maxy

	return result
}

func getInput(inputFile string) ([]string, error) {
	txt, err := ioutil.ReadFile(inputFile)
	if err != nil {
		return nil, err
	}
	return strings.Split(string(txt), "\n"), nil
}
