namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Independent subsystem disables cleared by X-Ray setup and restored
/// by its teardown.
/// </summary>
/// <remarks>
/// The game stores these as four separate composable flags: enemy projectiles,
/// room objects, block animations, and colour effects.
/// Automatic Reserve recovery clears only the shared time freeze,
/// leaving this complete set suspended to create G-Mode.
/// </remarks>
[Flags]
public enum XraySuspendedSubsystems
{
    /// <summary>No independent X-ray subsystem disable is retained; this does not establish whether the separate shared time-freeze flag is clear.</summary>
    None = 0,
    /// <summary>The enemy projectiles, which X-ray setup disables; suppresses enemy-projectile updates and drawing independently of shared time freeze.</summary>
    EnemyProjectiles = 1 << 0,
    /// <summary>The room objects, which X-ray setup disables; suspends room-object processing, including door-transition room objects.</summary>
    RoomObjects = 1 << 1,
    /// <summary>The block animations, which X-ray setup disables; prevents them from advancing.</summary>
    BlockAnimations = 1 << 2,
    /// <summary>The room palette effects, which X-ray setup disables; not Samus's independently handled visor colors.</summary>
    ColourEffects = 1 << 3,
    /// <summary>All four independent disables installed by admitted X-ray setup and cleared by normal teardown; excludes shared time freeze, the cone effect, and Samus control ownership.</summary>
    All = EnemyProjectiles | RoomObjects | BlockAnimations | ColourEffects,
}
