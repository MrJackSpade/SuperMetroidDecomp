namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The twelve pictures the reachable Draygon wall-cannon room-object animations draw (#2176, #2401): each facing's shield,
/// the shield flashing white when a shot hits it, and the four stages of the sparks crackling along a destroyed cannon's
/// seam, which flare, spread, recede and settle flat over and over.
/// </summary>
internal enum DraygonCannonDraw
{
    /// <summary>The right-facing cannon's closed shield.</summary>
    RightShield,
    /// <summary>The right-facing cannon's shield flashing white when a shot hits it.</summary>
    RightShieldFlash,
    /// <summary>The destroyed right-facing cannon's sparks flaring: the arc leaps out from the seam in two peaks near the middle of the column.</summary>
    RightSparksFlaring,
    /// <summary>The destroyed right-facing cannon's sparks spreading: the arc's two peaks move out to the ends of the column.</summary>
    RightSparksSpreading,
    /// <summary>The destroyed right-facing cannon's sparks receding: the arc's peaks draw back in and sink toward the seam.</summary>
    RightSparksReceding,
    /// <summary>The destroyed right-facing cannon's sparks settled flat along the seam, before they flare again.</summary>
    RightSparksFlat,
    /// <summary>The left-facing cannon's closed shield.</summary>
    LeftShield,
    /// <summary>The left-facing cannon's shield flashing white when a shot hits it.</summary>
    LeftShieldFlash,
    /// <summary>The destroyed left-facing cannon's sparks flaring: the arc leaps out from the seam in two peaks near the middle of the column.</summary>
    LeftSparksFlaring,
    /// <summary>The destroyed left-facing cannon's sparks spreading: the arc's two peaks move out to the ends of the column.</summary>
    LeftSparksSpreading,
    /// <summary>The destroyed left-facing cannon's sparks receding: the arc's peaks draw back in and sink toward the seam.</summary>
    LeftSparksReceding,
    /// <summary>The destroyed left-facing cannon's sparks settled flat along the seam, before they flare again.</summary>
    LeftSparksFlat,
}
