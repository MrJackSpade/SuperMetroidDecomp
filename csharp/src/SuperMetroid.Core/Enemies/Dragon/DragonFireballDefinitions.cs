namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Where a Dragon spits its fireball from and how the fireball flies (#2149).</summary>
internal static class DragonFireballDefinitions
{
    /// <summary>
    /// Where a fireball leaves a Dragon facing right: twelve pixels ahead of its body's middle and 28 above; one facing
    /// left spits it as far to its left.
    /// </summary>
    internal static Offset16<RoomPixels> MuzzleFacingRight { get; } = new(12, -28);

    /// <summary>The upward speed a fireball is spat at: three pixels and 193 256ths an update.</summary>
    internal static SignedEightEight LaunchRise { get; } = -SignedEightEight.FromParts(3, 193);

    /// <summary>The fireball's sideways speed toward the Dragon's facing: two and three quarter pixels an update.</summary>
    internal static SignedEightEight SidewaysSpeed { get; } = SignedEightEight.FromParts(2, 192);

    /// <summary>How much a fireball's fall speeds up each update: an eighth of a pixel an update.</summary>
    internal static SignedEightEight Gravity { get; } = SignedEightEight.FromParts(0, 32);
}
