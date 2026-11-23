using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The reward's gesture (#2541): the suitless or helmet-removal actors, until they ask for the jump.</summary>
/// <param name="gesture">The gesture's actors.</param>
internal sealed partial class EndingRewardGestureScene(EndingRewardGesture gesture) : EndingActorScene
{
    /// <summary>The gesture's actors.</summary>
    internal EndingRewardGesture Gesture { get; } = Ensure.NotNull(gesture);

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase => EndingCreditsPhase.PostCreditsGesture;

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepGesture(this);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows)
    {
        if (shows(EndingSpriteLayer.Actors))
            Gesture.Draw(sprites, EndingCreditsPresentation.ActorArt(ending, EndingSpriteFamily.EndingReward));
    }
    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureSharedScreen(ending, paint);
}
