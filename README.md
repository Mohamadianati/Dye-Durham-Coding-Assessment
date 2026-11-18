# Dye-Durham-Coding-Assessment
A .NET console app that sorts names by family name and given names, with unit tests and GitHub Actions CI.

## Build, run, and test

Install the .NET 9 SDK and run these commands from the repository root.

```text
dotnet build Sorter.sln
dotnet run --project NameSorter -- unsorted-names-list.txt
dotnet test Sorter.sln
```

Open `Sorter.sln` to use Visual Studio.

## Input and output

Provide one name per line. The last word is the family name. The preceding one to three words are the given names. Blank lines are ignored. Sorting compares the family name, then each given name, ignoring case. A shorter sequence comes first when it is a prefix of a longer one, and entries with equal sort keys keep their input order.

The sample file contains:

```text
Zoe Smith
Ada Marie Brown
Ben Smith
Ada Brown
```

The program prints the following names and writes the same lines to `sorted-names-list.txt` in the current working directory, replacing any prior contents.

```text
Ada Brown
Ada Marie Brown
Ben Smith
Zoe Smith
```

An invalid name reports its line number. Errors go to standard error. The exit code is 0 for success, 1 for processing errors, and 2 for incorrect usage.

## Project layout

- `Domain` contains the name parts, validation rules, and computed full name.
- `Application` contains the sorting workflow, its comparer, and the repository contract.
- `Infrastructure` implements the repository for text files.
- `Program.cs` handles the console command and displays the results.
