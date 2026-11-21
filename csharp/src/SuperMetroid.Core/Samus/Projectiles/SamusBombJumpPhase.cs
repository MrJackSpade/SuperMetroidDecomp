using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The part of an armed bomb jump that holds Samus's movement (<see cref="SamusMovementOwner.BombJump"/>): its one-update
/// start, which sets her velocity, or its rise along the arc (#2553). Each phase runs its own movement.
/// </summary>
public abstract class SamusBombJumpPhase
{
    /// <summary>Only the two phases derive.</summary>
    private protected SamusBombJumpPhase()
    {
    }

    /// <summary>The bomb jump's one-update start, which sets her velocity and moves her nowhere.</summary>
    public static SamusBombJumpPhase Starting { get; } = new StartingPhase();

    /// <summary>The bomb jump moving her along its rising arc.</summary>
    public static SamusBombJumpPhase Rising { get; } = new RisingPhase();

    /// <summary>Runs one update of the phase's movement.</summary>
    /// <param name="terrain">The terrain she moves through.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="frameCounter">The update's frame counter.</param>
    internal abstract BombJumpMovementResult Run(SamusTerrain terrain, SamusState samus, int frameCounter);

    /// <summary>The start.</summary>
    private sealed class StartingPhase : SamusBombJumpPhase
    {
        /// <inheritdoc/>
        internal override BombJumpMovementResult Run(SamusTerrain terrain, SamusState samus, int frameCounter) =>
            SamusBombJumpMovement.Start(samus);
    }

    /// <summary>The rise.</summary>
    private sealed class RisingPhase : SamusBombJumpPhase
    {
        /// <inheritdoc/>
        internal override BombJumpMovementResult Run(SamusTerrain terrain, SamusState samus, int frameCounter) =>
            SamusBombJumpMovement.Step(terrain, samus, frameCounter);
    }
}
