using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EmployeeManagementService.Infrastructure.Persistence.Converters;

public sealed class CollectionValueComparer<TItem> : ValueComparer<ICollection<TItem>>
{
    public CollectionValueComparer()
        : base(
            (left, right) => AreEqual(left, right),
            collection => CalculateHashCode(collection),
            collection => CreateSnapshot(collection))
    {
    }

    private static bool AreEqual(ICollection<TItem>? left, ICollection<TItem>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        return left.SequenceEqual(right);
    }

    private static int CalculateHashCode(ICollection<TItem>? collection)
    {
        if (collection is null)
        {
            return 0;
        }

        var hash = new HashCode();

        foreach (var item in collection)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    private static ICollection<TItem> CreateSnapshot(ICollection<TItem>? collection)
        => collection is null ? new List<TItem>() : new List<TItem>(collection);
}
