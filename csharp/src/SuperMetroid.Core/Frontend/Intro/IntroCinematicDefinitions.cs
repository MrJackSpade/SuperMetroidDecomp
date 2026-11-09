namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening cinematic's shared definitions: its text layout, palette fades, music, actors' palettes and sounds, and the narration caret.</summary>
public static class IntroCinematicDefinitions
{
    /// <summary>The planes the opening's scene and text are drawn on.</summary>
    public static class Planes
    {
        /// <summary>Width of an opening plane in cells.</summary>
        public const int Width = 32;
    }

    /// <summary>Blank text cells, the Japanese blank glyph and the final line's placement.</summary>
    public static class Text
    {
        /// <summary>The first gameplay text cell cleared.</summary>
        public const int GameplayBlankStartIndex = 128;
        /// <summary>Gameplay text cells cleared.</summary>
        public const int GameplayBlankWordCount = 640;
        /// <summary>The first cell the final text line replaces.</summary>
        public const int FinalLineFirstCell = 768;
    }

    /// <summary>The colours the repeating flashback and narration crossfades fade.</summary>
    public static class Palette
    {
        /// <summary>The crossfade counter's starting value.</summary>
        public const int CrossfadeInitialCounter = 127;
        /// <summary>Updates per crossfade colour step.</summary>
        public const int CrossfadeStepFrames = 4;
        /// <summary>The colours faded out as a scene replaces the narration.</summary>
        public static IntroFadeColours Gameplay { get; } = new(
            [IntroBackgroundFade.GameplayBackground, IntroBackgroundFade.Environment], [IntroSpriteFade.Samus]);
        /// <summary>The colours cleared and faded back in as the narration returns from a scene.</summary>
        public static IntroFadeColours GameplayClear { get; } = new(
            [IntroBackgroundFade.ClearedBackground, IntroBackgroundFade.Environment], [IntroSpriteFade.Samus]);
        /// <summary>The Mother Brain scene's colours, faded in and out with it.</summary>
        public static IntroFadeColours Narration { get; } = new(
            [IntroBackgroundFade.NarrationText, IntroBackgroundFade.NarrationBackground],
            [IntroSpriteFade.NarrationPortrait, IntroSpriteFade.NarrationObjects]);
        /// <summary>The Metroid discovery scenes' colours, faded in and out with them.</summary>
        public static IntroFadeColours Discovery { get; } = new(
            [IntroBackgroundFade.DiscoveryBackground], [IntroSpriteFade.DiscoveryScientists]);
        /// <summary>The background groups that hold the narration text's colours, of which a transition moves at most one.</summary>
        public static IReadOnlyList<IntroBackgroundFade> NarrationColours { get; } =
            [IntroBackgroundFade.GameplayBackground, IntroBackgroundFade.ClearedBackground];
    }

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
        /// <summary>The caret's row before narration begins.</summary>
        public const int CaretInitialY = 248;
        /// <summary>The caret's leftmost column, in pixels.</summary>
        public const int CaretLeftX = 8;
        /// <summary>The caret's row on the first narration line.</summary>
        public const int CaretFirstTextY = 24;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixels from a character's origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
    }
}
