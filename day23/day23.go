package main

import (
	"fmt"
	"log"
	"os"
	"path"
	"time"

	"./intcode"
)

type network struct {
	PCs     []*intcode.PC
	PacketQ chan networkPacket
}

type networkPacket struct {
	Address, X, Y int64
}

type natEvent func(x int64, y int64)

const natAddress = 255

func createNetwork(pcCount int, program []int64, onNatReceive natEvent, onNatSend natEvent) network {
	net := network{PCs: make([]*intcode.PC, pcCount), PacketQ: make(chan networkPacket, 1024)}
	natX, natY := int64(0), int64(0)

	// Read (continuously) from the networkPacket queue(channel) and
	// dispatch to other machines appropriately
	go func(lan network) {
		q := lan.PacketQ
		for {
			pk := <-q // Ok to block here. Can't do anything if we don't have a packet to dispatch.
			addr := int(pk.Address)
			if addr == natAddress {
				natX, natY = pk.X, pk.Y
				onNatReceive(pk.X, pk.Y)
			} else {
				//log.Printf("Dispatch msg to %d. X: %d; Y: %d", addr, pk.X, pk.Y)
				lan.PCs[addr].Enqueue(pk.X)
				lan.PCs[addr].Enqueue(pk.Y)
			}
		}
	}(net)

	// Create all the intcode pcs
	for i := 0; i < pcCount; i++ {
		pc := intcode.PC{InQ: make(chan int64), OutQ: make(chan int64)}
		net.PCs[i] = &pc

		// Read from the pc output queue/channel and when
		// we have three items send them, as a complete networkPacket,
		// to the network packet queue(channel)
		go func(pcQ chan int64, packetQ chan networkPacket) {
			packetindex := 0
			packetbuilder := make([]int64, 3)

			for {
				select { // Don't block while waiting for output from the pc
				case qitem := <-pcQ:
					packetbuilder[packetindex] = qitem
					packetindex++
					if packetindex == 3 {
						pk := networkPacket{
							Address: packetbuilder[0],
							X:       packetbuilder[1],
							Y:       packetbuilder[2],
						}
						packetQ <- pk
						packetindex = 0
					}
				default:
					// Yield to let everyone else have bit of processor time
					time.Sleep(1 * time.Millisecond)
				}
			}
		}(pc.OutQ, net.PacketQ)

		// Run each pc in it's own Go routine
		go pc.RunProgram(program)
		// Set the 'network address'
		pc.Enqueue(int64(i))
	}

	// Monitor the network
	go func(lan network) {
		idlecount := 0
		for {
			if allPCsIdle(lan.PCs) {
				idlecount++
				// We must wait enough time for the network to "settle".
				// idlecount > 2 with a sleep time of between 2-5ms seems to achieve that - may
				// need different values on different hardware. Maybe.
				// However... approx 1 in 10 runs part2 gets an incorrect answer
				if idlecount > 2 && !(natX == 0 && natY == 0) {
					idlecount = 0
					// Send the most recent NAT values to address 0
					onNatSend(natX, natY)
					lan.PCs[0].Enqueue(natX)
					lan.PCs[0].Enqueue(natY)
					natX, natY = 0, 0 // Reset so that we don't send the same value without having received them first
				} else {
					// Poke each pc by sending it -1
					for pci := 0; pci < pcCount; pci++ {
						lan.PCs[pci].Enqueue(-1)
					}
				}
			} else {
				idlecount = 0
			}
			time.Sleep(5 * time.Millisecond)
		}
	}(net)

	return net
}

func main() {
	dir, _ := os.Getwd()
	inputname := path.Join(dir, "day23-input.txt")
	input, err := intcode.InputToIntcodeProgram(inputname)
	if err != nil {
		log.Fatal(err)
	}

	lastnaty := int64(0)
	part1, part2 := int64(0), int64(0)
	onNatRecv := func(x int64, y int64) {
		//log.Printf("NAT receive X: %d; Y: %d", x, y)
		if part1 == 0 {
			part1 = y
			fmt.Println("Part 1: ", part1)
		}
	}
	onNatSend := func(x int64, y int64) {
		//log.Printf("NAT send X: %d; Y: %d", x, y)
		if x != 0 && y != 0 {
			if part2 == 0 && lastnaty == y {
				part2 = y
				fmt.Println("Part 2: ", part2)
			}
			lastnaty = y
		}
	}
	// Set up 50 intcode machines
	createNetwork(50, input, onNatRecv, onNatSend)

	done := false
	for !done {
		done = part1 != 0 && part2 != 0
		time.Sleep(5 * time.Millisecond)
	}

	//fmt.Println("Part 1: ", part1)
	//fmt.Println("Part 2: ", part2)
}

func allPCsIdle(pcs []*intcode.PC) bool {
	for pci := 0; pci < 50; pci++ {
		if !pcs[pci].WaitingForInput {
			return false
		}
	}
	return true
}
