namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A phase the ending's cast shows (#2571): it steps itself on the ending and enters its successor.</summary>
internal partial class EndingCastPhase : EndingCreditsPhase
{
    /// <summary>Runs one update of the phase.</summary>
    private readonly Func<EndingCreditsState, bool> _step;

    /// <summary>A cast phase named <paramref name="name"/>, stepped by <paramref name="step"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="step">Runs one update of the phase.</param>
    internal EndingCastPhase(string name, Func<EndingCreditsState, bool> step)
        : base(name) => _step = step;

    /// <summary>Runs one update of the phase on the ending.</summary>
    /// <param name="ending">The ending.</param>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool Step(EndingCreditsState ending) => _step(ending);
}
