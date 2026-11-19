using System.Collections.Frozen;

namespace SuperMetroid.Core;

/// <summary>
/// The written-out installed key of every member of one identity (#2520): the one codec that spells the identity in the
/// names the import writes into the installation, in the documents it installs and in the saves the game writes, and reads
/// it back from them. Each key is written out in the table it is built from, so renaming a member never changes a name on
/// disk (SME6460) and no document is read by a member's source name (SME6462).
/// </summary>
/// <typeparam name="TIdentity">The identity whose members are keyed.</typeparam>
public sealed class InstalledKeys<TIdentity>
    where TIdentity : struct, Enum
{
    /// <summary>Each member's key.</summary>
    private readonly FrozenDictionary<TIdentity, string> keys;

    /// <summary>Each key's member.</summary>
    private readonly FrozenDictionary<string, TIdentity> identities;

    /// <summary>Keys every member of <typeparamref name="TIdentity"/> by its entry in <paramref name="keys"/>.</summary>
    /// <param name="keys">Every member's written-out key, each one distinct and holding text.</param>
    /// <exception cref="ArgumentException">A member has no key, a key is empty, or two members share a key.</exception>
    public InstalledKeys(IReadOnlyDictionary<TIdentity, string> keys)
    {
        Ensure.NotNull(keys);
        foreach (TIdentity identity in Enum.GetValues<TIdentity>())
        {
            if (!keys.TryGetValue(identity, out string? key) || string.IsNullOrEmpty(key))
                throw new ArgumentException($"The {typeof(TIdentity).Name} member {identity} has no installed key.", nameof(keys));
        }
        if (keys.Keys.Any(identity => !Enum.IsDefined(identity)))
            throw new ArgumentException($"An installed key names no {typeof(TIdentity).Name} member.", nameof(keys));
        this.keys = keys.ToFrozenDictionary();
        identities = keys.GroupBy(entry => entry.Value, StringComparer.Ordinal).ToFrozenDictionary(
            group => group.Key,
            group => group.Count() == 1 ? group.Single().Key
                : throw new ArgumentException($"Two {typeof(TIdentity).Name} members share the installed key '{group.Key}'.", nameof(keys)),
            StringComparer.Ordinal);
    }

    /// <summary>Every member's key, by member: the table a family keyed member by member reads its names from.</summary>
    public IReadOnlyDictionary<TIdentity, string> ByIdentity => keys;

    /// <summary>The installed key of <paramref name="identity"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The identity is no member of <typeparamref name="TIdentity"/>.</exception>
    [InstalledKey]
    public string Of(TIdentity identity) => Ensure.Listed(keys, identity);

    /// <summary>Whether <paramref name="key"/> is the installed key of a member, and that member, read without allocating.</summary>
    public bool TryParse(ReadOnlySpan<char> key, out TIdentity identity) =>
        identities.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(key, out identity);

    /// <summary>Whether <paramref name="key"/> is the installed key of a member, and that member.</summary>
    public bool TryParse(string? key, out TIdentity identity)
    {
        if (key is not null && identities.TryGetValue(key, out identity))
            return true;
        identity = default;
        return false;
    }
}
