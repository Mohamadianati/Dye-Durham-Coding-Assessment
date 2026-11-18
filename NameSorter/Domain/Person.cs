namespace NameSorter.Domain;

/// <summary>Represents the given names and family name of a person.</summary>
public sealed partial class Person : IEquatable<Person>
{
    private const int MinimumNameParts = 2;
    private const int MaximumNameParts = 4;

    private Person(string[] givenNames, string familyName)
    {
        GivenNames = Array.AsReadOnly(givenNames);
        FamilyName = familyName;
    }

    /// <summary>Gets the person's given names in their original order.</summary>
    public IReadOnlyList<string> GivenNames { get; }

    /// <summary>Gets the person's family name.</summary>
    public string FamilyName { get; }

    /// <summary>Parses one to three given names followed by a family name.</summary>
    /// <param name="fullName">The name with its parts separated by whitespace.</param>
    /// <returns>A person whose name preserves the spelling of each part.</returns>
    /// <exception cref="ArgumentNullException">The name is null.</exception>
    /// <exception cref="FormatException">The name has fewer than two or more than four parts.</exception>
    public static Person Parse(string fullName)
    {
        ArgumentNullException.ThrowIfNull(fullName);

        string[] parts = fullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < MinimumNameParts or > MaximumNameParts)
        {
            throw new FormatException("A name must have one to three given names followed by a family name.");
        }

        return new Person(parts[..^1], parts[^1]);
    }

    /// <summary>Determines whether the other person has the same name parts and spelling.</summary>
    public bool Equals(Person? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(FamilyName, other.FamilyName)
        && GivenNames.SequenceEqual(other.GivenNames, StringComparer.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Person other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(FamilyName, StringComparer.Ordinal);
        foreach (string givenName in GivenNames)
        {
            hash.Add(givenName, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}
