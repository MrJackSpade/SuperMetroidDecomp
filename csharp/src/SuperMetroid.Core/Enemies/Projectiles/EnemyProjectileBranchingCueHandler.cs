using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Carries out one cue a typed projectile program published on <paramref name="projectile"/>, returning the program
/// it continues at, as a native conditional goto does, or null to run on.
/// </summary>
internal delegate TProgram? EnemyProjectileBranchingCueHandler<TProgram, in TCue>(RoomEnemyProjectileSlot projectile, SamusState? samus, TCue cue)
    where TProgram : struct, Enum;
