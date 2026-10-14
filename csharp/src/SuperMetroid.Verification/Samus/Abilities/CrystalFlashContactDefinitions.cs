using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Address-space overrides for the controlled Crystal Flash contact fixture.</summary>
internal static class CrystalFlashContactDefinitions
{
    /// <summary>Constructed population/tileset offset in their respective retail banks.</summary>
    public const ushort Pointer = 0x8000;
    /// <summary>Constructed population occupies $A1:8000.</summary>
    public const int PopulationAddress = 0xa18000;
    /// <summary>Constructed tileset occupies $B4:8000.</summary>
    public const int TilesetAddress = 0xb48000;
    /// <summary>Retail Ripper enemy header $A0:D47F.</summary>
    public const EnemyDefinitionId RipperHeader = EnemyDefinitionId.Ripper;
    /// <summary>Population flags: process instructions and process off screen.</summary>
    public const ushort Properties = 0x2800;
}
