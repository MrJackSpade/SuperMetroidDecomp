namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Jump, falling and landing actor definitions used after the reward gesture.</summary>
internal static class EndingRewardJumpDefinitions
{
    /// <summary>Initial velocity applied when the reward jump begins: sixteen pixels an update upward.</summary>
    public static SignedSixteenSixteen LaunchVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-16);
    /// <summary>The jump adds 7/32 pixel each update to the shared velocity before each actor's movement.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(56);
    /// <summary>Crossing this height, above the screen, starts the body's fall and deletes the old head.</summary>
    public const int FallStartY = -80;
    /// <summary>The body stops at the suitless Samus's standing height before its landed animation begins.</summary>
    public static int LandingY => EndingRewardStartDefinitions.Suitless.Position.Y;
    /// <summary>Where the helmeted head is placed as the jump readies it.</summary>
    public static Offset16<ScreenPixels> HelmetedHeadReadied => new(118, 120);
    /// <summary>Where the helmetless head is placed as the jump readies it.</summary>
    public static Offset16<ScreenPixels> HelmetlessHeadReadied => new(120, 120);
    /// <summary>Where the helmeted head is placed as it launches.</summary>
    public static Offset16<ScreenPixels> HelmetedHeadLaunched => new(120, 114);
    /// <summary>Where the helmetless head is placed as it launches.</summary>
    public static Offset16<ScreenPixels> HelmetlessHeadLaunched => new(121, 116);
}
