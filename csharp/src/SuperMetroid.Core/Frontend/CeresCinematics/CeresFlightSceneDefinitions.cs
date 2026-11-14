using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The opening flight toward Ceres: its music and its colour effects.</summary>
internal static class CeresFlightSceneDefinitions
{
    /// <summary>The song that starts with the flight.</summary>
    internal const MusicTrack FlightSong = MusicTrack.CeresFlight;
    /// <summary>The queue delay the flight's track is requested with.</summary>
    internal const int MusicDelay = 14;

    /// <summary>
    /// The gunship engine's flickering colours on the rear view's floor, started as the rear view is set up, just before the
    /// station's navigation lights (#1733).
    /// </summary>
    internal const ColourEffectId GunshipEngineFlicker = CeresDestructionSceneDefinitions.GunshipEngineFlicker;

    /// <summary>The colour group the rear view's floor shows the engine flicker's records of (#1733).</summary>
    internal const ColourEffectColours EngineColours = ColourEffectColours.CutsceneGunshipEngine;

    /// <summary>The station's blinking navigation lights on its sprites, started as the rear view is set up (#1733).</summary>
    internal const ColourEffectId NavigationLights = ColourEffectId.CutsceneCeresNavigationLightsSprite;

    /// <summary>
    /// The colour group of each actor colouring a colour effect animates (#1733): an actor drawn in the colouring shows the
    /// record the effect last showed over its drawn colours.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSpriteColouring, ColourEffectColours> LitColours { get; } =
        new Dictionary<CeresSpriteColouring, ColourEffectColours>
        {
            [CeresSpriteColouring.Station] = ColourEffectColours.CeresNavigationLightsSprite,
        };
}
