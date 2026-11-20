using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The reward's shot (#2541): the turning shot picture and the colour fades, over the landed Samus, until the shot's hold
/// hands over to the white flash.
/// </summary>
/// <param name="jump">The landed jump's actors, which keep running.</param>
/// <param name="shot">The shot's picture, fades and hold.</param>
internal sealed class EndingPostShotScene(EndingRewardJump jump, EndingPostShot shot) : EndingScene
{
    /// <summary>The landed jump's actors.</summary>
    internal EndingRewardJump Jump { get; } = Ensure.NotNull(jump);

    /// <summary>The shot's picture, fades and hold.</summary>
    internal EndingPostShot Shot { get; } = Ensure.NotNull(shot);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepPostShot(this);
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
        EndingCreditsPresentation.CapturePostShot(ending, Shot, paint);
}
