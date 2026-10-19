namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The PLM families whose semantic state a slot carries beside its native words.</summary>
internal enum PlmFamily
{
    /// <summary>One of bank $84's permanent-item headers.</summary>
    Collectible,

    /// <summary>The resident scroll trigger.</summary>
    Scroll,

    /// <summary>A yellow, green or red door cap.</summary>
    ColoredDoor,

    /// <summary>A condition-gated grey door cap.</summary>
    GreyDoor,

    /// <summary>A map, refill or save station.</summary>
    Station,

    /// <summary>The resident five-block downward gate.</summary>
    DownwardGate,

    /// <summary>One component of a mirrored eye-door assembly.</summary>
    EyeDoor,

    /// <summary>One of Draygon's three retail cannons.</summary>
    DraygonCannon,

    /// <summary>A door-spawned Wrecked Ship treadmill.</summary>
    WreckedShipTreadmill,
}
