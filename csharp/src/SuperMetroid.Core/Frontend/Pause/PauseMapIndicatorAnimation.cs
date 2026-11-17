using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pulse the pause and file-select map markers share (#1825, #2359): the marker grows from its smallest picture through
/// its middle one to its largest and shrinks back through the middle one, then starts again. The smallest and largest
/// pictures hold for eight menu updates, the middle one for four.
/// </summary>
internal static class PauseMapIndicatorAnimation
{
    /// <summary>The pulse's one segment: each picture for its hold, then the moment the pulse completes.</summary>
    internal static AnimationSegments<MapIndicatorSegment, MapSpriteId, MapIndicatorCue> Segments { get; } = new(
        new Dictionary<MapIndicatorSegment, AnimationBeat<MapSpriteId, MapIndicatorCue>[]>
        {
            [MapIndicatorSegment.Pulse] =
            [
                Show(MapSpriteId.IndicatorSmall, 8),
                Show(MapSpriteId.IndicatorMiddle, 4),
                Show(MapSpriteId.IndicatorLarge, 8),
                Show(MapSpriteId.IndicatorMiddle, 4),
                new AnimationBeat<MapSpriteId, MapIndicatorCue>.Event(MapIndicatorCue.PulseCompleted),
            ],
        });

    /// <summary>The pulse played over and over.</summary>
    internal static Animation<MapIndicatorSegment> Pulsing { get; } = new([Segments.Whole(MapIndicatorSegment.Pulse)], AnimationEnd.LoopsFrom(0));

    /// <summary>Shows <paramref name="picture"/> for <paramref name="duration"/> menu updates.</summary>
    private static AnimationBeat<MapSpriteId, MapIndicatorCue> Show(MapSpriteId picture, int duration) =>
        new AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame(picture, duration);
}
