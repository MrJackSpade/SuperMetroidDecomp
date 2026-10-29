using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidBackgroundDefinitions"/> that only verification reads.</summary>
internal static class KraidBackgroundDefinitionsConstants
{
    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    /// <summary>Provides the preserved lower-tail word used when checking Kraid background import fidelity.</summary>
    extension(KraidBackgroundDefinitions)
    {
        /// <summary>VRAM word destination computed by <c>$A7:ADCC-$ADD6</c> for BG1 base zero.</summary>
        internal static ushort RoomBackgroundTileVramWord => checked((ushort)BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, RoomTileCharacters.KraidBackdropFirst));

        /// <inheritdoc cref="KraidBackgroundDefinitionsConstants.PreservedLowerTailFirstWord"/>
        internal static int PreservedLowerTailFirstWord => KraidBackgroundDefinitionsConstants.PreservedLowerTailFirstWord;
    }
}
