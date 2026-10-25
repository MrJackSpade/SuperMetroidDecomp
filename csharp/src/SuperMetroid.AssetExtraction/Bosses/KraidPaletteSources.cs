using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Where the cartridge keeps each of Kraid's named palettes (#1412).</summary>
internal static class KraidPaletteSources
{
    /// <summary>
    /// The cartridge source of the named palette: the operand of the palette load in Kraid's code, independently checked
    /// against A7:A981, AA97, B3BB, B3C2 and C37B; invalid source values are rejected.
    /// </summary>
    /// <param name="source">The named palette.</param>
    /// <returns>The 24-bit SNES bus address of its first little-endian RGB5 color word; no cartridge read is performed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source is not one of the five supported palette identities.</exception>
    internal static int SourceAddress(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop => EnemySourceDefinitions.KraidRoomBackgroundColors,
        KraidPaletteSource.InitialTarget => KraidSourceDefinitions.InitialTargetColors,
        KraidPaletteSource.Health => EnemySourceDefinitions.KraidHealthColors,
        KraidPaletteSource.Secondary => EnemySourceDefinitions.KraidSecondaryColors,
        KraidPaletteSource.DeathArm => EnemySourceDefinitions.KraidDeathArmColors,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };
}
