using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening cinematic's shared definitions: its text layout, crossfade timing, music, actors' sounds, and the narration caret.</summary>
public static class IntroCinematicDefinitions
{
    /// <summary>The planes the opening's scene and text are drawn on.</summary>
    public static class Planes
    {
        /// <summary>Width of an opening plane in cells.</summary>
        public const int Width = 32;
    }

    /// <summary>The timing of the repeating crossfades between the narration and the flashback scenes.</summary>
    public static class Crossfade
    {
        /// <summary>The crossfade counter's starting value.</summary>
        public const int InitialCounter = 127;
        /// <summary>Updates per crossfade step.</summary>
        public const int StepFrames = 4;
    }

    /// <summary>
    /// The old Mother Brain fight's blinking background lights (#1733), started as a crossfade into a flashback ends while
    /// Samus draws first, and ended on the update the cinematic starts page two.
    /// </summary>
    public const ColourEffectId FightLights = ColourEffectId.OldMotherBrainFightBackgroundLights;

    /// <summary>The music the opening's scene changes start.</summary>
    public static class Music
    {
        /// <summary>The song each loaded cinematic music set starts: its first own song.</summary>
        public const MusicTrack SceneTrack = MusicTrack.FirstSetSong;
        /// <summary>The queue delay the scene track is requested with.</summary>
        public const int SceneTrackDelay = 14;
        /// <summary>The countdown before the discovery scene's music begins.</summary>
        public const int DiscoveryInitialTimer = 14;
    }

    /// <summary>Screen-fade cadences of the opening's scene changes.</summary>
    public static class Fade
    {
        /// <summary>The narration's fades: one brightness level every two updates.</summary>
        public const int NarrationSlowFade = 2;
        /// <summary>The final fade: one brightness level every second update.</summary>
        public const int FinishFade = 1;
    }

    /// <summary>The actors' sounds.</summary>
    public static class Objects
    {
        /// <summary>The queue limit the opening's sounds are requested under.</summary>
        public static readonly SoundRequestPriority MaximumQueuedSounds = SoundRequestPriority.Normal;
        /// <summary>The Baby Metroid's first cry.</summary>
        public static readonly SoundEffectId BabyCry1 = RoomSound.BabyMetroidCry1;
        /// <summary>The typewriter sound while narration characters appear.</summary>
        public static readonly SoundEffectId Typewriter = RoomSound.IntroTypewriter;
        /// <summary>The sound of the Baby Metroid hatching.</summary>
        public static readonly SoundEffectId EggHatch = EnemySound.ContactKill;
    }

    /// <summary>The narration caret's positions and the text characters' size.</summary>
    public static class ObjectSystem
    {
        /// <summary>The caret's leftmost column, in pixels.</summary>
        public const int CaretLeftX = 8;
        /// <summary>The caret's row on the first narration line.</summary>
        public const int CaretFirstTextY = 24;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixels from a text character's origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
        /// <summary>The Japanese text's next-page arrow's column, in pixels from the screen's left: the screen's middle (#1655).</summary>
        public const int NextPageArrowX = 128;
        /// <summary>The Japanese text's next-page arrow's row, in pixels from the screen's top: below the page's text (#1655).</summary>
        public const int NextPageArrowY = 216;
    }
}
