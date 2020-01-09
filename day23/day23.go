package main

import (
	"fmt"
	"log"
	"os"
	"path"
	"time"

	"../intcode"
)

type network struct {
	PCs     []*intcode.PC
	PacketQ chan networkPacket
}

type networkPacket struct {
	Sender, Recipient int
	X, Y              int64
}

type natEvent func(x int64, y int64)

const natAddress = 255

func (net *network) allPCsIdle() bool {
	pcs := net.PCs
	for pci := 0; pci < 50; pci++ {
		if pcs[pci] == nil {
			return false
		}
		if !pcs[pci].WaitingForInput {
			return false
		}
	}
	return true
}

func createNetwork(pcCount int, program []int64, onNatReceive natEvent, onNatSend natEvent) network {
	net := network{PCs: make([]*intcode.PC, pcCount), PacketQ: make(chan networkPacket, 1024)}

	// Create all the intcode pcs
	// For each one:
	//   * create a queue/channel reader to convert the ouput into
	//     network packets without blocking the pc.
	//   * Start each one running
	//   * Set it's Id/address
	for i := 0; i < pcCount; i++ {
		pc := intcode.PC{InQ: make(chan int64), OutQ: make(chan int64)}
		net.PCs[i] = &pc

		// Read from the pc output queue/channel and when
		// we have three items send them, as a complete networkPacket,
		// to the network packet queue(channel)
		go func(pcQ chan int64, packetQ chan networkPacket, pcId int) {
			packetindex := 0
			packetbuilder := make([]int64, 3)

			for {
				select { // Don't block while waiting for output from the pc
				case qitem := <-pcQ:
					packetbuilder[packetindex] = qitem
					packetindex++
					if packetindex == 3 {
						pk := networkPacket{
							Sender:    pcId,
							Recipient: int(packetbuilder[0]),
							X:         packetbuilder[1],
							Y:         packetbuilder[2],
						}
						packetQ <- pk
						packetindex = 0
					}
				default:
					// Yield to let everyone else have bit of processor time
					time.Sleep(1 * time.Millisecond)
				}
			}
		}(pc.OutQ, net.PacketQ, i)

		// Run each pc in it's own Go routine
		go pc.RunProgram(program)
		// Set the 'network address'
		pc.Enqueue(int64(i))
	}

	// Manage/Process the network
	//
	// Read (continuously) from the networkPacket queue(channel) and
	// dispatch to other machines appropriately.
	// If there is no network packet available check the network to
	// see if all the machines are waiting/idle.
	go func(lan network) {
		natX, natY := int64(0), int64(0)
		q := lan.PacketQ
		idlecount := 0

		for {
			select {
			case pk := <-q: // Don't block. 'cos if there's nothing on the channel we want to run the network monitoring
				// Process the received packet
				addr := pk.Recipient
				if addr == natAddress {
					natX, natY = pk.X, pk.Y
					onNatReceive(pk.X, pk.Y)
				} else {
					//log.Printf("Dispatch msg from %d to %d. X: %d; Y: %d", pk.Sender, addr, pk.X, pk.Y)
					lan.PCs[addr].Enqueue(pk.X)
					lan.PCs[addr].Enqueue(pk.Y)
				}
			default:
				// Monitor the network
				if lan.allPCsIdle() {
					idlecount++
					// We must wait enough time for the network to "settle".
					// Just 1 extra loop through seems to work
					if idlecount > 1 && !(natX == 0 && natY == 0) {
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
			}
			time.Sleep(1 * time.Millisecond)
		}
	}(net)

	return net
}

func main() {
	dir, _ := os.Getwd()
	input, err := intcode.InputToIntcodeProgram(path.Join(dir, "day23-input.txt"))
	if err != nil {
		log.Fatal(err)
	}

	var part1, part2, lastnat int64 = 0, 0, 0
	done := make(chan bool)

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
			if part2 == 0 && lastnat == y {
				part2 = y
				fmt.Println("Part 2: ", part2)
				done <- true // Assuming part 2 fnishes after part 1
			}
			lastnat = y
		}
	}
	// Set up 50 intcode machines
	createNetwork(50, input, onNatRecv, onNatSend)
	// Wait/block until we're finished
	<-done
}
