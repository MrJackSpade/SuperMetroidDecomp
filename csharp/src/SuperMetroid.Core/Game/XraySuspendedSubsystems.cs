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
    None = 0,
    EnemyProjectiles = 1 << 0,
    Plms = 1 << 1,
    AnimatedTiles = 1 << 2,
    PaletteFx = 1 << 3,
    All = EnemyProjectiles | Plms | AnimatedTiles | PaletteFx,
}
