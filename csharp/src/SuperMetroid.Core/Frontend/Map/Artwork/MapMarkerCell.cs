namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The menu atlas cells of the regular map markers (bank $82:C232..C262, C298, C38C..C3A8, CF7C..CFE0).</summary>
internal enum MapMarkerCell
{
    /// <summary>$89: the backing behind the position indicator.</summary>
    IndicatorBacking = 0x89,

    /// <summary>$8A: an undefeated boss marker.</summary>
    Boss = 0x8a,

    /// <summary>$8B: a missile refill station.</summary>
    MissileStation = 0x8b,

    /// <summary>$8C: an energy refill station.</summary>
    EnergyStation = 0x8c,

    /// <summary>$8E: a map station.</summary>
    MapStation = 0x8e,

    /// <summary>$8F: one half of the gunship marker, mirrored for the other half.</summary>
    GunshipHalf = 0x8f,

    /// <summary>$9D: one half of a vertical (up or down) area arrow.</summary>
    VerticalArrowHalf = 0x9d,

    /// <summary>$9E: one half of a horizontal (left or right) area arrow.</summary>
    HorizontalArrowHalf = 0x9e,

    /// <summary>$9F: one corner of the defeated-boss cross.</summary>
    DefeatedBossCorner = 0x9f,

    /// <summary>$AF: one corner of the pulsing position indicator.</summary>
    IndicatorCorner = 0xaf,
}