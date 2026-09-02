namespace SuperMetroid.Core.Game;

/// <summary>Fixed banks and memory destinations used by the cartridge enemy loader.</summary>
internal static class RoomEnemyRomLayout
{
    /// <summary>Bank containing 64-byte enemy definition records.</summary>
    public const int DefinitionBank = 0xa00000;
    /// <summary>Bank containing room enemy population records.</summary>
    public const int PopulationBank = 0xa10000;
    /// <summary>Bank containing enemy graphics-set records.</summary>
    public const int TilesetBank = 0xb40000;
    /// <summary>First VRAM byte occupied by dynamically staged ordinary enemy tiles.</summary>
    public const int VramByteBase = 0xd800;
    /// <summary>Byte offset separating ordinary enemy staging from the fixed sprite set.</summary>
    public const int OrdinaryStagingOffset = 0x0800;
}
