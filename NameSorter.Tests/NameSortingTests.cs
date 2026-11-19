using NameSorter.Application;
using NameSorter.Domain;
using Xunit;

namespace NameSorter.Tests;

/// <summary>Covers the domain rules, sorting service, and console workflow.</summary>
public sealed class NameSortingTests
{
    [Theory]
    [InlineData("Janet Parsons", "Parsons", "Janet")]
    [InlineData(" Adonis  Julius Archer ", "Archer", "Adonis Julius")]
    [InlineData("Hunter Uriah Mathew Clarke", "Clarke", "Hunter Uriah Mathew")]
    public void Parse_SplitsFamilyNameAndFormatsFullName(string input, string familyName, string givenNames)
    {
        Person person = Person.Parse(input);

        Assert.Equal(familyName, person.FamilyName);
        Assert.Equal(givenNames.Split(' '), person.GivenNames);
        Assert.Equal($"{givenNames} {familyName}", person.FullName);
        Assert.Equal(Person.Parse(person.FullName), person);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Only")]
    [InlineData("One Two Three Four Five")]
    public void Parse_RejectsInvalidNamePartCount(string input)
    {
        Assert.Throws<FormatException>(() => Person.Parse(input));
    }

    [Fact]
    public void Sort_UsesFamilyNameThenEachGivenName()
    {
        string[] input = ["Amy Zee", "Ben Smith", "Ada Zoe SMITH", "Ada Ann Smith", "Ada Smith", "Zane Adams"];
        string[] expected = ["Zane Adams", "Ada Smith", "Ada Ann Smith", "Ada Zoe SMITH", "Ben Smith", "Amy Zee"];
        var repository = new InMemoryRepository(input.Select(Person.Parse).ToArray());

        IReadOnlyList<Person> sorted = new NameSortingService(repository).Sort("input", "output");

        Assert.Equal(expected, sorted.Select(person => person.FullName));
        Assert.Equal(expected, repository.Written.Select(person => person.FullName));
    }

    [Fact]
    public void Sort_RetainsDuplicatesAndPreservesTheOrderOfCaseOnlyTies()
    {
        string[] input = ["Ada Smith", "ada smith", "Ada Smith"];
        var repository = new InMemoryRepository(input.Select(Person.Parse).ToArray());

        IReadOnlyList<Person> sorted = new NameSortingService(repository).Sort("input", "output");

        Assert.Equal(input, sorted.Select(person => person.FullName));
    }

    [Fact]
    public void Run_PrintsSortedNamesAndOverwritesPreviousOutput()
    {
        using var files = new TemporaryDirectory();
        File.WriteAllLines(files.PathFor("names.txt"), ["Zoe Smith", "", "Ada Brown"]);
        string outputFile = files.PathFor("sorted-names-list.txt");
        File.WriteAllText(outputFile, "old output");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        string[] expected = ["Ada Brown", "Zoe Smith"];

        int exitCode = Program.Run(["names.txt"], files.DirectoryPath, stdout, stderr);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Join(Environment.NewLine, expected) + Environment.NewLine, stdout.ToString());
        Assert.Equal(expected, File.ReadAllLines(outputFile));
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void Run_InvalidLineReportsItsNumberAndPreservesPreviousOutput()
    {
        using var files = new TemporaryDirectory();
        File.WriteAllLines(files.PathFor("names.txt"), ["Ada Brown", "", "Only"]);
        string outputFile = files.PathFor("sorted-names-list.txt");
        File.WriteAllText(outputFile, "previous output");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        int exitCode = Program.Run(["names.txt"], files.DirectoryPath, stdout, stderr);

        Assert.Equal(1, exitCode);
        Assert.Contains("line 3", stderr.ToString());
        Assert.Empty(stdout.ToString());
        Assert.Equal("previous output", File.ReadAllText(outputFile));
    }

    [Fact]
    public void Run_EmptyInputCreatesEmptyOutput()
    {
        using var files = new TemporaryDirectory();
        File.WriteAllText(files.PathFor("names.txt"), "");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        int exitCode = Program.Run(["names.txt"], files.DirectoryPath, stdout, stderr);

        Assert.Equal(0, exitCode);
        Assert.Empty(stdout.ToString());
        Assert.Empty(stderr.ToString());
        Assert.Empty(File.ReadAllLines(files.PathFor("sorted-names-list.txt")));
    }

    [Fact]
    public void Run_MissingFileReportsAnError()
    {
        using var files = new TemporaryDirectory();
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        int exitCode = Program.Run(["missing.txt"], files.DirectoryPath, stdout, stderr);

        Assert.Equal(1, exitCode);
        Assert.Contains("missing.txt", stderr.ToString());
        Assert.Empty(stdout.ToString());
        Assert.False(File.Exists(files.PathFor("sorted-names-list.txt")));
    }

    [Fact]
    public void Run_WithoutAnArgumentPrintsUsage()
    {
        using var files = new TemporaryDirectory();
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        int exitCode = Program.Run([], files.DirectoryPath, stdout, stderr);

        Assert.Equal(2, exitCode);
        Assert.Contains("Usage:", stderr.ToString());
        Assert.Empty(stdout.ToString());
    }

    [Fact]
    public void Run_OneThousandNamesWritesEveryNameInOrderToBothOutputs()
    {
        using var files = new TemporaryDirectory();
        string[] expected = Enumerable.Range(0, 1_000)
            .Select(index => $"Given{index % 50:D2} Family{index / 50:D2}")
            .ToArray();
        File.WriteAllLines(files.PathFor("names.txt"), expected.Reverse());
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        int exitCode = Program.Run(["names.txt"], files.DirectoryPath, stdout, stderr);

        Assert.Equal(0, exitCode);
        Assert.Empty(stderr.ToString());
        Assert.Equal(string.Join(Environment.NewLine, expected) + Environment.NewLine, stdout.ToString());
        Assert.Equal(expected, File.ReadAllLines(files.PathFor("sorted-names-list.txt")));
    }

    private sealed class InMemoryRepository(IReadOnlyList<Person> input) : IRepository<Person>
    {
        public IReadOnlyList<Person> Written { get; private set; } = [];

        public IReadOnlyList<Person> Read(string path) => input;

        public void Write(string path, IEnumerable<Person> items) => Written = items.ToArray();
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string DirectoryPath { get; } = Directory.CreateTempSubdirectory("name-sorter-tests-").FullName;

        public string PathFor(string fileName) => Path.Combine(DirectoryPath, fileName);

        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    }
}
