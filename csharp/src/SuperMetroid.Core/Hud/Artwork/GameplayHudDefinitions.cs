namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>The gameplay HUD's layout: its size, its top row, its digits and minimap squares, and its energy tanks.</summary>
public static partial class GameplayHudDefinitions
{
    /// <summary>The HUD's width in pixels: it spans the screen.</summary>
    public const int WidthPixels = ScreenGeometry.ScreenWidthPixels;
    /// <summary>The HUD's height in pixels: the static top row and the band its values are drawn over.</summary>
    public const int HeightPixels = ScreenGeometry.GameplayHudHeightPixels;
    /// <summary>The static top row's height in pixels; the HUD's values are drawn below it.</summary>
    public const int TopRowHeightPixels = 8;
    /// <summary>Pixels across one counter digit: each digit after a counter's first is shown this far right of the one before.</summary>
    public const int DigitWidthPixels = 8;
    /// <summary>Pixels across and down one minimap square: each square of the area map it shows is drawn this size.</summary>
    public const int MinimapSquarePixels = 8;
    /// <summary>The decimal numerals a counter's digits show.</summary>
    public const int Numerals = 10;

    /// <summary>Fourteen energy tanks in two rows of seven; this excludes the numeric energy count.</summary>
    public const int EnergyTankCount = 14;

    /// <summary>The digits of the energy count, the energy beyond full tanks: two.</summary>
    public const int EnergyCountDigits = 2;

    /// <summary>The digits of the missile counter: three.</summary>
    public const int MissileDigits = 3;

    /// <summary>The digits of the Super Missile counter: two.</summary>
    public const int SuperMissileDigits = 2;

    /// <summary>The digits of the Power Bomb counter: two.</summary>
    public const int PowerBombDigits = 2;

    /// <summary>The minimap's size: five area-map squares across and three down.</summary>
    public static Extent16<MapTiles> MinimapSize { get; } = new(5, 3);
}
