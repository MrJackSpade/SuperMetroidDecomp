using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres self-destruct stands in one stage of readying its warning (#1406, #1538): the stage, and the updates it
/// has spent on it so far.
/// </summary>
/// <param name="Stage">The stage; none for readying that was never started.</param>
/// <param name="Elapsed">The updates spent on the stage so far.</param>
internal readonly record struct CeresEscapeReadying(CeresEscapeReadyingStage Stage, int Elapsed)
{
    /// <summary>Bringing up the escape timer and warning, from its first update.</summary>
    internal static CeresEscapeReadying Warning { get; } = new(CeresEscapeReadyingStage.Warning, 0);

    /// <summary>Readying the arena's backgrounds, from its first update.</summary>
    internal static CeresEscapeReadying Backgrounds { get; } = new(CeresEscapeReadyingStage.Backgrounds, 0);

    /// <summary>Whether a stage was started.</summary>
    internal bool IsStarted => Stage != CeresEscapeReadyingStage.None;

    /// <summary>Whether the stage has spent all its updates.</summary>
    internal bool IsFinished => IsStarted && Elapsed >= Duration;

    /// <summary>The updates the stage takes.</summary>
    /// <exception cref="InvalidOperationException">No stage was started.</exception>
    private int Duration => Stage switch
    {
        CeresEscapeReadyingStage.Warning => CeresEscapeReadyingDefinitions.WarningReadyingDuration,
        CeresEscapeReadyingStage.Backgrounds => CeresEscapeReadyingDefinitions.BackgroundReadyingDuration,
        CeresEscapeReadyingStage.None => throw new InvalidOperationException("Ceres escape readying that was never started has no duration."),
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresEscapeReadyingStage)} {Stage}."),
    };

    /// <summary>The readying after one more update of its stage.</summary>
    internal CeresEscapeReadying Advance() => this with { Elapsed = Elapsed + 1 };
}
