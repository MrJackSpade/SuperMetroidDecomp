using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The brief white flash into the final logo (#2541): a white wash over the post-credits text and the landed Samus, full
/// white as it begins and a step fewer each update, fading to none, where it stays.
/// </summary>
/// <param name="jump">The landed jump's actors, which keep running.</param>
/// <param name="flash">The flash's white wash.</param>
internal sealed class EndingWhiteFlashScene(EndingRewardJump jump, WhiteWash flash) : EndingScene
{
    /// <summary>The landed jump's actors.</summary>
    internal EndingRewardJump Jump { get; } = Ensure.NotNull(jump);

    /// <summary>The flash's white wash.</summary>
    internal WhiteWash Flash { get; } = Ensure.NotNull(flash);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepWhiteFlash(this);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows)
    {
        if (shows(EndingSpriteLayer.Actors))
            EndingCreditsPresentation.DrawJump(ending, sprites, Jump);
    }
    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureSharedScreen(ending, paint);
}
