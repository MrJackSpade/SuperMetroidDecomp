using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// A map marker's pulse (<see cref="PauseMapIndicatorAnimation"/>, #2359), played by the shared animation player. The pulse
/// starts on its smallest picture, one update from its next, so the marker's first update shows its middle picture.
/// </summary>
internal sealed class MapIndicatorPulse
{
    /// <summary>The pulse as it plays.</summary>
    private readonly ShowingAnimationPlayer<MapIndicatorSegment, MapSpriteId, MapIndicatorCue> player =
        ShowingAnimationPlayer<MapIndicatorSegment, MapSpriteId, MapIndicatorCue>.StartShowing(
            PauseMapIndicatorAnimation.Segments, PauseMapIndicatorAnimation.Pulsing, 1);

    /// <summary>The marker's picture.</summary>
    internal MapSpriteId Sprite => player.Showing;

    /// <summary>Runs one menu update of the pulse, raising each completed pulse in <paramref name="raise"/>.</summary>
    internal void Step(Action<MapIndicatorCue> raise) => player.Step(raise);
}
