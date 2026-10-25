using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Cartridge values and helpers of <see cref="BgCharacterWords"/> that only verification reads.</summary>
internal static class BgCharacterWordsConstants
{
    /// <summary>The VRAM word the cartridge keeps <paramref name="set"/>'s character <paramref name="character"/> from.</summary>
    internal static int WordOf(BgCharacterSet set, int character) => set switch
    {
        BgCharacterSet.Tiles => RoomGraphicsLayout.AreaCharacters.Offset + character * (set.CharacterBytes() / sizeof(ushort)),
        BgCharacterSet.Layer3 => GameplayVramLayout.HudCharacters.Offset + character * (set.CharacterBytes() / sizeof(ushort)),
        BgCharacterSet.KraidLayer3 => Core.Rooms.LibraryBackgroundListConstants.KraidHudCharacterBaseWord + character * (set.CharacterBytes() / sizeof(ushort)),
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown background character set."),
    };
}
