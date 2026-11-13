using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The opening flight toward Ceres: its music and its colour effects.</summary>
internal static class CeresFlightSceneDefinitions
{
    /// <summary>The song of the flight's music set that starts with the flight: its first own song.</summary>
    internal const MusicTrack FlightSong = MusicTrack.FirstSetSong;
    /// <summary>The queue delay the flight's track is requested with.</summary>
    internal const int MusicDelay = 14;

    /// <summary>
    /// The gunship engine's flickering colours on the rear view's floor, started as the rear view is set up, just before the
    /// station's navigation lights (#1733).
    /// </summary>
    internal const PaletteFxObject GunshipEngineFlicker = CeresDestructionSceneDefinitions.GunshipEngineFlicker;

    /// <summary>The colour group the rear view's floor shows the engine flicker's records of (#1733).</summary>
    internal const PaletteFxColours EngineColours = PaletteFxColours.CutsceneGunshipEngine;

    /// <summary>The station's blinking navigation lights on its sprites, started as the rear view is set up (#1733).</summary>
    internal const PaletteFxObject NavigationLights = PaletteFxObject.CutsceneCeresNavigationLightsSprite;

    /// <summary>
    /// The colour group of each actor colouring a colour effect animates (#1733): an actor drawn in the colouring shows the
    /// record the effect last showed over its drawn colours.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSpriteColouring, PaletteFxColours> LitColours { get; } =
        new Dictionary<CeresSpriteColouring, PaletteFxColours>
        {
            [CeresSpriteColouring.Station] = PaletteFxColours.CeresNavigationLightsSprite,
        };
}
