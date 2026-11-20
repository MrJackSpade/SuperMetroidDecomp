using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The reward's jump (#2541): Samus's jump, fall and landing, until her landing asks for the shot.</summary>
/// <param name="jump">The jump's actors.</param>
internal sealed class EndingRewardJumpScene(EndingRewardJump jump) : EndingScene
{
    /// <summary>The jump's actors.</summary>
    internal EndingRewardJump Jump { get; } = Ensure.NotNull(jump);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepJump(this);
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
