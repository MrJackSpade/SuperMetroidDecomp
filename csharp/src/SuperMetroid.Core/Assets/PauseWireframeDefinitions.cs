using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Four mutually exclusive visuals selected by the compiled Varia/Hi-Jump rule.</summary>
public enum PauseWireframeKind { PowerSuit, PowerSuitHiJump, VariaSuit, VariaSuitHiJump }

/// <summary>Native pause wireframe patch geometry, independent of inventory selection.</summary>
public static class PauseWireframeDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-wireframes.json";
    public const int Count = 4;
    /// <summary>$82:B25F Samus_Wireframe_Tilemaps pointer order: Power, Power/Hi-Jump, Varia, Varia/Hi-Jump.</summary>
    public const int Pointers = PauseMenuRomData.EquipmentTilemapPatchPointerTable;
    /// <summary>$82:B20C copies seventeen rows of eight words from bank $82.</summary>
    public const int Columns = 8, Rows = 17, Cells = Columns * Rows;
    /// <summary>$82:B20C destination starts at EquipmentScreenBG1Tilemap+$1D8, tile (12,7).</summary>
    public const int DestinationByte = 472;
    /// <summary>Native equipment page row stride in bytes.</summary>
    public const int DestinationStride = 64;
    /// <summary>Native equipment page byte size, one 32x32 tilemap.</summary>
    public const int DestinationSize = 2048;
    /// <summary>All four native artwork patches share bank $82.</summary>
    public const int Bank = 0x820000;
}
