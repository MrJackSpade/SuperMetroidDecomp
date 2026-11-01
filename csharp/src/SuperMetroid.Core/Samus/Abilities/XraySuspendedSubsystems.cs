namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Independent subsystem-disable words cleared by X-Ray setup and restored
/// by its teardown.
/// </summary>
/// <remarks>
/// The game stores these as four separate composable words: enemy projectiles,
/// PLMs, animated tiles, and palette FX.
/// Automatic Reserve recovery clears only the shared time-freeze word,
/// leaving this complete set suspended to create G-Mode.
/// </remarks>
[Flags]
public enum XraySuspendedSubsystems
{
    /// <summary>No independent X-ray subsystem disable is retained; this does not establish whether the separate shared time-freeze word is clear.</summary>
    None = 0,
    /// <summary>Host mask for the enemy-projectile enable word cleared by X-ray setup; suppresses enemy-projectile updates and drawing independently of shared time freeze.</summary>
    EnemyProjectiles = 1 << 0,
    /// <summary>Host mask for the PLM enable word cleared by X-ray setup; suspends room-object processing, including door-transition PLMs.</summary>
    Plms = 1 << 1,
    /// <summary>Host mask for the animated-tile enable word cleared by X-ray setup; prevents animated-tile objects from advancing.</summary>
    AnimatedTiles = 1 << 2,
    /// <summary>Host mask for the palette-FX enable word cleared by X-ray setup; suspends room palette effects, not Samus's independently handled visor colors.</summary>
    PaletteFx = 1 << 3,
    /// <summary>All four independent disables installed by admitted X-ray setup and cleared by normal teardown; excludes shared time freeze, the cone effect, and Samus control ownership.</summary>
    All = EnemyProjectiles | Plms | AnimatedTiles | PaletteFx,
}
