using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The result page's panel and percentage cells, counted items and clear-time digit positions (#1416, #1467).</summary>
public static class EndingResultTextLayout
{
    /// <summary>The cell the nine-row result panel's upper-left corner sits in: column zero of row nine.</summary>
    public static Position16<TilemapCells> ResultPanelCell { get; } = new(0, 9);

    /// <summary>The cell the two-row copyright panel's upper-left corner sits in: column zero of row twelve.</summary>
    public static Position16<TilemapCells> CopyrightPanelCell { get; } = new(0, 12);

    /// <summary>The cell the two-row Japanese subtitle's upper-left corner sits in: column zero of row twenty-three.</summary>
    public static Position16<TilemapCells> JapaneseSubtitleCell { get; } = new(0, 23);

    /// <summary>The cell of the item percentage's hundreds digit; tens, units and the percent sign follow to its right.</summary>
    public static Position16<TilemapCells> PercentageHundredsCell { get; } = new(14, 14);

    /// <summary>The items that each count one toward the ending item percentage; ammo and energy pickups are counted separately.</summary>
    public const SamusEquipmentFlags CollectibleItemMask =
        SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall |
        SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster |
        SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope;

    /// <summary>The beams that each count one toward the ending item percentage.</summary>
    public const SamusBeamFlags CollectibleBeamMask =
        SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Charge;

    /// <summary>The clear time's hours tens digit's screen X.</summary>
    public const int HoursTensX = 156;

    /// <summary>The clear time's hours units digit's screen X.</summary>
    public const int HoursUnitsX = 164;

    /// <summary>The clear time's minutes tens digit's screen X, to the right of the colon.</summary>
    public const int MinutesTensX = 180;

    /// <summary>The clear time's minutes units digit's screen X.</summary>
    public const int MinutesUnitsX = 188;
}
