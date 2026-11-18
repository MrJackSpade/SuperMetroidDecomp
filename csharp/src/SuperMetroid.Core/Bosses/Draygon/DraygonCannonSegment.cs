namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One run of a Draygon wall cannon's pictures, by facing (#2401).</summary>
internal enum DraygonCannonSegment
{
    /// <summary>The right-facing cannon's shield, which it rests on.</summary>
    RightShield,

    /// <summary>The right-facing cannon's hit flash: its shield, then the shield flashing white.</summary>
    RightHitFlash,

    /// <summary>The destroyed right-facing cannon's sparks, flaring, spreading, receding and settling flat.</summary>
    RightSparks,

    /// <summary>The left-facing cannon's shield, which it rests on.</summary>
    LeftShield,

    /// <summary>The left-facing cannon's hit flash: its shield, then the shield flashing white.</summary>
    LeftHitFlash,

    /// <summary>The destroyed left-facing cannon's sparks, flaring, spreading, receding and settling flat.</summary>
    LeftSparks,
}
