# Advent Of Code 2019

A bunch of samples to help solve the [Advent of code 2019](https://adventofcode.com/2019) puzzles.

Including a massively over-engineered C# program (Dotnet Core 3 - but probably works with most versions so long as you change the `TargetFramework` attribute in `aoc.csproj`) to run each days code.

As it's Dotnet Core it's cross platform so even beardy (other forms of facial hair are available) Linux users can have a go :wink:

## C# Stuff
### Install Dotnet Core

#### Windows, Linux (including Windows Subsystem for Linux (WSL)), Mac

* Go to https://docs.microsoft.com/en-gb/dotnet/core/install/sdk
* Pick your OS
* Follow the instructions

or (as of 2019-12-02)

#### Windows

* Download [Dotnet Core](https://dotnet.microsoft.com/download) SDK installer
* Run it

#### Linux &amp; WSL

(Assuming you have done the one-time registration of the Microsoft key and feed as per the installation instructions above)

```bash
$> sudo apt-get install dotnet-sdk-3.1
```

#### Mac

* Dunno. Read the instructions.

#### Once Dotnet Core is installed

Clone the repo and go to the repository root directory.

To run code for current day (if we're still in December):
```csharp
$> dotnet run
```

or, to run code for a specific day(s):

```csharp
$> dotnet run aoc <day_no> [<day_no> [<day_no>]]
```

or, for every day so far:

```csharp
$> dotnet run aoc *      // On Windows only
or
$> dotnet run aoc -*
or
$> dotnet run aoc --all
```

## Go stuff
**Warning:** "Go" beginner alert.

To run the Go samples (where XX is 01, 02 etc):

```go
$> cd /<repo_root>/dayXX
$> go run ./dayXX.go
```

## LINQPad stuff
*.linq files are [LINQPad](https://www.linqpad.net/) C# scripts.
