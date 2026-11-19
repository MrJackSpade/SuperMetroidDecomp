namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The reward's gesture (#2541): the suitless or helmet-removal actors, until they ask for the jump.</summary>
/// <param name="gesture">The gesture's actors.</param>
internal sealed class EndingRewardGestureScene(EndingRewardGesture gesture) : EndingScene
{
    /// <summary>The gesture's actors.</summary>
    internal EndingRewardGesture Gesture { get; } = Ensure.NotNull(gesture);
}
