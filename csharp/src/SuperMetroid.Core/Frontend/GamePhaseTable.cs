using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Exactly one phase for every game state no translated producer leaves unwritten.</summary>
internal sealed class GamePhaseTable
{
    /// <summary>The phase of each state.</summary>
    private readonly Dictionary<SuperMetroidGameState, IGamePhase> _phases = [];

    /// <summary>Indexes <paramref name="phases"/> by state.</summary>
    /// <param name="phases">The phase of every reachable state.</param>
    /// <param name="unreachable">The states no translated producer writes; reaching one means corrupt internal state.</param>
    /// <exception cref="InvalidOperationException">A state has two phases, is both unreachable and phased, or has neither.</exception>
    internal GamePhaseTable(IEnumerable<IGamePhase> phases, IEnumerable<SuperMetroidGameState> unreachable)
    {
        foreach (IGamePhase phase in Ensure.NotNull(phases))
        {
            if (!_phases.TryAdd(phase.State, phase))
                throw new InvalidOperationException($"Game state {phase.State} has two phases.");
        }
        HashSet<SuperMetroidGameState> unused = [.. Ensure.NotNull(unreachable)];
        SuperMetroidGameState[] phasedUnused = [.. unused.Where(_phases.ContainsKey)];
        if (phasedUnused.Length != 0)
            throw new InvalidOperationException($"Unreachable game states with a phase: {string.Join(", ", phasedUnused)}.");
        SuperMetroidGameState[] missing = [.. Enum.GetValues<SuperMetroidGameState>().Where(state => !_phases.ContainsKey(state) && !unused.Contains(state))];
        if (missing.Length != 0)
            throw new InvalidOperationException($"Game states without a phase: {string.Join(", ", missing)}.");
    }

    /// <summary>The phase of <paramref name="state"/>.</summary>
    internal IGamePhase Of(SuperMetroidGameState state) =>
        _phases.TryGetValue(state, out IGamePhase? phase)
            ? phase
            : throw new InvalidDataException($"Frontend dispatcher received invalid state {state}.");
}
