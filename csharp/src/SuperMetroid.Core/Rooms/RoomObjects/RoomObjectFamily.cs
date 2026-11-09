namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The PLM families whose semantic state a slot carries beside its words.</summary>
internal enum RoomObjectFamily
{
    /// <summary>One's permanent-item headers.</summary>
    Collectible,

    /// <summary>The resident scroll trigger.</summary>
    Scroll,

    /// <summary>The resident five-block downward gate.</summary>
    DownwardGate,

    /// <summary>One of Draygon's three retail cannons.</summary>
    DraygonCannon,

    /// <summary>A door-spawned Wrecked Ship treadmill.</summary>
    WreckedShipTreadmill,

    /// <summary>The Speed Booster escape's lavaquake controller.</summary>
    SpeedBoosterEscape,
}
