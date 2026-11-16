namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The twelve pictures the reachable Draygon wall-cannon room-object animations draw (#2176): each facing's shield, the
/// shield flashing white when a shot hits it, and the four pictures of the sparks crackling along a destroyed cannon's
/// seam. The sparks' pictures are named for where the arc reaches farthest from the seam, in pixels below the top of the
/// cannon's two-block column.
/// </summary>
internal enum DraygonCannonDraw
{
    /// <summary>The right-facing cannon's closed shield.</summary>
    RightShield,
    /// <summary>The right-facing cannon's shield flashing white when a shot hits it.</summary>
    RightShieldFlash,
    /// <summary>The destroyed right-facing cannon's sparks, reaching farthest out 11 and 20 pixels down.</summary>
    RightSparksPeakingAt11And20,
    /// <summary>The destroyed right-facing cannon's sparks, reaching farthest out 3 and 28 pixels down.</summary>
    RightSparksPeakingAt3And28,
    /// <summary>The destroyed right-facing cannon's sparks, reaching farthest out 6 and 25 pixels down, a pixel short of the others' reach.</summary>
    RightSparksPeakingAt6And25,
    /// <summary>The destroyed right-facing cannon's sparks lying flattest along the seam, two pixels short of the others' reach.</summary>
    RightSparksFlat,
    /// <summary>The left-facing cannon's closed shield.</summary>
    LeftShield,
    /// <summary>The left-facing cannon's shield flashing white when a shot hits it.</summary>
    LeftShieldFlash,
    /// <summary>The destroyed left-facing cannon's sparks, reaching farthest out 11 and 20 pixels down.</summary>
    LeftSparksPeakingAt11And20,
    /// <summary>The destroyed left-facing cannon's sparks, reaching farthest out 3 and 28 pixels down.</summary>
    LeftSparksPeakingAt3And28,
    /// <summary>The destroyed left-facing cannon's sparks, reaching farthest out 6 and 25 pixels down, a pixel short of the others' reach.</summary>
    LeftSparksPeakingAt6And25,
    /// <summary>The destroyed left-facing cannon's sparks lying flattest along the seam, two pixels short of the others' reach.</summary>
    LeftSparksFlat,
}
