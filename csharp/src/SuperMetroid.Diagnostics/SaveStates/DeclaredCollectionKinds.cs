namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The collections a location declared as a sequence, a map or a set of some type may hold (#2619), each registered the
/// first time a state meets one rather than all of them when the declaration registers: a declaration names several kinds
/// a value may be (an array, a list, an immutable array, a read-only collection, the compiler's own list), and a session
/// holds one, so registering every kind for every declaration built thousands of handlings no state used.
/// </summary>
internal interface IDeclaredCollectionKinds
{
    /// <summary>Registers the handling of <paramref name="kind"/>, one of the kinds the declaration may hold; false for any other.</summary>
    /// <param name="kind">The collection's generic type definition, or <see cref="Array"/> for an array of the element type.</param>
    bool Register(Type kind);

    /// <summary>Registers every kind the declaration may hold.</summary>
    void RegisterAll();
}

/// <summary>What a location declared as a sequence of <typeparamref name="T"/> may hold (<see cref="StateGraphTypes.Sequence{T}"/>).</summary>
/// <typeparam name="T">The element type.</typeparam>
/// <param name="types">The registry.</param>
internal sealed class SequenceKinds<T>(StateGraphTypes types) : IDeclaredCollectionKinds
{
    /// <inheritdoc/>
    public bool Register(Type kind)
    {
        if (kind == typeof(Array))
            types.Array<T>();
        else if (kind == typeof(List<>))
            types.List<T>();
        else if (kind == typeof(System.Collections.Immutable.ImmutableArray<>))
            types.ImmutableArray<T>();
        else if (kind == typeof(System.Collections.ObjectModel.ReadOnlyCollection<>))
            types.ReadOnlyCollection<T>();
        else if (kind == typeof(CompilerListKind))
            types.CompilerList<T>();
        else
            return false;
        return true;
    }

    /// <inheritdoc/>
    public void RegisterAll()
    {
        types.Array<T>();
        types.List<T>();
        types.ImmutableArray<T>();
        types.ReadOnlyCollection<T>();
        types.CompilerList<T>();
    }
}

/// <summary>What a location declared as a map from <typeparamref name="TKey"/> to <typeparamref name="TValue"/> may hold (<see cref="StateGraphTypes.Map{TKey, TValue}"/>).</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
/// <param name="types">The registry.</param>
internal sealed class MapKinds<TKey, TValue>(StateGraphTypes types) : IDeclaredCollectionKinds where TKey : notnull
{
    /// <inheritdoc/>
    public bool Register(Type kind)
    {
        if (kind == typeof(Dictionary<,>))
            types.Dictionary<TKey, TValue>();
        else if (kind == typeof(System.Collections.Frozen.FrozenDictionary<,>))
            types.FrozenDictionary<TKey, TValue>();
        else if (kind == typeof(System.Collections.Immutable.ImmutableDictionary<,>))
            types.ImmutableDictionary<TKey, TValue>();
        else
            return false;
        return true;
    }

    /// <inheritdoc/>
    public void RegisterAll()
    {
        types.Dictionary<TKey, TValue>();
        types.FrozenDictionary<TKey, TValue>();
        types.ImmutableDictionary<TKey, TValue>();
    }
}

/// <summary>What a location declared as a set of <typeparamref name="T"/> may hold (<see cref="StateGraphTypes.Set{T}"/>).</summary>
/// <typeparam name="T">The member type.</typeparam>
/// <param name="types">The registry.</param>
internal sealed class SetKinds<T>(StateGraphTypes types) : IDeclaredCollectionKinds
{
    /// <inheritdoc/>
    public bool Register(Type kind)
    {
        if (kind == typeof(HashSet<>))
            types.HashSet<T>();
        else if (kind == typeof(System.Collections.Frozen.FrozenSet<>))
            types.FrozenSet<T>();
        else if (kind == typeof(System.Collections.Immutable.ImmutableHashSet<>))
            types.ImmutableHashSet<T>();
        else
            return false;
        return true;
    }

    /// <inheritdoc/>
    public void RegisterAll()
    {
        types.HashSet<T>();
        types.FrozenSet<T>();
        types.ImmutableHashSet<T>();
    }
}

/// <summary>Stands for the read-only lists the compiler makes for collection expressions, as a kind a sequence may hold.</summary>
internal static class CompilerListKind;
