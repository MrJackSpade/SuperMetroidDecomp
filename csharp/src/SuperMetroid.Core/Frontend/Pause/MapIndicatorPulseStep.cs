using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>One step of the map markers' pulse: the picture shown and the menu updates it holds for.</summary>
/// <param name="Sprite">The marker's picture.</param>
/// <param name="Duration">The menu updates the picture holds for.</param>
internal readonly record struct MapIndicatorPulseStep(MapSpriteId Sprite, int Duration);
