using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyFrameCartridgeLookupTooling"/> that only verification reads.</summary>
internal static class EnemyFrameCartridgeLookupToolingConstants
{
    /// <summary>
    /// The bank-local word the installed frame shown from bank <paramref name="bank"/>'s record at <paramref name="word"/> is
    /// stored at: the word itself, or, for a record that stores a frame's picture again (#2210, #2272), that frame's own word.
    /// </summary>
    internal static ushort OwnWord(byte bank, ushort word) =>
        EnemyFrameCartridgeLocationDefinitions.AlsoStoredAt.TryGetValue(new SnesAddress(bank, word), out EnemyFrameId frame)
            ? EnemyFrameCartridgeLookup.Of(frame).Offset
            : word;
}
