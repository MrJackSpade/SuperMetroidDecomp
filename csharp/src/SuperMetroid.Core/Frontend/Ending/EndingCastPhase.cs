namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A phase the ending's cast shows (#2571): it steps itself on the ending and enters its successor.</summary>
internal sealed class EndingCastPhase : EndingCreditsPhase
{
    /// <summary>Runs one update of the phase.</summary>
    private readonly Func<EndingCreditsState, bool> _step;

    /// <summary>A cast phase named <paramref name="name"/> whose floor shows <paramref name="floorShot"/>, stepped by <paramref name="step"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="floorShot">The shot the turning floor shows in the phase.</param>
    /// <param name="step">Runs one update of the phase.</param>
    internal EndingCastPhase(string name, EndingFloorShot floorShot, Func<EndingCreditsState, bool> step)
        : base(name, floorShot) => _step = step;

    /// <summary>Runs one update of the phase on the ending.</summary>
    /// <param name="ending">The ending.</param>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool Step(EndingCreditsState ending) => _step(ending);
}
