using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The result panel's page cells, percentage glyphs, counted items and clear-time digit positions (#1416).</summary>
public static class EndingResultTextLayout
{
    /// <summary>Row-major tile-word index 288, column zero of row nine, where the 9-row result panel is placed.</summary>
    public const int ResultPanelFirstCell = 288;

    /// <summary>The tile words of the 32-column, 9-row result panel cleared before the copyright panel.</summary>
    public const int ResultPanelWords = 288;

    /// <summary>Row-major tile-word index 384, column zero of row twelve, where the copyright panel is placed.</summary>
    public const int CopyrightPanelFirstCell = 384;

    /// <summary>Row-major tile-word index 736, column zero of row twenty-three, where the Japanese subtitle is drawn or cleared.</summary>
    public const int JapaneseSubtitleFirstCell = 736;

    /// <summary>The tile words of the two-row, 32-column Japanese subtitle.</summary>
    public const int JapaneseSubtitleWords = 64;

    /// <summary>Tile-word index of the top half of the item percentage's hundreds digit; tens, units and the percent sign follow to its right.</summary>
    public const int PercentageHundredsTopIndex = 462;

    /// <summary>The tilemap word of the upper half of percentage digit zero; adding 0-9 selects the digit.</summary>
    public const ushort DigitTopTile = 0x3860;

    /// <summary>The tilemap word of the lower half of percentage digit zero, one row below its upper half.</summary>
    public const ushort DigitBottomTile = 0x3870;

    /// <summary>The tilemap word of the upper half of the percent sign, after the three digits.</summary>
    public const ushort PercentTopTile = 0x386a;

    /// <summary>The tilemap word of the lower half of the percent sign, one row below its upper half.</summary>
    public const ushort PercentBottomTile = 0x387a;

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
    public const ushort HoursTensX = 156;

    /// <summary>The clear time's hours units digit's screen X.</summary>
    public const ushort HoursUnitsX = 164;

    /// <summary>The clear time's minutes tens digit's screen X, to the right of the colon.</summary>
    public const ushort MinutesTensX = 180;

    /// <summary>The clear time's minutes units digit's screen X.</summary>
    public const ushort MinutesUnitsX = 188;
}
