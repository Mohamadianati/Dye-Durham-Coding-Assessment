using NameSorter.Application;
using NameSorter.Domain;
using NameSorter.Infrastructure;

namespace NameSorter;

/// <summary>Adapts the name sorting workflow to a console command.</summary>
internal static class Program
{
    private const string OutputFileName = "sorted-names-list.txt";

    /// <summary>Runs the command using the current directory and the console streams.</summary>
    private static int Main(string[] args) => Run(args, Directory.GetCurrentDirectory(), Console.Out, Console.Error);

    /// <summary>Runs the command for the supplied directory and reports its exit code.</summary>
    internal static int Run(string[] args, string workingDirectory, TextWriter output, TextWriter error)
    {
        if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            error.WriteLine("Usage: name-sorter <input-file>");
            return 2;
        }

        try
        {
            string inputPath = Path.GetFullPath(args[0], workingDirectory);
            string outputPath = Path.Combine(workingDirectory, OutputFileName);
            var repository = new TextFileRepository<Person>(Person.Parse, person => person.FullName);
            var sorter = new NameSortingService(repository);

            foreach (Person person in sorter.Sort(inputPath, outputPath))
            {
                output.WriteLine(person.FullName);
            }

            return 0;
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException
                                          or UnauthorizedAccessException or ArgumentException
                                          or NotSupportedException)
        {
            error.WriteLine($"Name sorter: {exception.Message}");
            return 1;
        }
    }
}
