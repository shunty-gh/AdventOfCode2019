package main

import (
	"container/list"
	"fmt"
	"log"
	"math"
	"os"
	"path"

	"../intcode"
)

type location struct {
	X, Y int
}

func main() {
	dir, _ := os.Getwd()
	input, err := intcode.InputToIntcodeProgram(path.Join(dir, "day15-input.txt"))
	if err != nil {
		log.Fatal(err)
	}

	pc := intcode.PC{InQ: make(chan int64), OutQ: make(chan int64)}
	go pc.RunProgram(input)

	shipMap, err := explore(&pc)
	if err != nil {
		fmt.Println("Error exploring ship.", err)
	}
	drawMap(shipMap)

	// Technically we don't need to find the shortest path because the map/ship paths
	// form a maze with only one route to any point so when we found the O2 supply
	// in our exploration step then the path length at that point was all we need.
	// But we don't *really* know that from the AoC description so, for the sake of
	// completeness, we'll do a BFS search too.
	part1, o2location := shortestPath(shipMap)
	fmt.Println("Part 1: ", part1)

	part2 := fillWithOxygen(shipMap, o2location)
	fmt.Println("Part 2: ", part2)
}

type locationDistance struct {
	Loc  location
	Dist int
}

func fillWithOxygen(ship map[location]int, startAt location) int {
	// BFS using an array as our queue of items to visit
	moves := []location{{0, 1}, {1, 0}, {0, -1}, {-1, 0}}
	filled := map[location]int{startAt: 0}
	heads := []location{startAt}

	for {
		head := heads[0]
		minutes := filled[head]
		for _, move := range moves {
			nextloc := location{head.X + move.X, head.Y + move.Y}
			if _, ok := filled[nextloc]; ok { // Skip if we've already visited
				continue
			}

			if next, ok := ship[nextloc]; ok && next > 0 {
				heads = append(heads, nextloc)
				filled[nextloc] = minutes + 1
			}
		}

		if len(heads) == 1 { // Nowhere left to fill
			return minutes
		}
		heads = heads[1:]
	}
}

func shortestPath(ship map[location]int) (int, location) {
	// BFS using a list as our queue of items to visit
	moves := []location{{0, 1}, {1, 0}, {0, -1}, {-1, 0}}
	visited := make(map[location]int)
	tovisit := list.New()
	tovisit.PushBack(locationDistance{Loc: location{0, 0}, Dist: 0})
	for {
		ld := tovisit.Remove(tovisit.Front()).(locationDistance)
		loc, dist := ld.Loc, ld.Dist
		visited[loc] = dist
		for _, move := range moves {
			nextloc := location{loc.X + move.X, loc.Y + move.Y}
			if _, ok := visited[nextloc]; ok { // Skip if we've already visited
				continue
			}

			if next, ok := ship[nextloc]; ok && next > 0 { // Exists and is not a wall
				if next == 2 { // Found it
					return dist + 1, nextloc
				}
				tovisit.PushBack(locationDistance{Loc: nextloc, Dist: dist + 1})
			}
		}
	}
}

func explore(pc *intcode.PC) (map[location]int, error) {
	// Essentially DFS using a list to keep track of the route
	// we've taken so we can backtrack by providing the necessary
	// reverse travel instruction to the robot.
	x, y := 0, 0
	route := list.New()
	visited := map[location]int{location{x, y}: 1}
	for {
		/*
			Try and move in direction
			  Check if we have already visited. Try next direction
			   if all directions visited then backtrack. Loop
			  Move
			    If status == 0 add node to map and mark as wall. Try next direction
			    If status == 1 add node to map. Loop
				If status == 2 add node to map and note location of O2 system. Loop
			  Backtrack
			    Go back one step in our route. Retry all moves
		*/
		// Try and move. N,S,W,E -> 1,2,3,4. Check if we've been there before.
		dir, dx, dy := 0, 0, 0
		backtrack := true
		for dir = 1; dir <= 4; dir++ {
			dx, dy = x, y
			switch dir {
			case 1:
				dy++
			case 2:
				dy--
			case 3:
				dx--
			case 4:
				dx++
			}
			if _, ok := visited[location{dx, dy}]; !ok { // Found somewhere we haven't been yet
				backtrack = false
				break
			}
		}

		if backtrack && x == 0 && y == 0 {
			// We've been everywhere & seen it all
			break
		}

		if !backtrack {
			// Move
			pc.InQ <- int64(dir)
			status := <-pc.OutQ
			switch status {
			case 0:
				visited[location{dx, dy}] = 0
			case 1:
				fallthrough
			case 2:
				visited[location{dx, dy}] = int(status)
				route.PushBack(dir)
				x, y = dx, dy
				if status == 2 {
					log.Printf("Found oxygen system at (%d, %d) after %d moves", x, y, route.Len())
				}
			default:
				return nil, fmt.Errorf("Unknown status result code %d", status)
			}
		} else {
			// Backtrack: Move the oposite direction we last came in
			lastdir := route.Remove(route.Back()).(int)
			var backdir int
			if lastdir == 1 { // N
				backdir = 2
				y--
			} else if lastdir == 2 { // S
				backdir = 1
				y++
			} else if lastdir == 3 { // W
				backdir = 4
				x++
			} else { // E
				backdir = 3
				x--
			}
			pc.InQ <- int64(backdir)
			<-pc.OutQ // It must succeed, we've already come from here
		}
	}
	return visited, nil
}

func drawMap(ship map[location]int) {
	minX, maxX, minY, maxY := math.MaxInt32, 0, math.MaxInt32, 0
	for k := range ship {
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
	for y := maxY; y >= minY; y-- {
		for x := minX; x <= maxX; x++ {
			if p, ok := ship[location{x, y}]; ok {
				if x == 0 && y == 0 {
					fmt.Print("X")
				} else if p == 0 {
					fmt.Print("#")
				} else if p == 1 {
					fmt.Print(".")
				} else { // p == 2 ie O2 supply
					fmt.Print("O")
				}
			} else {
				fmt.Print("#")
			}
		}
		fmt.Println("")
	}
	fmt.Println("")
}
