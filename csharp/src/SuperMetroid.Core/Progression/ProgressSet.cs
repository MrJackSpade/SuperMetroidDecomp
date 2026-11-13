namespace SuperMetroid.Core.Progression;

/// <summary>
/// Which members of a fixed kind of progress have happened (#1553): the story events that have happened, the item
/// locations collected, the doors opened for good. Each member is a named identity; the set lists them in the order its
/// kind declares them and rejects anything that is not one of them.
/// </summary>
/// <typeparam name="T">The kind of progress.</typeparam>
public sealed class ProgressSet<T>
    where T : notnull
{
    /// <summary>
    /// Every member of the kind, in its declared order. A member's place is found in this list itself, so the set holds
    /// nothing derived from its members' hash codes, which a copy of the set made in another process could not use.
    /// </summary>
    private readonly IReadOnlyList<T> _kind;

    /// <summary>Whether each member, by its place, has happened.</summary>
    private readonly bool[] _on;

    /// <summary>Creates the set of <paramref name="kind"/>'s members, none of which has happened.</summary>
    /// <param name="kind">Every member of the kind, in its declared order, each once.</param>
    /// <exception cref="ArgumentException"><paramref name="kind"/> lists a member twice.</exception>
    public ProgressSet(IReadOnlyList<T> kind)
    {
        _kind = Ensure.NotNull(kind);
        var members = new HashSet<T>(kind.Count);
        for (int place = 0; place < kind.Count; place++)
        {
            if (!members.Add(kind[place]))
                throw new ArgumentException($"{kind[place]} is listed twice.", nameof(kind));
        }
        _on = new bool[kind.Count];
    }

    /// <summary>Whether <paramref name="member"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is not one of the kind.</exception>
    public bool Contains(T member) => _on[Place(member)];

    /// <summary>Records that <paramref name="member"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is not one of the kind.</exception>
    public void Add(T member) => _on[Place(member)] = true;

    /// <summary>Records that <paramref name="member"/> has not happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is not one of the kind.</exception>
    public void Remove(T member) => _on[Place(member)] = false;

    /// <summary>Records whether <paramref name="member"/> has happened.</summary>
    /// <param name="member">The member.</param>
    /// <param name="happened">Whether it has happened afterwards.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is not one of the kind.</exception>
    public void Set(T member, bool happened) => _on[Place(member)] = happened;

    /// <summary>Records that every member of the kind has happened.</summary>
    public void AddAll() => Array.Fill(_on, true);

    /// <summary>The members that have happened, in the kind's declared order.</summary>
    public IEnumerable<T> Members
    {
        get
        {
            for (int place = 0; place < _on.Length; place++)
            {
                if (_on[place])
                    yield return _kind[place];
            }
        }
    }

    /// <summary>Makes every member match <paramref name="source"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="source"/> is a set of a different kind.</exception>
    public void CopyFrom(ProgressSet<T> source)
    {
        Ensure.NotNull(source);
        RequireSameKind(source);
        source._on.CopyTo(_on, 0);
    }

    /// <summary>A detached copy of this set.</summary>
    public ProgressSet<T> Copy()
    {
        var copy = new ProgressSet<T>(_kind);
        copy.CopyFrom(this);
        return copy;
    }

    /// <summary>
    /// Applies <paramref name="overwrite"/>: every member it decides has happened when it lists it and has not
    /// otherwise, and every other member keeps its state.
    /// </summary>
    /// <exception cref="ArgumentException">The overwrite turns on a member it does not decide.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The overwrite names a member that is not one of the kind.</exception>
    public void Overwrite(DecidedFlags<T> overwrite)
    {
        Ensure.NotNull(overwrite);
        foreach (T member in overwrite.Set)
        {
            if (!overwrite.Decided.Contains(member))
                throw new ArgumentException($"The overwrite turns on {member}, which it does not decide.", nameof(overwrite));
        }
        foreach (T member in overwrite.Decided)
            Set(member, overwrite.Set.Contains(member));
    }

    /// <summary>The place of <paramref name="member"/>, rejecting anything that is not one of the kind.</summary>
    private int Place(T member)
    {
        for (int place = 0; place < _kind.Count; place++)
        {
            if (EqualityComparer<T>.Default.Equals(_kind[place], member))
                return place;
        }
        throw new ArgumentOutOfRangeException(nameof(member), member, $"{member} is not a {typeof(T).Name} of this set.");
    }

    /// <summary>Whether <paramref name="other"/> lists the same members in the same order.</summary>
    private bool SameKind(ProgressSet<T> other) => ReferenceEquals(other._kind, _kind) || other._kind.SequenceEqual(_kind);

    /// <summary>Rejects a set of a different kind.</summary>
    private void RequireSameKind(ProgressSet<T> other)
    {
        if (!SameKind(other))
            throw new ArgumentException($"A set of another {typeof(T).Name} kind cannot stand for this one.", nameof(other));
    }
}
