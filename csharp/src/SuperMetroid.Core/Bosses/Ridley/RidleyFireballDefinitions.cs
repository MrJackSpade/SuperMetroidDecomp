namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where both Ridleys breathe their fireballs from, how far the afterburn of one spreads (#2232), and how fast and within
/// which arcs they aim them (#2335).
/// </summary>
internal static class RidleyFireballDefinitions
{
    /// <summary>His mouth, where a fireball leaves and is aimed from, while he faces left; facing right mirrors it.</summary>
    internal static readonly Offset16<RoomPixels> MuzzleFacingLeft = new(-25, -43);

    /// <summary>The afterburn stages a fireball breathed with its afterburn spawns on impact.</summary>
    internal const int AfterburnStages = 3;

    /// <summary>The fireball's speed: five pixels per update.</summary>
    internal static readonly SignedEightEight FireballSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The start of the arc a left-facing Ridley aims his fireballs within.</summary>
    internal static readonly TrigonometryAngle LeftFacingAimStart = new(176);

    /// <summary>The end of the arc a left-facing Ridley aims his fireballs within; the clamp keeps this end itself.</summary>
    internal static readonly TrigonometryAngle LeftFacingAimEnd = new(235);

    /// <summary>Below this a left-facing Ridley's aim clamps to the arc's end rather than its start: a quarter turn.</summary>
    internal static readonly TrigonometryAngle LeftFacingAimSplit = new(64);

    /// <summary>The start of the arc a right-facing Ridley aims his fireballs within.</summary>
    internal static readonly TrigonometryAngle RightFacingAimStart = new(21);

    /// <summary>The end of the arc a right-facing Ridley aims his fireballs within; the clamp keeps this end itself.</summary>
    internal static readonly TrigonometryAngle RightFacingAimEnd = new(80);

    /// <summary>From this a right-facing Ridley's aim clamps to the arc's start rather than its end: three quarters of a turn.</summary>
    internal static readonly TrigonometryAngle RightFacingAimSplit = new(192);
}
