namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>How the four particles a Skree or Metaree bursts into fly out (#2182): the right pair's, which the left pair mirror.</summary>
internal static class MetalParticleBurstDefinitions
{
    /// <summary>How far to the right of the enemy the right pair starts, and to the left the left pair, in pixels.</summary>
    internal const int SideOffset = 6;

    /// <summary>The outer right particle's launch: 1.25 pixels an update right and just over 3 up.</summary>
    internal static readonly EightEightVector<RoomPixels> OuterRightVelocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(320, -769);

    /// <summary>The inner right particle's launch: 0.375 pixels an update right and just over 4 up.</summary>
    internal static readonly EightEightVector<RoomPixels> InnerRightVelocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(96, -1025);

    /// <summary>Every particle's downward acceleration, 80/256 pixel per update each update (#2284).</summary>
    internal static readonly SignedEightEight Gravity = SignedEightEight.FromParts(0, 80);
}
