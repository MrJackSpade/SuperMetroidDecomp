using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// The cartridge words that name the members of one closed identity (#1411): the import pipeline's private knowledge,
/// decoded once. The identity's own values are arbitrary. An identity is what a thing does, so several words name one
/// identity when the cartridge has several routines or records doing the same thing (#1624). An identity is an enum member
/// or a definition instance, such as a door setup (#1721), compared as itself.
/// </summary>
/// <typeparam name="TIdentity">The identity.</typeparam>
internal sealed class NativeWordIdentities<TIdentity>
    where TIdentity : notnull
{
    /// <summary>Each word's identity.</summary>
    private readonly Dictionary<int, TIdentity> _identities;

    /// <summary>Keeps <paramref name="words"/>, requiring one distinct word per identity.</summary>
    internal NativeWordIdentities(IReadOnlyDictionary<TIdentity, ushort> words)
        : this(words.Select(entry => KeyValuePair.Create(entry.Value, entry.Key)))
    {
    }

    /// <summary>Keeps each word's identity in <paramref name="identities"/>; several words may name one identity.</summary>
    internal NativeWordIdentities(IReadOnlyDictionary<ushort, TIdentity> identities)
        : this((IEnumerable<KeyValuePair<ushort, TIdentity>>)identities)
    {
    }

    /// <summary>Keeps each word's identity, requiring every word to name one identity.</summary>
    private NativeWordIdentities(IEnumerable<KeyValuePair<ushort, TIdentity>> identities)
    {
        _identities = new Dictionary<int, TIdentity>();
        foreach (KeyValuePair<ushort, TIdentity> entry in identities)
            if (!_identities.TryAdd(entry.Key, entry.Value))
                throw new InvalidDataException($"${entry.Key:X4} names both {_identities[entry.Key]} and {entry.Value}.");
        WordsByIdentity = _identities
            .GroupBy(entry => entry.Value)
            .ToDictionary(group => group.Key, group => group.Select(entry => checked((ushort)entry.Key)).Order().ToArray());
    }

    /// <summary>Each identity's words in rising order: one, or several when the cartridge does the same thing twice.</summary>
    internal IReadOnlyDictionary<TIdentity, ushort[]> WordsByIdentity { get; }

    /// <summary>Decodes <paramref name="word"/> or throws, naming <paramref name="meaning"/>.</summary>
    internal TIdentity Decode(int word, string meaning) => _identities.TryGetValue(word, out TIdentity? identity)
        ? identity
        : throw new InvalidDataException($"${word:X4} is not a {meaning} ({typeof(TIdentity).Name}).");

    /// <summary>Decodes <paramref name="word"/> when it names an identity.</summary>
    internal bool TryDecode(int word, [MaybeNullWhen(false)] out TIdentity identity) => _identities.TryGetValue(word, out identity);

    /// <summary>
    /// The one word naming <paramref name="identity"/>. An identity several words name has no one word, so this throws;
    /// a caller that needs those words reads them all from <see cref="WordsByIdentity"/>.
    /// </summary>
    internal ushort Word(TIdentity identity)
    {
        if (!WordsByIdentity.TryGetValue(identity, out ushort[]? words))
            throw new ArgumentOutOfRangeException(nameof(identity), identity, $"{identity} has no cartridge word.");
        if (words.Length != 1)
        {
            throw new InvalidOperationException(
                $"{identity} is named by {words.Length} cartridge words ({string.Join(", ", words.Select(word => $"${word:X4}"))}), not one.");
        }
        return words[0];
    }
}
