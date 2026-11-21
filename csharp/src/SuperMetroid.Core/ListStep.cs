namespace SuperMetroid.Core;

/// <summary>
/// One entry of an authored list an owner walks in the order the list shows (#2315), a boss's or a growing shutter's: the entry, the entry after it, and the
/// entries after and before it when the walk cycles, the first following the last.
/// </summary>
/// <typeparam name="T">The list's entries.</typeparam>
public readonly record struct ListStep<T>
{
    /// <summary>The list walked.</summary>
    private readonly IReadOnlyList<T> _list;

    /// <summary>The entry at <paramref name="place"/> of <paramref name="list"/>.</summary>
    private ListStep(IReadOnlyList<T> list, int place)
    {
        _list = list;
        Place = place;
    }

    /// <summary>The entry's place in its list, counting from zero.</summary>
    private int Place { get; }

    /// <summary>The entry.</summary>
    public T Entry => _list[Place];

    /// <summary>The entry after this one, or null after the list's last.</summary>
    public ListStep<T>? Following => Place + 1 < _list.Count ? new ListStep<T>(_list, Place + 1) : null;

    /// <summary>
    /// The entry after this one in a walk that comes to rest at the list's last entry: the last is where the walk ends, so
    /// it follows itself.
    /// </summary>
    public ListStep<T> FollowingToRest => Place + 1 < _list.Count ? new ListStep<T>(_list, Place + 1) : this;

    /// <summary>The entry after this one in a cycling walk: the list's first follows its last.</summary>
    public ListStep<T> NextInCycle => new(_list, (Place + 1) % _list.Count);

    /// <summary>The entry before this one in a cycling walk run backwards: the list's last precedes its first.</summary>
    public ListStep<T> PreviousInCycle => new(_list, (Place == 0 ? _list.Count : Place) - 1);

    /// <summary>The first entry of <paramref name="list"/>.</summary>
    /// <exception cref="ArgumentException">The list is empty.</exception>
    public static ListStep<T> FirstOf(IReadOnlyList<T> list) => At(list, 0);

    /// <summary>The entry at <paramref name="place"/> of <paramref name="list"/>, counting from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The list has no such place.</exception>
    public static ListStep<T> At(IReadOnlyList<T> list, int place) =>
        new(Ensure.NotNull(list), Ensure.BetweenInclusive(0, list.Count - 1, place));

    /// <inheritdoc/>
    public override string ToString() => $"{Entry} (place {Place} of {_list.Count})";
}
