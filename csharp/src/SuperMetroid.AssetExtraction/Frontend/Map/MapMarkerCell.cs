namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>The menu atlas cells of the regular map markers (bank $82:C232..C262, C298, C38C..C3A8, CF7C..CFE0).</summary>
internal enum MapMarkerCell
{
    /// <summary>The backing behind the position indicator.</summary>
    IndicatorBacking,

    /// <summary>An undefeated boss marker.</summary>
    Boss,

    /// <summary>A missile refill station.</summary>
    MissileStation,

    /// <summary>An energy refill station.</summary>
    EnergyStation,

    /// <summary>A map station.</summary>
    MapStation,

    /// <summary>One half of the gunship marker, mirrored for the other half.</summary>
    GunshipHalf,

    /// <summary>One half of a vertical (up or down) area arrow.</summary>
    VerticalArrowHalf,

    /// <summary>One half of a horizontal (left or right) area arrow.</summary>
    HorizontalArrowHalf,

    /// <summary>One corner of the defeated-boss cross.</summary>
    DefeatedBossCorner,

    /// <summary>One corner of the pulsing position indicator.</summary>
    IndicatorCorner,
}