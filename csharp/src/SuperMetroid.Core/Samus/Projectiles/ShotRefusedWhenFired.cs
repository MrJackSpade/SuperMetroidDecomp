namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a Spazer and Plasma beam's shot runs when every shot of it would run data rather than the game's code (#1923,
/// <see cref="SamusBeamShotBehaviorDefinitions"/>): reproducing it needs full memory and processor emulation, which is
/// outside the emulation scope limit (#1500), so the shot is refused as it is fired, after its muzzle collision.
/// </summary>
/// <param name="runs">What the shot runs, completing "The shot runs ...".</param>
internal sealed class ShotRefusedWhenFired(string runs) : SamusShotFiring
{
    /// <summary>What the shot runs, completing "The shot runs ...".</summary>
    internal string Runs { get; } = Ensure.NotNull(runs);

    /// <summary>Refuses the shot (#1500).</summary>
    /// <exception cref="NotSupportedException">Always.</exception>
    internal override void Fire(SamusProjectileOccupant held) =>
        throw new NotSupportedException(
            $"The shot runs {Runs}; reproducing that needs full memory and processor emulation, which is outside the " +
            "emulation scope limit (#1500).");
}
