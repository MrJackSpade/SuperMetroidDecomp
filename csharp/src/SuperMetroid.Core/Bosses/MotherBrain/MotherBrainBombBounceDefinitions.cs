namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Mother Brain bomb's Y acceleration while it falls and bounces, and the sideways speed each bounce restores. Before its first bounce it falls at the ordinary
/// gravity of 7; each later bounce's gravity grows by sixteen times the bounce number, capped at 64 per bounce. The bomb
/// expires once it lands from its last bounce.
/// </summary>
internal static class MotherBrainBombBounceDefinitions
{
    /// <summary>The ordinary fall gravity, used before the first bounce.</summary>
    internal static readonly SignedEightEight FallAcceleration = SignedEightEight.FromPixelFraction(7, 256);

    /// <summary>The bomb's sideways speed as it leaves her mouth: seven eighths of a pixel an update, rightward.</summary>
    internal static readonly SignedEightEight LaunchSpeedX = SignedEightEight.FromPixelFraction(7, 8);

    /// <summary>The bomb's vertical speed as it leaves her mouth: a pixel an update, downward.</summary>
    internal static readonly SignedEightEight LaunchSpeedY = SignedEightEight.FromPixels(1);

    /// <summary>The upward speed each floor bounce restarts the bomb's fall at: two pixels an update.</summary>
    internal static readonly SignedEightEight BounceRise = SignedEightEight.FromPixels(-2);

    /// <summary>The X at or right of which the bomb bounces off the arena's right wall.</summary>
    internal const int RightWallX = 240;

    /// <summary>The height of the floor the bomb bounces on.</summary>
    internal const int FloorY = 208;

    /// <summary>The drag taken from the bomb's sideways speed on every update of its first fall, stopping at rest.</summary>
    internal static readonly SignedEightEight FallDrag = SignedEightEight.FromPixelFraction(1, 128);

    /// <summary>The sideways speed a Mother Brain bomb's floor bounce restores, without its direction.</summary>
    internal static readonly SignedEightEight BounceSpeed = SignedEightEight.FromPixelFraction(7, 16);

    /// <summary>The gravity after each bounce, from the first to the eighth and last, in 256ths of a pixel an update.</summary>
    internal static readonly SignedEightEight[] BounceAccelerations =
    [
        SignedEightEight.FromPixelFraction(1, 16),
        SignedEightEight.FromPixelFraction(1, 8),
        SignedEightEight.FromPixelFraction(1, 4),
        SignedEightEight.FromPixelFraction(7, 16),
        SignedEightEight.FromPixelFraction(11, 16),
        SignedEightEight.FromPixelFraction(15, 16),
        SignedEightEight.FromPixelFraction(19, 16),
        SignedEightEight.FromPixelFraction(23, 16),
    ];
}
