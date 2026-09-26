namespace SuperMetroid.Core.Game;

/// <summary>Authored palette sources and native CGRAM destinations for the Tourian entrance statue.</summary>
public static class TourianStatuePaletteRomData
{
    /// <summary>Sixteen base-decoration OBJ colors at $AA:D785.</summary>
    public const int BaseColors = EnemyRomTablePointers.TourianStatue.BaseDecorationPaletteWords;
    public const int BaseColorCount = 16;
    public const int BaseCgramIndex = 240;

    /// <summary>Sixteen statue OBJ colors at $AA:D765.</summary>
    public const int StatueColors = EnemyRomTablePointers.TourianStatue.StatuePaletteWords;
    public const int StatueColorCount = 16;
    public const int StatueCgramIndex = 160;

    /// <summary>Four four-color eye images at $86:B91E, selected by doubled boss parameters zero through six.</summary>
    public const int EyeColors = TourianStatueRomData.EyeColors;
    public const int EyeRowCount = 4;
    public const int EyeColorCount = 4;
    public const int EyeCgramIndex = 249;

    /// <summary>Eight grey target colors at $87:839C; the animated-tile instruction selects their destination.</summary>
    public const int GreyColors = TourianStatueRomData.GreyColors;
    public const int GreyColorCount = 8;
}
