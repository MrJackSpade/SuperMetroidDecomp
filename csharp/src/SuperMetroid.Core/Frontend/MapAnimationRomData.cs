namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$82 pause/file-select map animation definitions.</summary>
public static class MapAnimationRomData
{
    /// <summary>$82:C0E8, pointers to three-byte sprite animation records.</summary>
    public const int SpritePrograms = 0x82c0e8;
    /// <summary>$82:C1E4, pointers to sprite base-ID variants; arrow variant words are zero.</summary>
    public const int SpriteBases = 0x82c1e4;
    /// <summary>$82:C10C, three-byte palette animation timing records terminated by $FF.</summary>
    public const int PaletteTiming = 0x82c10c;
    /// <summary>$82:A987, sixteen-color frames copied into CGRAM palette eleven.</summary>
    public const int PaletteColors = 0x82a987;
    /// <summary>$82:A92B replaces colors 176 through 191, including color zero.</summary>
    public const int PaletteDestination = 176;
}
