namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What the session keeps across the gameplay runtimes it creates, and hands each one at its start: its frame clock, what
/// the shared game state continues from, the compatibility services whose state outlives each game, and its sound and
/// music queues.
/// </summary>
internal sealed record GameplaySessionCarryover
{
    /// <summary>The session's frame clock, which counts across runtimes.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>What the runtime's shared game state continues from: the random number generator, the rumble delay and the explored maps.</summary>
    public required SharedGameStateCarryover GameState { get; init; }

    /// <summary>The compatibility services whose state outlives each game, which the runtime hands to the owners that consult them.</summary>
    public required CompatibilityKeptAcrossGames Compatibility { get; init; }

    /// <summary>
    /// The session's sound and music queues, which the runtime's gameplay requests go straight into as they are made; none
    /// for a runtime outside a session, whose requests reach no queue.
    /// </summary>
    public AudioQueues? Audio { get; init; }
}
