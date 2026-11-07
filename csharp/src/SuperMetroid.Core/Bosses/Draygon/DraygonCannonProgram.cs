namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's cannon PLM programs (#1296), one set per facing, in place of their lists.</summary>
internal enum DraygonCannonProgram
{
    /// <summary>Links the hit program and arms the missile wake of the shielded right-facing cannon.</summary>
    RightStart,
    /// <summary>Shows the shield and sleeps.</summary>
    RightIdle,
    /// <summary>Counts a hit, flashes the shield and returns to idle.</summary>
    RightHit,
    /// <summary>Destroys the right-facing cannon.</summary>
    RightDestroy,
    /// <summary>The destroyed right-facing cannon's smoke loop.</summary>
    RightSmoke,
    /// <summary>Links the hit program and arms the missile wake of the shielded left-facing cannon.</summary>
    LeftStart,
    /// <summary>Shows the shield and sleeps.</summary>
    LeftIdle,
    /// <summary>Counts a hit, flashes the shield and returns to idle.</summary>
    LeftHit,
    /// <summary>Destroys the left-facing cannon.</summary>
    LeftDestroy,
    /// <summary>The destroyed left-facing cannon's smoke loop.</summary>
    LeftSmoke,
}
