using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidBackgroundDefinitions"/> that only verification reads.</summary>
internal static class KraidBackgroundDefinitionsConstants
{
    /// <summary>Provides the room backdrop's character destination as the cartridge's graphics word.</summary>
    extension(KraidBackgroundDefinitions)
    {
        /// <summary>VRAM word destination computed by <c>$A7:ADCC-$ADD6</c> for BG1 base zero.</summary>
        internal static ushort RoomBackgroundTileVramWord => checked((ushort)BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, RoomTileCharacters.KraidBackdropFirst));
    }
}
