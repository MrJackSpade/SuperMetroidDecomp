namespace SuperMetroid.Core.Game;

/// <summary>
/// Independent subsystem-disable words cleared by X-Ray setup at
/// <c>$91:E231-$91:E23D</c> and restored by <c>$91:E2AD</c>.
/// </summary>
/// <remarks>
/// The cartridge stores these as four separate composable words: enemy projectiles at
/// WRAM <c>$198D</c>, PLMs at <c>$1C23</c>, animated tiles at <c>$1EF1</c>, and palette FX
/// at <c>$1E79</c>. Automatic Reserve recovery clears only shared time-freeze word
/// <c>$0A78</c>, leaving this complete set suspended to create G-Mode.
/// </remarks>
[Flags]
public enum XraySuspendedSubsystems : byte
{
    /// <summary>No independent X-ray subsystem disable is retained; this does not establish whether the separate shared time-freeze word is clear.</summary>
    None = 0,
    /// <summary>Host mask for the $198D enemy-projectile enable word cleared by $91:E231; suppresses enemy-projectile updates and drawing independently of shared time freeze.</summary>
    EnemyProjectiles = 1 << 0,
    /// <summary>Host mask for the $1C23 PLM enable word cleared by $91:E235; suspends room-object instruction processing, including door-transition PLMs.</summary>
    Plms = 1 << 1,
    /// <summary>Host mask for the $1EF1 animated-tile enable word cleared by $91:E239; prevents animated-tile objects from advancing.</summary>
    AnimatedTiles = 1 << 2,
    /// <summary>Host mask for the $1E79 palette-FX enable word cleared by $91:E23D; suspends room palette effects, not Samus's independently handled visor colors.</summary>
    PaletteFx = 1 << 3,
    /// <summary>All four independent disables installed by admitted X-ray setup and cleared by normal teardown; excludes shared time freeze, HDMA, and Samus control ownership.</summary>
    All = EnemyProjectiles | Plms | AnimatedTiles | PaletteFx,
}
