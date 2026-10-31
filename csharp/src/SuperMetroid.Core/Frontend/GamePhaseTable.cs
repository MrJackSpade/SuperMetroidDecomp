using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Exactly one phase for every game state.</summary>
internal sealed class GamePhaseTable
{
    /// <summary>The phase of each state.</summary>
    private readonly Dictionary<SuperMetroidGameState, IGamePhase> _phases = [];

    /// <summary>Indexes <paramref name="phases"/> by state.</summary>
    /// <param name="phases">The phase of every state.</param>
    /// <exception cref="InvalidOperationException">A state has two phases, or has none.</exception>
    internal GamePhaseTable(IEnumerable<IGamePhase> phases)
    {
        foreach (IGamePhase phase in Ensure.NotNull(phases))
        {
            if (!_phases.TryAdd(phase.State, phase))
                throw new InvalidOperationException($"Game state {phase.State} has two phases.");
        }
        SuperMetroidGameState[] missing = [.. Enum.GetValues<SuperMetroidGameState>().Where(state => !_phases.ContainsKey(state))];
        if (missing.Length != 0)
            throw new InvalidOperationException($"Game states without a phase: {string.Join(", ", missing)}.");
    }

    /// <summary>The phase of <paramref name="state"/>.</summary>
    internal IGamePhase Of(SuperMetroidGameState state) =>
        _phases.TryGetValue(state, out IGamePhase? phase)
            ? phase
            : throw new InvalidDataException($"Frontend dispatcher received invalid state {state}.");
}
