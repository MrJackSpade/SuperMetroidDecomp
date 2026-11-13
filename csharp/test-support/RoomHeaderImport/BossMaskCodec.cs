using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The bosses a cartridge boss mask names (#1549): the operand by which a room-state selector, a room object or an
/// animated-tile list tests or sets an area's boss byte. Only checks and the room header importer decode masks; the
/// game reads typed boss bits.
/// </summary>
internal static class BossMaskCodec
{
    /// <summary>The bosses mask <paramref name="value"/> names. Unlike a stored byte, a mask names only bosses the game tests.</summary>
    /// <param name="value">The mask byte.</param>
    /// <param name="context">The mask's owner, named in the error.</param>
    /// <exception cref="InvalidDataException">The mask sets a bit that names no boss, or is not a byte.</exception>
    internal static DefeatedBosses Decode(int value, string context)
    {
        if (value is < 0 or > byte.MaxValue)
            throw new InvalidDataException($"{context}: boss mask {value} is not a byte.");
        int unnamed = value & ~ProgressionTableCodec.EncodeBossMask(DefeatedBossKinds.AllKnown);
        if (unnamed != 0)
            throw new InvalidDataException($"{context}: boss mask ${value:X2} sets bits ${unnamed:X2}, which name no boss.");
        return ProgressionTableCodec.StoredBosses(value);
    }
}
