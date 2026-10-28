
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Kraid data extraction reads (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed colour documents and background artwork.
/// </summary>
internal static class KraidSourceDefinitions
{
    /// <summary>$A7:AAA6, Kraid initialization's sprite palette three: the sixteen initial rock target colours.</summary>
    internal const int InitialTargetColors = 0xa7aaa6;

    /// <summary>Bank $A7, Kraid's code bank: the bank of its bank-local head tilemap pointers.</summary>
    internal const int CodeBank = 0xa70000;

    /// <summary>$B9:FA38, Background_Brinstar_1A_Kraid_Upper: the compressed upper body tilemap.</summary>
    internal const int UpperBodyTilemap = 0xb9fa38;

    /// <summary>$B9:FE3E, Background_Brinstar_1A_Kraid_Lower_0: the compressed lower body tilemap.</summary>
    internal const int LowerBodyTilemap = 0xb9fe3e;

    /// <summary>$A7:A716, Tiles_KraidRoomBackground: the room backdrop's characters, uploaded when Kraid finishes growing and when his defeated room loads.</summary>
    internal const int RoomBackgroundTiles = 0xa7a716;

    /// <summary>The bytes of the room backdrop's character upload: sixteen 4-bpp characters.</summary>
    internal const ushort RoomBackgroundTileBytes = 512;
}
