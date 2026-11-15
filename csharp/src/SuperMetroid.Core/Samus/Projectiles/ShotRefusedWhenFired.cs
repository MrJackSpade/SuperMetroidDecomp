namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a Spazer and Plasma beam's shot runs when every shot of it would run data rather than the game's code (#1923,
/// <see cref="SamusBeamFrameBehaviorDefinitions"/>): reproducing it needs full memory and processor emulation, which is
/// outside the emulation scope limit (#1500), so the shot is refused as it is fired, after its muzzle collision.
/// </summary>
/// <param name="runs">What the shot runs, completing "The shot runs ...".</param>
internal sealed class ShotRefusedWhenFired(string runs) : SamusShotBehavior
{
    /// <summary>What the shot runs, completing "The shot runs ...".</summary>
    internal string Runs { get; } = Ensure.NotNull(runs);

    /// <summary>Refuses the shot (#1500).</summary>
    /// <exception cref="NotSupportedException">Always.</exception>
    internal override void AdmitFiring() =>
        throw new NotSupportedException(
            $"The shot runs {Runs}; reproducing that needs full memory and processor emulation, which is outside the " +
            "emulation scope limit (#1500).");

    /// <summary>Never runs: the shot is refused as it is fired, so a shot that runs it was never admitted.</summary>
    /// <exception cref="InvalidOperationException">Always.</exception>
    internal override void Run(SamusShotUpdate update) =>
        throw new InvalidOperationException(
            $"The shot in place {update.Shot.Place} runs {Runs}, which is refused as it is fired, so it was never admitted.");
}
