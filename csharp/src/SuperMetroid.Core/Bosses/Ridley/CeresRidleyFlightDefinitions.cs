namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Ceres Ridley flies into his fireball position and how fast he may seek (#2171).</summary>
internal static class CeresRidleyFlightDefinitions
{
    /// <summary>The least vertical speed Ridley keeps, either way, while he moves into his fireball position: half a pixel an update.</summary>
    internal static readonly SignedEightEight FireballMoveMinimumSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>The fastest 1/256-pixel speed his seek clamps to toward a lower target: five pixels per update.</summary>
    internal static readonly SignedEightEight MaximumSeekSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The fastest 1/256-pixel speed toward a higher target: five pixels per update the other way.</summary>
    internal static readonly SignedEightEight MinimumSeekSpeed = -MaximumSeekSpeed;
}
