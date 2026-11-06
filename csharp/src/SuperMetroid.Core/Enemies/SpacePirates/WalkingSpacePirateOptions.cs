namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Independently tested options of a walking Pirate, which its population entry names. The values are arbitrary bits.
/// </summary>
[Flags]
public enum WalkingSpacePirateOptions
{
    /// <summary>It starts facing left, with the fast laser and no projectile flinch.</summary>
    None = 0,
    /// <summary>Selects the right-facing initial animation; clear selects left, independently of the other option.</summary>
    StartsFacingRight = 1,
    /// <summary>Enables the projectile-flinch check after normal AI, and its lasers move two rather than four horizontal pixels per projectile update.</summary>
    SlowLaserAndProjectileFlinch = 2,
}
