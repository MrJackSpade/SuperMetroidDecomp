using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The reward's reveal (#2541): the idle reward in the cast and the colour fade toward its colours, which its two halves
/// share, the copyright's hold between them pausing it.
/// </summary>
/// <param name="fade">The reward's colour fade, started as the reveal starts.</param>
/// <param name="phase">The phase the scene runs: one of the reveal's halves, or the copyright's hold between them.</param>
internal sealed partial class EndingRewardRevealScene(PacedFadeSteps fade, EndingCreditsPhase phase) : EndingScene
{
    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase { get; } =
        phase == EndingCreditsPhase.PostCreditsReward || phase == EndingCreditsPhase.PostCreditsCopyright ? phase
            : throw new ArgumentException($"The reward's reveal runs no {phase} phase.", nameof(phase));

    /// <summary>The reward's colour fade.</summary>
    internal PacedFadeSteps Fade { get; } = Ensure.NotNull(fade);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepRewardReveal(this);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows) =>
        EndingCreditsPresentation.DrawCast(ending, sprites, shows);
    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureSharedScreen(ending, paint);
}
