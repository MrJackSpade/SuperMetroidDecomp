using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What each band of the gameplay screen shows while Mother Brain rises through the floor (#1514, #2573): her body, its own
/// layer behind the room's foreground, shows only between the room's ceiling band and its floor, and the floor's band hides
/// the actors, her legs among them, so she rises from behind the floor.
/// </summary>
/// <remarks>
/// The floor's band stays a band of its own rather than following from depth alone: her legs are actors, drawn in front of
/// the floor's tiles, and the floor hides them only by its band; drawing them by depth would show her legs over the floor,
/// which the game never shows.
/// </remarks>
internal static partial class MotherBrainAscentDisplay
{
    /// <summary>
    /// The three bands below the HUD: the ceiling's band shows the room and the actors without her body, the middle band
    /// shows the room, her body and the actors, and the floor's band shows the room alone.
    /// </summary>
    internal static ImmutableArray<ShownBand> Bands { get; } =
    [
        new(new ScreenLineBand(ScreenGeometry.GameplayHudHeightPixels, MotherBrainAscentDisplayDefinitions.UpperBandEnd),
            ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites),
        new(new ScreenLineBand(MotherBrainAscentDisplayDefinitions.UpperBandEnd, MotherBrainAscentDisplayDefinitions.FloorBandStart),
            ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.BossBody | ScreenLayers.Sprites),
        new(new ScreenLineBand(MotherBrainAscentDisplayDefinitions.FloorBandStart, ScreenGeometry.ScreenHeightPixels),
            ScreenLayers.Foreground | ScreenLayers.Background),
    ];
}
