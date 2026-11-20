using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The opening flight toward Ceres: its music, colour effects, floor views, caption timing and fade.</summary>
internal static class CeresFlightSceneDefinitions
{
    /// <summary>The front approach's starting horizontal floor scroll, in pixels.</summary>
    internal const int FrontInitialFloorScrollX = -72;
    /// <summary>The front approach's starting vertical floor scroll, in pixels.</summary>
    internal const int FrontInitialFloorScrollY = -104;
    /// <summary>The front approach's starting floor angle.</summary>
    internal static RotationAngle FrontInitialAngle => RotationAngle.FromSteps(224);

    /// <summary>The rear view's horizontal floor scroll, in pixels.</summary>
    internal const int RearViewFloorScrollX = -32;
    /// <summary>The rear view's vertical floor scroll, in pixels.</summary>
    internal const int RearViewFloorScrollY = -128;
    /// <summary>The rear view's floor angle.</summary>
    internal static RotationAngle RearViewAngle => RotationAngle.FromSteps(32);

    /// <summary>The updates between the SPACE COLONY caption's letters.</summary>
    internal const int CaptionLetterDelay = 16;
    /// <summary>The update the Japanese caption shows before the completed caption's hold.</summary>
    internal const int JapaneseCaptionDelay = 1;
    /// <summary>The completed caption's hold before the fade out.</summary>
    internal const int CaptionHold = 128;

    /// <summary>The song that starts with the flight.</summary>
    internal const MusicTrack FlightSong = MusicTrack.CeresFlight;
    /// <summary>How long the flight's track waits on the music queue: fourteen passes.</summary>
    internal static MusicQueueDelay MusicDelay { get; } = new(14);

    /// <summary>Updates each step of the flight's closing fade-out takes: one level every second update.</summary>
    internal const int FadeOutUpdatesPerStep = 2;

    /// <summary>
    /// The gunship engine's flickering colours on the rear view's floor, started as the rear view is set up, just before the
    /// station's navigation lights (#1733).
    /// </summary>
    internal const ColourEffectId GunshipEngineFlicker = CeresDestructionSceneDefinitions.GunshipEngineFlicker;

    /// <summary>The engine flicker whose current frame the rear view's floor shows (#1733).</summary>
    internal const ColourEffectColours EngineColours = ColourEffectColours.CutsceneGunshipEngine;

    /// <summary>The station's blinking navigation lights on its sprites, started as the rear view is set up (#1733).</summary>
    internal const ColourEffectId NavigationLights = ColourEffectId.CutsceneCeresNavigationLightsSprite;

    /// <summary>
    /// The colour effect each actor colouring animates (#1733): an actor drawn in the colouring shows the frame the
    /// effect last showed over its drawn colours.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSpriteColouring, ColourEffectColours> LitColours { get; } =
        new Dictionary<CeresSpriteColouring, ColourEffectColours>
        {
            [CeresSpriteColouring.Station] = ColourEffectColours.CeresNavigationLightsSprite,
        };


    /// <summary>The updates each step off the rear view's white flash takes: one, a step every update.</summary>
    internal const int RearViewFlashUpdatesPerStep = 1;

    /// <summary>The update of the rear view's white flash its first step off white comes on: the first.</summary>
    internal const int RearViewFlashFirstStepUpdate = 1;
}
