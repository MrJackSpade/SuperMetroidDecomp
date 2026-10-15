namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One general bank-$86 command of a typed projectile program whose family adds no behavior of its own; the projectile
/// engine carries it out for every such family.
/// </summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine a <see cref="EnemyProjectileCommonCueKind.SetPreInstruction"/> cue installs.</param>
/// <param name="Scatter">The random repositioning a <see cref="EnemyProjectileCommonCueKind.Scatter"/> cue performs.</param>
internal readonly record struct EnemyProjectileCommonCue(
    EnemyProjectileCommonCueKind Kind,
    EnemyProjectilePreInstruction PreInstruction = default,
    EnemyProjectileScatter Scatter = default);
