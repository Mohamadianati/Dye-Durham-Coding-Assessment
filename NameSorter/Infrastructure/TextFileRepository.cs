using NameSorter.Application;

namespace NameSorter.Infrastructure;

/// <summary>Stores one item per line in a text file using the supplied format.</summary>
/// <typeparam name="T">The type of item in the file.</typeparam>
public sealed class TextFileRepository<T> : IRepository<T>
{
    private readonly Func<string, T> _parse;
    private readonly Func<T, string> _format;

    /// <summary>Creates a repository with functions to convert between items and text lines.</summary>
    public TextFileRepository(Func<string, T> parse, Func<T, string> format)
    {
        _parse = parse ?? throw new ArgumentNullException(nameof(parse));
        _format = format ?? throw new ArgumentNullException(nameof(format));
    }

    /// <summary>Parses the nonblank lines of a text file in order.</summary>
    public IReadOnlyList<T> Read(string path)
    {
        var items = new List<T>();
        int lineNumber = 0;
        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                items.Add(_parse(line));
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"Invalid entry on line {lineNumber}: {exception.Message}", exception);
            }
        }

        return items;
    }

    /// <summary>Writes the items in order, replacing the contents of the destination file.</summary>
    public void Write(string path, IEnumerable<T> items)
    {
        string[] lines = items.Select(_format).ToArray();
        File.WriteAllLines(path, lines);
    }
}
