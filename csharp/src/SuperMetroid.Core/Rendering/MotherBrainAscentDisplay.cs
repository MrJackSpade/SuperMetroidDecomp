using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What each band of the gameplay screen shows while Mother Brain rises through the floor (#1514): her body, which the
/// room's background shows, appears only between the room's upper band and its floor, and the floor's band hides the
/// actors, so she rises from behind the floor.
/// </summary>
internal static class MotherBrainAscentDisplay
{
    /// <summary>
    /// The three bands below the HUD: the upper band shows the foreground and the actors without her body, the middle band
    /// shows the foreground, her body and the actors, and the floor's band shows the foreground alone.
    /// </summary>
    internal static ImmutableArray<ShownBand> Bands { get; } =
    [
        new(new ScreenLineBand(ScreenGeometry.GameplayHudHeightPixels, MotherBrainAscentDisplayDefinitions.UpperBandEnd),
            ScreenLayers.Foreground | ScreenLayers.Sprites),
        new(new ScreenLineBand(MotherBrainAscentDisplayDefinitions.UpperBandEnd, MotherBrainAscentDisplayDefinitions.FloorBandStart),
            ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites),
        new(new ScreenLineBand(MotherBrainAscentDisplayDefinitions.FloorBandStart, ScreenGeometry.ScreenHeightPixels),
            ScreenLayers.Foreground),
    ];
}
