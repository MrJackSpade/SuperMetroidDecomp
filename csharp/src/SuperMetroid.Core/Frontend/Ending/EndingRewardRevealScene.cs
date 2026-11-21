using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

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

    /// <summary>Whether the copyright's hold between the reveal's two halves runs, pausing the fade.</summary>
    internal bool CopyrightHolds { get; private set; }

    /// <summary>The first half has ended: the copyright's hold starts.</summary>
    internal void HoldCopyright() => CopyrightHolds = true;

    /// <summary>The copyright's hold has ended: the reveal's second half starts.</summary>
    internal void ResumeReveal() => CopyrightHolds = false;

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase => CopyrightHolds ? EndingCreditsPhase.PostCreditsCopyright : EndingCreditsPhase.PostCreditsReward;

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
