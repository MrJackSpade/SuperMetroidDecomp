namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>How a WorkRobot walks and how an unpowered one falls (#2156).</summary>
internal static class WorkRobotMotionDefinitions
{
    /// <summary>How long a powered WorkRobot waits before its animation's first step, in updates (#2182).</summary>
    internal const int FirstStepDelay = 4;

    /// <summary>How many of its radii ahead of its centre a WorkRobot's ledge probe stands, two (#2284).</summary>
    internal const int LedgeProbeRadii = 2;

    /// <summary>How far below its walk a WorkRobot's ledge probe looks for the floor, one pixel (#2182).</summary>
    internal static SignedSixteenSixteen LedgeProbeDepth { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>How far a WorkRobot's touch pushes Samus away from it each update, in pixels (#2182).</summary>
    internal const int TouchPush = 4;

    /// <summary>What gravity adds to a falling powered WorkRobot's speed each airborne update, half a pixel.</summary>
    internal static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128);
}
