namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>How the four particles a Skree or Metaree bursts into fly out (#2182): the right pair's, which the left pair mirror.</summary>
internal static class MetalParticleBurstDefinitions
{
    /// <summary>How far to the right of the enemy the right pair starts, and to the left the left pair, in pixels.</summary>
    internal const int SideOffset = 6;

    /// <summary>The outer right particle's launch: 1.25 pixels an update right and just over 3 up.</summary>
    internal static readonly PixelVelocity<RoomPixels> OuterRightVelocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(5, 4), SignedEightEight.FromPixelFraction(-769, 256));

    /// <summary>The inner right particle's launch: 0.375 pixels an update right and just over 4 up.</summary>
    internal static readonly PixelVelocity<RoomPixels> InnerRightVelocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(3, 8), SignedEightEight.FromPixelFraction(-1025, 256));

    /// <summary>Every particle's downward acceleration, five sixteenths of a pixel per update each update (#2284).</summary>
    internal static readonly SignedEightEight Gravity = SignedEightEight.FromPixelFraction(5, 16);
}
