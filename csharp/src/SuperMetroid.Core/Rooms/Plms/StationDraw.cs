namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The twenty draw lists selected by station animation programs.</summary>
internal enum StationDraw
{
    /// <summary>First map-station frame draw list.</summary>
    MapFrame0,
    /// <summary>Second map-station frame draw list.</summary>
    MapFrame1,
    /// <summary>Third map-station frame draw list.</summary>
    MapFrame2,
    /// <summary>Retracted right-side map access draw.</summary>
    MapRightRetracted,
    /// <summary>Extended right-side map access draw.</summary>
    MapRightExtended,
    /// <summary>Retracted left-side map access draw.</summary>
    MapLeftRetracted,
    /// <summary>Extended left-side map access draw.</summary>
    MapLeftExtended,
    /// <summary>First energy-station frame draw list.</summary>
    EnergyFrame0,
    /// <summary>Second energy-station frame draw list.</summary>
    EnergyFrame1,
    /// <summary>Third energy-station frame draw list.</summary>
    EnergyFrame2,
    /// <summary>First missile-station frame draw list.</summary>
    MissileFrame0,
    /// <summary>Second missile-station frame draw list.</summary>
    MissileFrame1,
    /// <summary>Third missile-station frame draw list.</summary>
    MissileFrame2,
    /// <summary>Retracted right-side resource access draw.</summary>
    ResourceRightRetracted,
    /// <summary>Extended right-side resource access draw.</summary>
    ResourceRightExtended,
    /// <summary>Retracted left-side resource access draw.</summary>
    ResourceLeftRetracted,
    /// <summary>Extended left-side resource access draw.</summary>
    ResourceLeftExtended,
    /// <summary>Save-pod idle draw list.</summary>
    SaveIdle,
    /// <summary>First active save-pod draw list.</summary>
    SaveActive,
    /// <summary>Second active save-pod draw list.</summary>
    SaveAlternate,
}
