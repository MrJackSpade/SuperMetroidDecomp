using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The HUD characters Kraid's death restores (#1413): his body map overwrote them, and the death sequence puts them back
/// one quarter at a time, as the background artwork bindings place them (#1352).
/// </summary>
internal static class KraidHudRestoration
{
    /// <summary>The bytes of one of the four restored character quarters.</summary>
    internal const ushort QuarterBytes = HudTileAtlasFormat.RestoreQuarterBytes;

    /// <summary>The quarters Kraid's death restores in sequence.</summary>
    internal const int QuarterCount = HudTileAtlasFormat.RestoreQuarterCount;

    /// <summary>The installed art of each restoration phase, indexed by quarter.</summary>
    private static readonly InstalledArtworkId[] QuarterAssets =
    [
        InstalledArtworkId.KraidBg3RestoreQuarter0, InstalledArtworkId.KraidBg3RestoreQuarter1,
        InstalledArtworkId.KraidBg3RestoreQuarter2, InstalledArtworkId.KraidBg3RestoreQuarter3,
    ];

    /// <summary>The installed art of restoration phase <paramref name="quarter"/>.</summary>
    internal static InstalledArtworkId AssetForQuarter(int quarter) => QuarterAssets[quarter];

    /// <summary>Quarter <paramref name="quarter"/> of the HUD characters, placed from its first character.</summary>
    internal static BgCharacterPlacement Placement(int quarter, ReadOnlyMemory<byte> characters) =>
        new(BgArtworkIdentity.Asset(AssetForQuarter(quarter)), BgCharacterSet.Layer3,
            quarter * (QuarterBytes / BgCharacterSet.Layer3.CharacterBytes()), characters);
}
