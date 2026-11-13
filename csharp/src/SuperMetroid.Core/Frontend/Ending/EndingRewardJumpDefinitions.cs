namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Jump, falling and landing actor definitions used after the reward gesture.</summary>
internal static class EndingRewardJumpDefinitions
{
    /// <summary>Initial velocity applied when the reward jump begins: sixteen pixels an update upward.</summary>
    public static SignedSixteenSixteen LaunchVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-16);
    /// <summary>The jump adds 7/32 pixel per call to the shared velocity before each actor's movement.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(56);
    /// <summary>Crossing this height, above the screen, starts the body's fall and deletes the old head.</summary>
    public const int FallStartY = -80;
    /// <summary>The body stops here before its landed animation begins.</summary>
    public const int LandingY = 136;
}
