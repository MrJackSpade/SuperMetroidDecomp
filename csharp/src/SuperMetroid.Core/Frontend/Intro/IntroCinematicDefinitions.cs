namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening cinematic's shared definitions: its installed artwork extents, text layout, palette fades, music, actors' palettes and sounds, and the narration caret.</summary>
public static class IntroCinematicDefinitions
{
    /// <summary>The extents, in bytes, of the installed opening artwork the cinematic requires.</summary>
    public static class Artwork
    {
        /// <summary>The bytes of one four-bit character.</summary>
        private const int FourBitCharacterBytes = 32;
        /// <summary>The scenes' four-bit background characters.</summary>
        private const int BackgroundCharacterCount = 1024;
        /// <summary>The scenes' 1024 four-bit background characters.</summary>
        public const int BackgroundCharacterBytes = BackgroundCharacterCount * FourBitCharacterBytes;
        /// <summary>The narration font's 144 two-bit characters.</summary>
        public const int FontOneBytes = 144 * 16;
        /// <summary>The cells of one 32-by-32 narration tilemap.</summary>
        public const int NarrationTilemapCells = 32 * 32;
        /// <summary>The 96 two-bit characters the Japanese narration blanks with its blank glyph.</summary>
        public const int JapaneseBlankCharactersByteCount = 96 * 16;
    }

    /// <summary>The opening's text tilemaps.</summary>
    public static class Tilemaps
    {
        /// <summary>Width of an opening tilemap in cells.</summary>
        public const int TilemapWidth = 32;
    }

    /// <summary>Blank text cells, the Japanese blank glyph and the final line's placement.</summary>
    public static class Text
    {
        /// <summary>The first gameplay text cell cleared.</summary>
        public const int GameplayBlankStartIndex = 128;
        /// <summary>Gameplay text cells cleared.</summary>
        public const int GameplayBlankWordCount = 640;
        /// <summary>Where the Japanese blank glyph starts in the font: its 41st character.</summary>
        public const int JapaneseBlankSourceOffset = 41 * 16;
        /// <summary>The first cell the final text line replaces.</summary>
        public const int FinalLineFirstCell = 768;
        /// <summary>Cells copied for the final text line.</summary>
        public const int FinalLineWordCount = 128;
    }

    /// <summary>The colour runs the repeating flashback and narration crossfades fade.</summary>
    public static class Palette
    {
        /// <summary>The crossfade counter's starting value.</summary>
        public const ushort CrossfadeInitialCounter = 127;
        /// <summary>Updates per crossfade colour step.</summary>
        public const int CrossfadeStepFrames = 4;
        /// <summary>The screen colours faded for the gameplay flashback.</summary>
        public static IReadOnlyList<ScreenColourGroup> Gameplay { get; } =
            [ScreenColourGroup.IntroGameplayBackground, ScreenColourGroup.IntroGameplayEnvironment, ScreenColourGroup.IntroGameplaySamus];
        /// <summary>The screen colours cleared when the gameplay flashback begins.</summary>
        public static IReadOnlyList<ScreenColourGroup> GameplayClear { get; } =
            [ScreenColourGroup.IntroGameplayClearedBackground, ScreenColourGroup.IntroGameplayEnvironment, ScreenColourGroup.IntroGameplaySamus];
        /// <summary>The screen colours faded for the narration screens.</summary>
        public static IReadOnlyList<ScreenColourGroup> Narration { get; } =
            [ScreenColourGroup.IntroNarrationText, ScreenColourGroup.IntroNarrationBackground,
                ScreenColourGroup.IntroNarrationPortrait, ScreenColourGroup.IntroNarrationObjects];
        /// <summary>The screen colours faded for the Metroid discovery scene.</summary>
        public static IReadOnlyList<ScreenColourGroup> Discovery { get; } =
            [ScreenColourGroup.IntroDiscoveryBackground, ScreenColourGroup.IntroDiscoveryScientists];
    }

    /// <summary>The music the opening's scene changes start.</summary>
    public static class Music
    {
        /// <summary>The track each loaded cinematic music set starts.</summary>
        public const byte SceneTrack = 5;
        /// <summary>The queue delay the scene track is requested with.</summary>
        public const ushort SceneTrackDelay = 14;
        /// <summary>The countdown before the discovery scene's music begins.</summary>
        public const int DiscoveryInitialTimer = 14;
    }

    /// <summary>Screen-fade cadences of the opening's scene changes.</summary>
    public static class Fade
    {
        /// <summary>The narration's fades: one brightness level every two updates.</summary>
        public const ushort NarrationSlowFade = 2;
        /// <summary>The final fade: one brightness level every second update.</summary>
        public const ushort FinishFade = 1;
    }

    /// <summary>The actors' sounds.</summary>
    public static class Objects
    {
        /// <summary>The queue limit the opening's sounds are requested under.</summary>
        public static readonly SoundQueueLimit MaximumQueuedSounds = SoundQueueLimit.Max6;
        /// <summary>The Baby Metroid's first cry.</summary>
        public static readonly SoundEffectId BabyCry1 = Library3Sound.BabyMetroidCry1;
        /// <summary>The typewriter sound while narration characters appear.</summary>
        public static readonly SoundEffectId Typewriter = Library3Sound.IntroTypewriter;
        /// <summary>The sound of the Baby Metroid hatching.</summary>
        public static readonly SoundEffectId EggHatch = Library2Sound.ContactKill;
    }

    /// <summary>The narration caret's positions and the text characters' size.</summary>
    public static class ObjectSystem
    {
        /// <summary>The caret's row before narration begins.</summary>
        public const ushort CaretInitialY = 248;
        /// <summary>The caret's leftmost column, in pixels.</summary>
        public const ushort CaretLeftX = 8;
        /// <summary>The caret's row on the first narration line.</summary>
        public const ushort CaretFirstTextY = 24;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixels from a character's origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
    }
}
