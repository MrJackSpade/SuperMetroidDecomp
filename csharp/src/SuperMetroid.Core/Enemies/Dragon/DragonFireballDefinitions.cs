namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Where a Dragon spits its fireball from and how the fireball flies (#2149).</summary>
internal static partial class DragonFireballDefinitions
{
    /// <summary>
    /// Where a fireball leaves a Dragon facing right: twelve pixels ahead of its body's middle and 28 above; one facing
    /// left spits it as far to its left.
    /// </summary>
    internal static Offset16<RoomPixels> MuzzleFacingRight { get; } = new(12, -28);

    /// <summary>The upward speed a fireball is spat at: three and three quarter pixels and a 256th an update.</summary>
    internal static SignedEightEight LaunchRise { get; } = -SignedEightEight.FromPixelFraction(961, 256);

    /// <summary>The fireball's sideways speed toward the Dragon's facing: two and three quarter pixels an update.</summary>
    internal static SignedEightEight SidewaysSpeed { get; } = SignedEightEight.FromPixelFraction(11, 4);

    /// <summary>How much a fireball's fall speeds up each update: an eighth of a pixel an update.</summary>
    internal static SignedEightEight Gravity { get; } = SignedEightEight.FromPixelFraction(1, 8);
}
