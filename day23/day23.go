package main

import (
	"fmt"
	"log"
	"os"
	"path"

	"./intcode"
)

func main() {
	dir, _ := os.Getwd()
	inputname := path.Join(dir, "day23-input.txt")
	input, err := intcode.InputToIntcodeProgram(inputname)
	if err != nil {
		log.Fatal(err)
	}

	// Set up 50 intcode machines
	pcs := make([]*intcode.PC, 50)
	for i := 0; i < 50; i++ {
		// Make the input channel non-buffered and the output channel buffered
		pc := intcode.PC{InQ: make(chan int64), OutQ: make(chan int64, 32)}
		pcs[i] = &pc
		go pc.RunProgram(input)
		// Set the 'network address'
		pc.Enqueue(int64(i))
	}

	// Set up a map for each pc used for collecting outputs until we
	// have 3 to create a complete (Address,X,Y) packet
	outputs := make(map[int][]int64)

	part1, part2 := int64(0), int64(0)
	natX, natY, lastnatY := int64(0), int64(0), int64(0)
	done := false
	idlecount := 0
	for !done {
		recvd := false
		for pcindex := 0; pcindex < 50; pcindex++ {
			// Try and get a value from this pc out queue
			pc := pcs[pcindex]
			nextpc := false
			for !nextpc {
				select {
				case output := <-pc.OutQ:
					recvd = true
					packet, ok := outputs[pcindex]
					if !ok {
						outputs[pcindex] = []int64{output}
					} else {
						outputs[pcindex] = append(packet, output)
					}
					// When we have collected 3 outputs from this pc then
					// retrieve them as a packet and send it off to the
					// appropriate receiver
					if len(outputs[pcindex]) == 3 {
						addr := outputs[pcindex][0]
						pX := outputs[pcindex][1]
						pY := outputs[pcindex][2]
						if int64(255) == addr {
							//log.Printf("Received NAT packet from %d: X=%d; Y=%d", pcindex, pX, pY)
							natX = pX
							natY = pY
							if part1 == 0 {
								part1 = pY
							}
						} else {
							//log.Printf("Sending packet from %d to %d: X=%d; Y=%d", pcindex, addr, pX, pY)
							pcs[int(addr)].Enqueue(pX)
							pcs[int(addr)].Enqueue(pY)
						}
						// Clear out the received values
						outputs[pcindex] = nil
					}
				default:
					// Move on to the next pc
					nextpc = true
				}
			}
		}

		// Check if all are idle
		if !recvd && allPCsIdle(pcs) {
			idlecount++

			// Need to give the network a chance to settle so check
			// the idle status at least a couple of times.
			// Not really happy with this as it appears to be quite arbitrary
			// with a few timing issues.
			// However, on my hardware, an idlecount of 2 gives the system
			// enough time to sttle and give a repeatable answer.
			if idlecount > 2 {
				idlecount = 0

				if !(natX == 0 && natY == 0) {
					if natY == lastnatY {
						part2 = natY
						done = true
					}
					//log.Printf("Sending NAT packet to 0: X=%d; Y=%d", natX, natY)
					pcs[0].Enqueue(natX)
					pcs[0].Enqueue(natY)

					lastnatY = natY
					natX = 0
					natY = 0
				}
			} else {
				for pci := 0; pci < 50; pci++ {
					pcs[pci].Enqueue(int64(-1))
				}
			}
		}
	}

	fmt.Println("Part 1: ", part1)
	fmt.Println("Part 2: ", part2)
}

func allPCsIdle(pcs []*intcode.PC) bool {
	for pci := 0; pci < 50; pci++ {
		if !pcs[pci].WaitingForInput {
			return false
		}
	}
	return true
}
