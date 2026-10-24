namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// The cartridge words that name the members of one closed identity (#1411): the import pipeline's private knowledge,
/// decoded once. The identity's own values are arbitrary.
/// </summary>
/// <typeparam name="TIdentity">The identity.</typeparam>
internal sealed class NativeWordIdentities<TIdentity>
    where TIdentity : struct, Enum
{
    /// <summary>Each word's identity.</summary>
    private readonly Dictionary<ushort, TIdentity> _identities;
    /// <summary>Each identity's word.</summary>
    private readonly Dictionary<TIdentity, ushort> _words;

    /// <summary>Keeps <paramref name="words"/>, requiring one distinct word per identity.</summary>
    internal NativeWordIdentities(IReadOnlyDictionary<TIdentity, ushort> words)
    {
        _words = new Dictionary<TIdentity, ushort>(words);
        _identities = new Dictionary<ushort, TIdentity>();
        foreach ((TIdentity identity, ushort word) in words)
            if (!_identities.TryAdd(word, identity))
                throw new InvalidDataException($"${word:X4} names both {_identities[word]} and {identity}.");
    }

    /// <summary>Decodes <paramref name="word"/> or throws, naming <paramref name="meaning"/>.</summary>
    internal TIdentity Decode(ushort word, string meaning) => _identities.TryGetValue(word, out TIdentity identity)
        ? identity
        : throw new InvalidDataException($"${word:X4} is not a {meaning} ({typeof(TIdentity).Name}).");

    /// <summary>Decodes <paramref name="word"/> when it names an identity.</summary>
    internal bool TryDecode(ushort word, out TIdentity identity) => _identities.TryGetValue(word, out identity);

    /// <summary>The word naming <paramref name="identity"/>.</summary>
    internal ushort Word(TIdentity identity) => _words.TryGetValue(identity, out ushort word)
        ? word
        : throw new ArgumentOutOfRangeException(nameof(identity), identity, $"{identity} has no cartridge word.");
}
