namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>How a WorkRobot walks and how an unpowered one falls (#2156).</summary>
internal static class WorkRobotMotionDefinitions
{
    /// <summary>How far one walk cue moves a WorkRobot, in pixels.</summary>
    internal const int StepDistance = 4;

    /// <summary>The speed an unpowered WorkRobot starts falling at, one pixel an update.</summary>
    internal static SignedSixteenSixteen FallStartSpeed { get; } = SignedSixteenSixteen.FromParts(1, 0);

    /// <summary>What gravity adds to a falling unpowered WorkRobot's speed each airborne update, half a pixel.</summary>
    internal static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128);
}
