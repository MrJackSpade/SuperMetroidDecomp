namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// How Draygon carries the Samus he holds (#2300): his chase to her, the spiral he spins her through and the tail whip that
/// pauses it.
/// </summary>
internal static class DraygonGrabDefinitions
{
    /// <summary>Whole-pixel speed of Draygon's direct chase before the carry spiral.</summary>
    internal const int ChaseSpeed = 2;

    /// <summary>The carry spiral's target and initial center.</summary>
    internal static readonly Position16<RoomPixels> SpiralTarget = new(256, 384);

    /// <summary>Initial angular position used when the Samus-carry spiral begins.</summary>
    internal static readonly RotationAngle InitialSpiralAngle = RotationAngle.ThreeQuarterTurn;

    /// <summary>Initial per-frame angular step of the Samus-carry spiral, slowed by <see cref="SpiralTurnSlowing"/> each frame.</summary>
    internal static readonly RotationAngle InitialSpiralAngleDelta = RotationAngle.FromSteps(8);

    /// <summary>How much the spiral's turn each frame slows every frame: the finest slowing an angle keeps.</summary>
    internal static readonly RotationAngle SpiralTurnSlowing = RotationAngle.FromStepFraction(1, 256);

    /// <summary>The carry spiral's per-frame radius growth.</summary>
    internal static readonly SignedSixteenSixteen SpiralRadiusGrowth = SignedSixteenSixteen.FromPixelFraction(1, 8);

    /// <summary>Largest radius reached by the expanding carry spiral.</summary>
    internal const int MaximumSpiralRadius = 160;

    /// <summary>Upper room boundary for the spiral center's vertical lift.</summary>
    internal const int MinimumSpiralCenterY = 64;

    /// <summary>The spiral center's per-frame lift upward.</summary>
    internal static readonly SignedSixteenSixteen SpiralCenterLift = -SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>
    /// The updates Draygon's tail whip pauses his spiral for; his tail starts its whip on the pause's first update.
    /// </summary>
    internal const int TailWhipUpdates = 64;
}
