namespace SuperMetroid.Core;

/// <summary>
/// An enum's members in value order: the one definition of a sequence whose stages are declared in order, so a stage's
/// successor is never restated as a table.
/// </summary>
internal static class EnumOrder
{
    /// <summary>The member after <paramref name="value"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is undefined or the last member.</exception>
    internal static T Next<T>(T value) where T : struct, Enum
    {
        T[] members = Members<T>.Ordered;
        int index = Array.IndexOf(members, value);
        if (index < 0 || index + 1 == members.Length)
            throw new InvalidOperationException($"{typeof(T).Name} {value} has no successor.");
        return members[index + 1];
    }

    /// <summary>The member after <paramref name="value"/>, wrapping from the last member to the first, as a ring does.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is undefined.</exception>
    internal static T NextWrapping<T>(T value) where T : struct, Enum
    {
        T[] members = Members<T>.Ordered;
        int index = Array.IndexOf(members, value);
        if (index < 0)
            throw new InvalidOperationException($"{typeof(T).Name} {value} is undefined.");
        return members[(index + 1) % members.Length];
    }

    /// <summary>The members of one enum, in value order.</summary>
    private static class Members<T> where T : struct, Enum
    {
        /// <summary>The members, each value once.</summary>
        internal static readonly T[] Ordered = Distinct(Enum.GetValues<T>());

        /// <summary>Rejects an enum whose members share a value, where a successor would be ambiguous.</summary>
        private static T[] Distinct(T[] values) =>
            values.Distinct().Count() == values.Length
                ? values
                : throw new InvalidOperationException($"{typeof(T).Name} has members that share a value.");
    }
}
