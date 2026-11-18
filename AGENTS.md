# NameSorter project guidance

- Use the existing `Sorter.sln` and target .NET 9.
- Keep name rules and the computed full-name property in the domain model. Preserve the existing file names, including `Person.Partial.cs`.
- Keep the sorting workflow and repository interface in `Application` and the text file implementation in `Infrastructure`.
- Keep command-line arguments, console streams, and exit codes in `Program.cs`.
- Use short XML documentation comments on classes and their public methods. Use inline comments to explain decisions a reader cannot infer from the code.
- Run `dotnet test Sorter.sln` when changing behavior. Reference the GitHub issue number in each related commit.
