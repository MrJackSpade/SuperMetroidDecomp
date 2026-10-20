namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One general bank-$86 command of a typed projectile program whose family adds no behavior of its own; the projectile
/// engine carries it out for every such family.
/// </summary>
internal abstract record EnemyProjectileCommonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyProjectileCommonCue()
    {
    }

    /// <summary>Scatters the projectile randomly about its position (<c>Instruction_EnemyProjectile_MoveRandomlyWithinXRadius_YRadius</c>, $86:81DF).</summary>
    /// <param name="Spread">The random repositioning a <see cref="Scatter"/> cue performs.</param>
    internal sealed record Scatter(EnemyProjectileScatter Spread) : EnemyProjectileCommonCue;

    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    /// <param name="PreInstruction">The movement routine a <see cref="SetPreInstruction"/> cue installs.</param>
    internal sealed record SetPreInstruction(EnemyProjectilePreInstruction PreInstruction) : EnemyProjectileCommonCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(EnemyProjectileCommonCueKind Kind) : EnemyProjectileCommonCue;
}
