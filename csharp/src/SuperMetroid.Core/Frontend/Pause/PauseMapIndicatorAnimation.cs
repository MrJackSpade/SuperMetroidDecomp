using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The pulse the pause and file-select map markers share (#1825).</summary>
internal static class PauseMapIndicatorAnimation
{
    /// <summary>
    /// The pulse's steps in order: the marker grows from its smallest picture through its middle one to its largest and
    /// shrinks back through the middle one, then starts again. The smallest and largest pictures hold for eight menu
    /// updates, the middle one for four.
    /// </summary>
    internal static IReadOnlyList<MapIndicatorPulseStep> Pulse { get; } =
    [
        new(MapSpriteId.IndicatorSmall, 8),
        new(MapSpriteId.IndicatorMiddle, 4),
        new(MapSpriteId.IndicatorLarge, 8),
        new(MapSpriteId.IndicatorMiddle, 4),
    ];
}
