using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// The native-state importers' and oracles' reading of a cartridge word (#2604): test code reading the cartridge, which
/// asks whether a word names an identity at all. The game never decodes these words; the import decodes the ones it
/// reads, and a word it does not know fails there (<see cref="NativeWordIdentities{TIdentity}.Decode"/>).
/// </summary>
internal static class NativeWordIdentitiesDecoding
{
    /// <summary>Decoding a word that may name no identity.</summary>
    extension<TIdentity>(NativeWordIdentities<TIdentity> words)
        where TIdentity : notnull
    {
        /// <summary>
        /// Decodes <paramref name="word"/> when it names an identity, from each identity's words
        /// (<see cref="NativeWordIdentities{TIdentity}.WordsByIdentity"/>).
        /// </summary>
        internal bool TryDecode(int word, [MaybeNullWhen(false)] out TIdentity identity)
        {
            if (word is >= 0 and <= ushort.MaxValue)
            {
                foreach ((TIdentity named, ushort[] namedBy) in words.WordsByIdentity)
                {
                    if (Array.IndexOf(namedBy, (ushort)word) >= 0)
                    {
                        identity = named;
                        return true;
                    }
                }
            }
            identity = default;
            return false;
        }
    }
}
