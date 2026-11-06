namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>When a frame is drawn under one encounter artwork sequence, and what completes its artwork (#1513).</summary>
/// <param name="DrawnBeforeFirstStep">Whether it is drawn before the sequence reaches its first step.</param>
/// <param name="Steps">The steps at which it is drawn.</param>
/// <param name="CompletedBy">The source whose first run of pages completes its artwork, or none when each step's look is its own.</param>
internal sealed record FrameDrawnSteps(bool DrawnBeforeFirstStep, IReadOnlyList<EncounterArtworkStepRange> Steps,
    EncounterArtworkSource? CompletedBy)
{
    /// <summary>Whether the frame is drawn at <paramref name="step"/>.</summary>
    internal bool DrawnAt(int step) => Steps.Any(range => range.Contains(step));
}
