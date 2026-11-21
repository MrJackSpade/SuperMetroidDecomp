namespace SuperMetroid.Core.Lists;

/// <summary>A list's length in entries, which compares as a plain number.</summary>
public readonly record struct ListLength : IComparable<ListLength>
{
    /// <summary>A length of <paramref name="entries"/> entries.</summary>
    public ListLength(int entries) => Entries = entries;

    /// <summary>The entries.</summary>
    public int Entries { get; }

    /// <summary>Whether <paramref name="left"/> is the shorter length, which production tests.</summary>
    public static bool operator <(ListLength left, ListLength right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> is the longer length, which nothing tests: C# requires it beside <c>&lt;</c>.</summary>
    public static bool operator >(ListLength left, ListLength right) => left.CompareTo(right) > 0;

    /// <summary>Whether two lengths are equal, which production asks.</summary>
    public bool Equals(ListLength other) => Entries == other.Entries;

    /// <summary>The length's hash, which nothing asks for directly: C# expects it beside <see cref="Equals(ListLength)"/>.</summary>
    public override int GetHashCode() => Entries;

    /// <inheritdoc />
    public int CompareTo(ListLength other) => Entries.CompareTo(other.Entries);
}
