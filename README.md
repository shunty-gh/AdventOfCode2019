# Advent Of Code 2019

A bunch of samples to help solve the [Advent of code 2019](https://adventofcode.com/2019) puzzles.

Including a massively over-engineered C# program (Dotnet Core 3 - but probably works with most versions so long as you change the `TargetFramework` attribute in `aoc.csproj`) to run each days code.

As it's Dotnet Core it's cross platform so even the beardy Linux users can have a go :wink:

## C# Stuff
### Install Dotnet Core

#### Windows, Linux (including Windows Subsystem for Linux (WSL)), Mac

* Go to https://docs.microsoft.com/en-gb/dotnet/core/install/sdk
* Pick your OS
* Follow the instructions

or (as of 2019-12-02)

#### Windows

* Download [Dotnet Core 3.0.1 installer](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.0.101-windows-x64-installer)
* Run it

#### Linux &amp; WSL

(Assuming you have done the one-time registration of the Microsoft key and feed as per the instructions above)

```bash
$> sudo apt-get install dotnet-sdk-3.0
```

#### Mac

* Dunno. Read the instructions.

### Once Dotnet Core is installed

Clone the repo.

```bash
$> cd <project_root>
```

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
$> dotnet run aoc *
```

## LINQPad stuff
*.linq files are [LINQPad](https://www.linqpad.net/) C# scripts.
