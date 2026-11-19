namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The reward's reveal (#2541): the idle reward in the cast and the colour fade toward its colours, which its two halves
/// share, the copyright's hold between them pausing it.
/// </summary>
/// <param name="fade">The reward's colour fade, started as the reveal starts.</param>
internal sealed class EndingRewardRevealScene(PacedFadeSteps fade) : EndingScene
{
    /// <summary>The reward's colour fade.</summary>
    internal PacedFadeSteps Fade { get; } = Ensure.NotNull(fade);
}
