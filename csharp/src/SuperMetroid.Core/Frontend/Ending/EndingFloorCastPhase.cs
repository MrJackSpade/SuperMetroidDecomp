namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A phase the ending's cast shows whose turning floor shows a shot (#2558).</summary>
internal sealed class EndingFloorCastPhase : EndingCastPhase, IEndingFloorPhase
{
    /// <summary>A cast phase named <paramref name="name"/> whose floor shows <paramref name="floorShot"/>, stepped by <paramref name="step"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="floorShot">The shot the turning floor shows in the phase.</param>
    /// <param name="step">Runs one update of the phase.</param>
    internal EndingFloorCastPhase(string name, EndingFloorShot floorShot, Func<EndingCreditsState, bool> step)
        : base(name, step) => FloorShot = floorShot;

    /// <inheritdoc/>
    public EndingFloorShot FloorShot { get; }
}
