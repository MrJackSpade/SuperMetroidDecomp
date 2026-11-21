namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a Spazer and Plasma beam's shot runs when it runs data rather than the game's code only once its update runs
/// (#1923, <see cref="SamusBeamShotBehaviorDefinitions"/>): the shot is fired as the game fires it, and refused when its
/// update would run it, since reproducing that needs full memory and processor emulation, which is outside the emulation
/// scope limit (#1500). A shot with no animation, such as the safe left-facing Murder Beam, is never updated and never
/// runs it.
/// </summary>
/// <param name="runs">What the shot runs, completing "The shot runs ...".</param>
internal sealed partial class ShotRefusedWhenRun(string runs) : SamusShotBehavior
{
    /// <summary>What the shot runs, completing "The shot runs ...".</summary>
    internal string Runs { get; } = Ensure.NotNull(runs);

    /// <summary>Refuses the shot as its update would run it (#1500).</summary>
    /// <exception cref="NotSupportedException">Always.</exception>
    internal override void Run(SamusShotUpdate update) =>
        throw new NotSupportedException(
            $"The shot in place {update.Shot.Place} runs {Runs}; reproducing that needs full memory and processor " +
            "emulation, which is outside the emulation scope limit (#1500).");
}
