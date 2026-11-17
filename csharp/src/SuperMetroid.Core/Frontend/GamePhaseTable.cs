using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Exactly one phase for every sequence of the game.</summary>
internal sealed class GamePhaseTable
{
    /// <summary>The phase of each sequence.</summary>
    private readonly Dictionary<SuperMetroidGameState, IGamePhase> _phases = [];

    /// <summary>Indexes <paramref name="phases"/> by sequence.</summary>
    /// <param name="phases">The phase of every sequence.</param>
    /// <exception cref="InvalidOperationException">A sequence has two phases, or has none.</exception>
    internal GamePhaseTable(IEnumerable<IGamePhase> phases)
    {
        foreach (IGamePhase phase in Ensure.NotNull(phases))
        {
            if (!_phases.TryAdd(phase.State, phase))
                throw new InvalidOperationException($"Sequence {phase.State} has two phases.");
        }
        SuperMetroidGameState[] missing = [.. Enum.GetValues<SuperMetroidGameState>().Where(state => !_phases.ContainsKey(state))];
        if (missing.Length != 0)
            throw new InvalidOperationException($"Sequences without a phase: {string.Join(", ", missing)}.");
    }

    /// <summary>The phase of <paramref name="state"/>.</summary>
    internal IGamePhase Of(SuperMetroidGameState state) =>
        _phases.TryGetValue(state, out IGamePhase? phase)
            ? phase
            : throw new InvalidDataException($"Frontend game loop received undefined sequence {state}.");
}
