using System.Collections;
using System.Collections.Frozen;

namespace SuperMetroid.Core;

/// <summary>
/// A value for every member of one identity (#2553): a closed choice's definition data, its entries checked once when it is
/// built to name every member, so reading a member's value can never miss and no reader guards against a missing one.
/// </summary>
/// <typeparam name="TIdentity">The identity whose members each have a value.</typeparam>
/// <typeparam name="TValue">The value each member has.</typeparam>
public sealed partial class IdentityTable<TIdentity, TValue> : IEnumerable<KeyValuePair<TIdentity, TValue>>
    where TIdentity : struct, Enum
{
    /// <summary>Each member's value.</summary>
    private readonly FrozenDictionary<TIdentity, TValue> values;

    /// <summary>Keeps <paramref name="values"/>, which names every member of <typeparamref name="TIdentity"/>.</summary>
    /// <exception cref="ArgumentException">A member has no value.</exception>
    public IdentityTable(IReadOnlyDictionary<TIdentity, TValue> values)
    {
        Ensure.NotNull(values);
        TIdentity[] missing = [.. Enum.GetValues<TIdentity>().Where(member => !values.ContainsKey(member))];
        if (missing.Length != 0)
            throw new ArgumentException($"The {typeof(TIdentity).Name} members {string.Join(", ", missing)} have no value.", nameof(values));
        this.values = values.ToFrozenDictionary();
    }

    /// <summary>Gives every member of <typeparamref name="TIdentity"/> the value <paramref name="valueOf"/> makes for it.</summary>
    public IdentityTable(Func<TIdentity, TValue> valueOf)
        : this(Enum.GetValues<TIdentity>().ToDictionary(member => member, Ensure.NotNull(valueOf)))
    {
    }

    /// <summary>The value of <paramref name="identity"/>.</summary>
    public TValue this[TIdentity identity] => values[identity];

    /// <summary>The members, each once.</summary>
    public IEnumerable<TIdentity> Keys => values.Keys;

    /// <summary>The members' values, in the order of <see cref="Keys"/>.</summary>
    public IEnumerable<TValue> Values => values.Values;

    /// <summary>How many members the identity has.</summary>
    public int Count => values.Count;

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TIdentity, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TIdentity, TValue>>)values).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
