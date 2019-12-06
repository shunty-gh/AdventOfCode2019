package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"strings"
)

func main() {
	input, err := getInput("./day06-input.txt")
	if err != nil {
		log.Fatal(err)
	}

	// Build a table of all orbits
	orbits := make(map[string]*orbit)
	for _, s := range input {
		if len(s) == 0 {
			continue
		}
		pair := strings.Split(strings.TrimSpace(s), ")")
		pname, cname := pair[0], pair[1]

		// Create the parent entry if necessary
		if _, ok := orbits[pname]; !ok {
			orbits[pname] = &orbit{nil, pname}
		}

		// Add the child element and make sure it is linked to the parent
		if child, ok := orbits[cname]; !ok {
			orbits[cname] = &orbit{orbits[pname], cname}
		} else {
			child.Parent = orbits[pname]
		}
	}

	part1, part2 := 0, 0
	for _, v := range orbits {
		parent := v.Parent
		for ; parent != nil; part1++ {
			parent = parent.Parent
		}
	}
	fmt.Println("Part 1", part1)

	// Part 2 - Build a table of Santa indirect orbits and their respective number of "hops"
	sorbits := make(map[string]int)
	sparent := orbits["SAN"].Parent
	for sindex := 0; sparent != nil; sindex++ {
		sorbits[sparent.Name] = sindex
		sparent = sparent.Parent
	}

	// Find the first common indirect orbit from YOU & Santa
	yparent := orbits["YOU"].Parent
	for ycount := 0; yparent != nil; ycount++ {
		if hops, ok := sorbits[yparent.Name]; ok {
			part2 = ycount + hops
			break
		}
		yparent = yparent.Parent
	}
	fmt.Println("Part 2", part2)
}

type orbit struct {
	Parent *orbit
	Name   string
}

func getInput(inputFile string) ([]string, error) {
	txt, err := ioutil.ReadFile(inputFile)
	if err != nil {
		return nil, err
	}
	return strings.Split(string(txt), "\n"), nil
}
