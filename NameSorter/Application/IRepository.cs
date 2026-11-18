namespace NameSorter.Application;

/// <summary>Reads and replaces lists of items at a supplied location.</summary>
/// <typeparam name="T">The type of item in the list.</typeparam>
public interface IRepository<T>
{
    /// <summary>Reads every item from the location in its stored order.</summary>
    IReadOnlyList<T> Read(string path);

    /// <summary>Writes the items in sequence, replacing any existing contents.</summary>
    void Write(string path, IEnumerable<T> items);
}
