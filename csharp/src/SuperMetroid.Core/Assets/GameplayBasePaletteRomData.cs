namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge sources of the gameplay base palettes.</summary>
internal static class GameplayBasePaletteRomData
{
    /// <summary>$9A:8000: the complete starting CGRAM image.</summary>
    internal const int InitialPalette = 0x9a8000;
    /// <summary>$9A:FC00: the shared gameplay OBJ palette.</summary>
    internal const int CommonSpritePalette = 0x9afc00;
}
