namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How Mother Brain's rainbow beam pushes Samus around the arena (#2171): toward the wall, toward the middle of the wall,
/// and her fall once the beam lets her go, inside the arena's ceiling, floor and wall.
/// </summary>
internal static class MotherBrainRainbowBeamPushDefinitions
{
    /// <summary>
    /// The speed the beam pushes Samus at: sixteen pixels an update right toward the wall, and the same speed along the
    /// beam's angle for her rise or fall.
    /// </summary>
    internal static readonly SignedEightEight PushSpeed = SignedEightEight.FromPixels(16);

    /// <summary>The whole X of the arena's right wall, the furthest right the push leaves Samus.</summary>
    internal const int WallX = 235;

    /// <summary>The whole Y of the middle of the wall the push draws Samus toward once she has reached the wall.</summary>
    internal const int MiddleOfWallY = 124;

    /// <summary>The speed the push draws Samus toward the middle of the wall at: a quarter of a pixel an update.</summary>
    internal static readonly SignedEightEight MiddleOfWallSpeed = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>The leftward speed Samus's fall from the beam starts at: a pixel an update.</summary>
    internal static readonly SignedEightEight FallStartSpeed = SignedEightEight.FromPixels(-1);

    /// <summary>How much the leftward push slows on each update of Samus's fall from the beam.</summary>
    internal static readonly SignedEightEight FallingPushSlowing = SignedEightEight.FromPixelFraction(1, 128);

    /// <summary>The gravity Samus's fall from the beam adds to her speed each update, downward.</summary>
    internal static readonly PixelVelocity<RoomPixels> FallGravity = new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(3, 32));

    /// <summary>The arena's ceiling: the least whole Y the forced movement leaves Samus at.</summary>
    internal const int Ceiling = 48;

    /// <summary>The arena's floor: the greatest whole Y the forced movement leaves Samus at.</summary>
    internal const int Floor = 192;
}
