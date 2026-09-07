namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge register definitions for gameplay display capture.</summary>
internal static class GameplayRenderDefinitions
{
    /// <summary>OBSEL=$03: gameplay OBJ uses word $6000, with 8/16-pixel object sizes.</summary>
    internal const byte ObjectSelection = 0x03;
    /// <summary>Setup ASM $8F:C97B sets BG12NBA=$66: both Mode-1 character bases are word $6000.</summary>
    internal const ushort CeresCharacterWord = 0x6000;
}
