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

    /// <summary>Whether <paramref name="left"/> is the longer length, which nothing tests: the compiler requires it beside <c>&lt;</c>.</summary>
    public static bool operator >(ListLength left, ListLength right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> is no longer, which nothing tests: the compiler requires it beside <c>&gt;=</c>.</summary>
    public static bool operator <=(ListLength left, ListLength right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> is no shorter, which only checks test.</summary>
    public static bool operator >=(ListLength left, ListLength right) => left.CompareTo(right) >= 0;

    /// <summary>Whether the length holds entries, which nothing tests: it goes together with <c>false</c>.</summary>
    public static bool operator true(ListLength length) => length.Entries > 0;

    /// <summary>Whether the length is empty, which nothing tests: it goes together with <c>true</c>.</summary>
    public static bool operator false(ListLength length) => length.Entries == 0;

    /// <summary>Whether two lengths are equal, which production asks.</summary>
    public bool Equals(ListLength other) => Entries == other.Entries;

    /// <summary>The length's hash, which nothing asks for directly: the compiler expects it beside <see cref="Equals(ListLength)"/>.</summary>
    public override int GetHashCode() => Entries;

    /// <inheritdoc />
    public int CompareTo(ListLength other) => Entries.CompareTo(other.Entries);
}
