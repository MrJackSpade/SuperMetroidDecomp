namespace SuperMetroid.Core.Game;

/// <summary>Authored palette sources and native CGRAM destinations for the Tourian entrance statue.</summary>
public static class TourianStatuePaletteRomData
{
    /// <summary>Sixteen base-decoration OBJ colors at $AA:D785.</summary>
    public const int BaseColors = EnemyRomTablePointers.TourianStatue.BaseDecorationPaletteWords;
    /// <summary>Sixteen RGB5 words in native <c>Palettes_TourianStatue_BaseDecoration</c>, covering the full OBJ palette, including color zero.</summary>
    public const int BaseColorCount = 16;
    /// <summary>First CGRAM color-entry index of the base-decoration band: 240, OBJ palette 7 color zero; native initialization targets <c>TargetPalettes_SpriteP7</c>.</summary>
    public const int BaseCgramIndex = 240;

    /// <summary>Sixteen statue OBJ colors at $AA:D765.</summary>
    public const int StatueColors = EnemyRomTablePointers.TourianStatue.StatuePaletteWords;
    /// <summary>Sixteen RGB5 words in native <c>Palettes_TourianStatue_Phantoon</c>, installed as the statue's full OBJ palette, including color zero.</summary>
    public const int StatueColorCount = 16;
    /// <summary>First CGRAM color-entry index of the statue band: 160, OBJ palette 2 color zero; native initialization targets <c>TargetPalettes_SpriteP2</c>.</summary>
    public const int StatueCgramIndex = 160;

    /// <summary>Four four-color eye images at $86:B91E, selected by doubled boss parameters zero through six.</summary>
    public const int EyeColors = TourianStatueRomData.EyeColors;
    /// <summary>Four boss-specific eye-glow rows, ordered Phantoon, Ridley, Draygon, and Kraid for native doubled selectors 0, 2, 4, and 6.</summary>
    public const int EyeRowCount = 4;
    /// <summary>Four adjacent RGB5 colors per eye-glow row; each native row occupies eight bytes.</summary>
    public const int EyeColorCount = 4;
    /// <summary>First CGRAM color-entry index of the shared eye-glow band: 249, OBJ palette 7 color 9; the selected row replaces entries 249..252.</summary>
    public const int EyeCgramIndex = 249;

    /// <summary>Eight grey target colors at $87:839C; the animated-tile instruction selects their destination.</summary>
    public const int GreyColors = TourianStatueRomData.GreyColors;
    /// <summary>Eight RGB5 words in the native grey-transition target band; the animated-tile destination operand chooses where these colors are installed rather than a fixed CGRAM index.</summary>
    public const int GreyColorCount = 8;
}
