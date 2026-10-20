using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A statue projectile program cue.</summary>
internal abstract record TourianStatueProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TourianStatueProjectileCue()
    {
    }

    /// <summary>Moves the projectile down by its cue's offset ($86:B841).</summary>
    /// <param name="Pixels">How far a <see cref="MoveDown"/> cue moves the projectile.</param>
    internal sealed record MoveDown(int Pixels) : TourianStatueProjectileCue;

    /// <summary>Installs its cue's movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    /// <param name="Movement">The routine a <see cref="StartMoving"/> cue installs.</param>
    internal sealed record StartMoving(EnemyProjectilePreInstruction Movement) : TourianStatueProjectileCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(TourianStatueProjectileCueKind Kind) : TourianStatueProjectileCue;
}
