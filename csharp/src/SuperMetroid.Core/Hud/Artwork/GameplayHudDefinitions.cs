namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Stable resource names and gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
{
    /// <summary>Installed JSON filename for the gameplay HUD's artwork and layout.</summary>
    public const string FileName = "gameplay-hud.json";
    /// <summary>Width of each HUD row in eight-pixel cells.</summary>
    public const int Width = 32;
    /// <summary>Number of HUD rows the HUD's values are drawn over, below the static top row.</summary>
    public const int Height = 3;
    /// <summary>Ninety-six cells in the three rows the HUD's values are drawn over; the static top row is excluded.</summary>
    public const int CellCount = Width * Height;
    /// <summary>The drawn rows' size in cells.</summary>
    public static Extent16<TilemapCells> Size => new(Width, Height);
    /// <summary>Thirty-two cells in the static minimap-border row.</summary>
    public const int TopRowCellCount = Width;

    /// <summary>Required case-sensitive icon keys in zero-based layout order: Missile, SuperMissile, PowerBomb, Grapple and XRay. The array reference is fixed but its entries are not protected from caller mutation.</summary>
    public static readonly string[] IconNames = ["Missile", "SuperMissile", "PowerBomb", "Grapple", "XRay"];

    /// <summary>The selectable item each icon in <see cref="IconNames"/> order shows; the beam selection has no icon.</summary>
    internal static readonly SuperMetroid.Core.Samus.HudItem[] IconItems =
    [
        SuperMetroid.Core.Samus.HudItem.Missile, SuperMetroid.Core.Samus.HudItem.SuperMissile,
        SuperMetroid.Core.Samus.HudItem.PowerBomb, SuperMetroid.Core.Samus.HudItem.Grapple, SuperMetroid.Core.Samus.HudItem.XRay,
    ];

    /// <summary>Fourteen energy tanks in two seven-cell rows; this excludes the numeric energy count.</summary>
    public const int EnergyTankCount = 14;
    /// <summary>Five selectable item icons; the beam selection is not an icon.</summary>
    public const int ItemCount = 5;
    /// <summary>Six cells in the two-column, three-row AUTO label and arrows.</summary>
    public const int AutoReserveCellCount = 6;

    /// <summary>The HUD's ammunition counters: missiles, Super Missiles and Power Bombs, the first three icons' items.</summary>
    internal const int AmmoCounterCount = 3;
}
