namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Stable resource names and gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
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

    /// <summary>Required case-sensitive icon keys in zero-based layout order: Missile, SuperMissile, PowerBomb, Grapple and XRay. The array reference is fixed but its entries are not protected from caller mutation.</summary>
    public static readonly string[] IconNames = ["Missile", "SuperMissile", "PowerBomb", "Grapple", "XRay"];

    /// <summary>The item icons in <see cref="IconNames"/> order, the order the HUD's layout lists them in.</summary>
    internal static readonly HudIcon[] Icons = [HudIcon.Missile, HudIcon.SuperMissile, HudIcon.PowerBomb, HudIcon.Grapple, HudIcon.XRay];

    /// <summary>The selectable item each icon in <see cref="IconNames"/> order shows; the beam selection has no icon.</summary>
    internal static readonly SuperMetroid.Core.Samus.HudItem[] IconItems =
    [
        SuperMetroid.Core.Samus.HudItem.Missile, SuperMetroid.Core.Samus.HudItem.SuperMissile,
        SuperMetroid.Core.Samus.HudItem.PowerBomb, SuperMetroid.Core.Samus.HudItem.Grapple, SuperMetroid.Core.Samus.HudItem.XRay,
    ];

    /// <summary>Fourteen energy tanks in two rows of seven; this excludes the numeric energy count.</summary>
    public const int EnergyTankCount = 14;
    /// <summary>Five selectable item icons; the beam selection is not an icon.</summary>
    public const int ItemCount = 5;
    /// <summary>Six parts in the two-column, three-row AUTO label and arrows.</summary>
    public const int AutoReserveCellCount = 6;

}
