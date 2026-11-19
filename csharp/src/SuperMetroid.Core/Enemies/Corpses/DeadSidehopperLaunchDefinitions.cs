namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The Tourian dead Sidehopper's hops: the cycle of launches it takes after each landing, and its gravity.</summary>
public static class DeadSidehopperLaunchDefinitions
{
    /// <summary>
    /// The launches of the corpse's four-hop cycle, in the order it takes them after each landing, the first following the
    /// fourth (#2432): 1.75, 1.125 and 1.125 pixels an update rightward and 2 upward, then 3 rightward and 4 upward.
    /// </summary>
    public static IReadOnlyList<PixelVelocity<RoomPixels>> Launches { get; } =
    [
        new(SignedEightEight.FromPixelFraction(7, 4), -SignedEightEight.FromPixels(2)), new(SignedEightEight.FromPixelFraction(9, 8), -SignedEightEight.FromPixels(2)),
        new(SignedEightEight.FromPixelFraction(9, 8), -SignedEightEight.FromPixels(2)), new(SignedEightEight.FromPixels(3), -SignedEightEight.FromPixels(4)),
    ];

    /// <summary>
    /// The victim's first hop, as its room loads: three eighths of a pixel an update rightward and a pixel an update
    /// downward.
    /// </summary>
    public static PixelVelocity<RoomPixels> FirstHop { get; } = new(SignedEightEight.FromPixelFraction(3, 8), SignedEightEight.FromPixels(1));

    /// <summary>How much a rising hop slows each update: an eighth of a pixel an update.</summary>
    public static SignedEightEight RisingGravity { get; } = SignedEightEight.FromPixelFraction(1, 8);

    /// <summary>How much a falling hop speeds up each update: half a pixel an update.</summary>
    public static SignedEightEight FallingGravity { get; } = SignedEightEight.FromPixelFraction(1, 2);

}
