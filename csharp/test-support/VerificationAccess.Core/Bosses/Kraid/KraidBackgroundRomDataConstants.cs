using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidBackgroundRomData"/> that only verification reads.</summary>
internal static class KraidBackgroundRomDataConstants
{
    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    /// <summary>Provides the preserved lower-tail word used when checking Kraid background import fidelity.</summary>
    extension(KraidBackgroundRomData)
    {
        /// <summary>First VRAM word restored by <c>$A7:C777</c>: the standard layer-3 characters' first.</summary>
        internal static ushort StandardBg3VramWord => checked((ushort)BgCharacterWordsConstants.WordOf(BgCharacterSet.Layer3, 0));

        /// <summary>VRAM word destination computed by <c>$A7:ADCC-$ADD6</c> for BG1 base zero.</summary>
        internal static ushort RoomBackgroundTileVramWord => checked((ushort)BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, KraidBackgroundRomData.RoomBackgroundFirstCharacter));

        /// <inheritdoc cref="KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord"/>
        internal static int PreservedLowerTailFirstWord => KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord;
    }
    /// <summary>
    /// <c>Tiles_Standard_BG3</c> at <c>$9A:B200</c>, restored by Kraid death functions
    /// <c>$A7:C777-$C7EF</c> after the private BG2 map has overwritten VRAM word $4000.
    /// </summary>
    public const int StandardBg3TilesAddress = 0x9ab200;
}
