using NameSorter.Domain;

namespace NameSorter.Application;

/// <summary>Sorts a list of people and saves it at the requested destination.</summary>
public sealed class NameSortingService
{
    private readonly IRepository<Person> _repository;
    private readonly IComparer<Person> _comparer;

    /// <summary>Creates the sorting workflow with a repository and an optional ordering strategy.</summary>
    public NameSortingService(IRepository<Person> repository, IComparer<Person>? comparer = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _comparer = comparer ?? new FamilyNameThenGivenNamesComparer();
    }

    /// <summary>Reads, sorts, and saves the names, then returns them in the same order.</summary>
    public IReadOnlyList<Person> Sort(string inputPath, string outputPath)
    {
        Person[] sorted = _repository.Read(inputPath).OrderBy(person => person, _comparer).ToArray();
        _repository.Write(outputPath, sorted);
        return sorted;
    }

    /// <summary>Compares family names first, followed by given names from left to right.</summary>
    private sealed class FamilyNameThenGivenNamesComparer : IComparer<Person>
    {
        private static readonly StringComparer NameComparison = StringComparer.OrdinalIgnoreCase;

        public int Compare(Person? left, Person? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            int familyOrder = NameComparison.Compare(left.FamilyName, right.FamilyName);
            if (familyOrder != 0)
            {
                return familyOrder;
            }

            int sharedCount = Math.Min(left.GivenNames.Count, right.GivenNames.Count);
            for (int index = 0; index < sharedCount; index++)
            {
                int givenNameOrder = NameComparison.Compare(left.GivenNames[index], right.GivenNames[index]);
                if (givenNameOrder != 0)
                {
                    return givenNameOrder;
                }
            }

            // A name that is a prefix of another comes first.
            return left.GivenNames.Count.CompareTo(right.GivenNames.Count);
        }
    }
}
