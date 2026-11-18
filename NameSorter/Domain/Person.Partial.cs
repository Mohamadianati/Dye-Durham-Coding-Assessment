namespace NameSorter.Domain;

public sealed partial class Person
{
    /// <summary>Gets the given names followed by the family name, separated by single spaces.</summary>
    public string FullName => $"{string.Join(' ', GivenNames)} {FamilyName}";
}
