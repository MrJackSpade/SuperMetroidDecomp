namespace SuperMetroid.Core.Rooms;

/// <summary>The twenty bank-$84 draw lists selected by station animation programs.</summary>
internal enum StationDraw : ushort
{
    /// <summary>First map-station frame draw list at $84:9F25.</summary>
    MapFrame0 = 0x9f25,
    /// <summary>Second map-station frame draw list at $84:9F31.</summary>
    MapFrame1 = 0x9f31,
    /// <summary>Third map-station frame draw list at $84:9F3D.</summary>
    MapFrame2 = 0x9f3d,
    /// <summary>Retracted right-side map access draw at $84:9F49.</summary>
    MapRightRetracted = 0x9f49,
    /// <summary>Extended right-side map access draw at $84:9F55.</summary>
    MapRightExtended = 0x9f55,
    /// <summary>Retracted left-side map access draw at $84:9F5B.</summary>
    MapLeftRetracted = 0x9f5b,
    /// <summary>Extended left-side map access draw at $84:9F67.</summary>
    MapLeftExtended = 0x9f67,
    /// <summary>First energy-station frame draw list at $84:9F6D.</summary>
    EnergyFrame0 = 0x9f6d,
    /// <summary>Second energy-station frame draw list at $84:9F79.</summary>
    EnergyFrame1 = 0x9f79,
    /// <summary>Third energy-station frame draw list at $84:9F85.</summary>
    EnergyFrame2 = 0x9f85,
    /// <summary>First missile-station frame draw list at $84:9F91.</summary>
    MissileFrame0 = 0x9f91,
    /// <summary>Second missile-station frame draw list at $84:9F9D.</summary>
    MissileFrame1 = 0x9f9d,
    /// <summary>Third missile-station frame draw list at $84:9FA9.</summary>
    MissileFrame2 = 0x9fa9,
    /// <summary>Retracted right-side resource access draw at $84:9FB5.</summary>
    ResourceRightRetracted = 0x9fb5,
    /// <summary>Extended right-side resource access draw at $84:9FBB.</summary>
    ResourceRightExtended = 0x9fbb,
    /// <summary>Retracted left-side resource access draw at $84:9FC1.</summary>
    ResourceLeftRetracted = 0x9fc1,
    /// <summary>Extended left-side resource access draw at $84:9FC7.</summary>
    ResourceLeftExtended = 0x9fc7,
    /// <summary>Save-pod idle draw list at $84:9A3F.</summary>
    SaveIdle = 0x9a3f,
    /// <summary>First active save-pod draw list at $84:9A9F.</summary>
    SaveActive = 0x9a9f,
    /// <summary>Second active save-pod draw list at $84:9A6F.</summary>
    SaveAlternate = 0x9a6f,
}
