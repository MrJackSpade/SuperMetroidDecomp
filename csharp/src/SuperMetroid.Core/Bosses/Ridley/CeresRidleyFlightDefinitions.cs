namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Ceres Ridley flies into his fireball position and swoops (#2171).</summary>
internal static class CeresRidleyFlightDefinitions
{
    /// <summary>The least vertical speed Ridley keeps, either way, while he moves into his fireball position: half a pixel an update.</summary>
    internal static readonly SignedEightEight FireballMoveMinimumSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>How far the swoop speed moves toward its target each update: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight SwoopSpeedStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);
}
