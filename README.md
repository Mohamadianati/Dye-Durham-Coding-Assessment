# Dye & Durham Name Sorter

## Overview

Name Sorter is a .NET 9 console app for the Dye & Durham coding assessment. It reads a text file, sorts its entries by family name and then by given names, prints the result, and writes the same list to `sorted-names-list.txt`.

## Requirements

- .NET 9 SDK. The repository's `global.json` selects an installed .NET 9 SDK.
- A text file with one name per line.
- Read access to the input file and write access to the directory where the command runs.
- Visual Studio is optional. To use it, open `Sorter.sln`.

## Build and run

Run these commands from the repository root.

```text
dotnet build Sorter.sln
dotnet run --project NameSorter/NameSorter.csproj -- "unsorted-names-list.txt"
```

Pass one argument after `--` to use a different input file. Both absolute and relative paths work. Keep the quotes around paths that contain spaces. Relative paths and the output location are based on the current working directory.

## Input and output

The final word of each line is the family name. The preceding one to three words are given names. Blank lines are ignored, and whitespace between name parts is normalized to single spaces in the output.

Names are compared without regard to case. When family names match, given names are compared from left to right. A shorter sequence comes first if it is a prefix of a longer one. Duplicate names are kept, and entries with equal sort keys keep their input order.

This is an example input file.

```text
Zoe Smith
Ada Marie Brown
Ben Smith
Ada Brown
```

Both the console and `sorted-names-list.txt` contain the following lines.

```text
Ada Brown
Ada Marie Brown
Ben Smith
Zoe Smith
```

A successful run creates or replaces the output file. An invalid name reports its line number to standard error and stops before writing output. The exit codes are `0` for success, `1` for a processing error, and `2` for incorrect command usage.

## Architecture

The console project uses Clean Architecture-style folders. The test project is separate. Domain rules and the sort workflow can be tested independently of the console and file system.

| Component | Responsibility |
| --- | --- |
| [Domain](NameSorter/Domain) | `Person.cs` owns the name parts and validation rules. `Person.Partial.cs` exposes the computed `FullName`. |
| [Application](NameSorter/Application) | `NameSortingService` coordinates reading, sorting, and saving. `IRepository<T>` defines the read and write contract. |
| [Infrastructure](NameSorter/Infrastructure) | `TextFileRepository<T>` handles text file I/O and adds line numbers to parse errors. |
| [Console entry point](NameSorter/Program.cs) | `Program.cs` wires the dependencies together, resolves paths, and handles console output, errors, and exit codes. |

### Domain-Driven Design (DDD)

`Person` is an immutable value object for the name being sorted. It enforces the given-name count when a name is parsed, and its equality is based on its name parts. `FullName` is derived from those parts. The domain model has no dependency on the console or on file storage.

### Generic Repository and Strategy

`IRepository<T>` defines operations to read and replace a list of items. `TextFileRepository<T>` takes parsing and formatting functions, so the same file-handling code can work with any type that can be represented by one text line. At startup, `Program.cs` supplies `Person.Parse` and a function that reads `Person.FullName`. Unit tests give the same service an in-memory repository.

The service accepts an `IComparer<Person>` as its ordering strategy. The default comparer sorts by family name and then given names. A different comparer can be injected without changing the workflow.

### SOLID in practice

| Principle | How it appears in the code |
| --- | --- |
| Single Responsibility | `Person` owns name rules, the comparer owns ordering, and the text repository owns file access. |
| Open/Closed | The service can accept a different comparer or repository without changes to its workflow. |
| Liskov Substitution | The service uses the same read and write contract with the text repository and the in-memory test implementation. |
| Interface Segregation | The repository contract exposes only the two operations the workflow needs. |
| Dependency Inversion | `NameSortingService` depends on `IRepository<Person>` and `IComparer<Person>`. The console entry point supplies the concrete dependencies. |

## Run the tests

This command runs the full xUnit suite, including unit and file integration tests.

```text
dotnet test Sorter.sln
```

Use this filter to run only the parsing and sorting unit tests.

```text
dotnet test Sorter.sln --filter "FullyQualifiedName~NameSortingTests.Parse_|FullyQualifiedName~NameSortingTests.Sort_"
```

The unit tests cover parsing, full-name formatting, sort order, and duplicates. The integration tests check error reporting, console and file output, overwriting an existing file, and a list of 1,000 names. GitHub Actions builds the solution and runs the full suite on pull requests targeting `main` and pushes to `main`.
