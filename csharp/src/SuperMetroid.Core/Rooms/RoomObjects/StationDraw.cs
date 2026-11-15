namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The twenty draws the stations' animations and access arms show (#1994), each named for what it shows.</summary>
internal enum StationDraw
{
    /// <summary>The map station's display at its dimmest.</summary>
    MapDim,
    /// <summary>The map station's display lit.</summary>
    MapLit,
    /// <summary>The map station's display at its brightest.</summary>
    MapBright,
    /// <summary>Retracted right-side map access draw.</summary>
    MapRightRetracted,
    /// <summary>Extended right-side map access draw.</summary>
    MapRightExtended,
    /// <summary>Retracted left-side map access draw.</summary>
    MapLeftRetracted,
    /// <summary>Extended left-side map access draw.</summary>
    MapLeftExtended,
    /// <summary>The energy station's glow at its dimmest.</summary>
    EnergyDim,
    /// <summary>The energy station's glow lit.</summary>
    EnergyLit,
    /// <summary>The energy station's glow at its brightest.</summary>
    EnergyBright,
    /// <summary>The missile station's glow at its dimmest.</summary>
    MissileDim,
    /// <summary>The missile station's glow lit.</summary>
    MissileLit,
    /// <summary>The missile station's glow at its brightest.</summary>
    MissileBright,
    /// <summary>Retracted right-side resource access draw.</summary>
    ResourceRightRetracted,
    /// <summary>Extended right-side resource access draw.</summary>
    ResourceRightExtended,
    /// <summary>Retracted left-side resource access draw.</summary>
    ResourceLeftRetracted,
    /// <summary>Extended left-side resource access draw.</summary>
    ResourceLeftExtended,
    /// <summary>The idle save pod, its floor a trigger and its lights off.</summary>
    SaveIdle,
    /// <summary>The saving pod with its lights on, its floor solid.</summary>
    SaveLit,
    /// <summary>The saving pod with its lights off, its floor solid.</summary>
    SaveDark,
}
