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
        /// <summary>The Samus portrait's 32-by-32 tilemap.</summary>
        public const int SamusHeadTilemapBytes = 32 * 32 * sizeof(ushort);
        /// <summary>The scenes' four 32-by-32 background tilemap pages.</summary>
        public const int BackgroundPageTilemapBytes = 4 * 32 * 32 * sizeof(ushort);
        /// <summary>The cinematic sprites' 288 four-bit characters.</summary>
        public const int ObjectCharacterBytes = 288 * FourBitCharacterBytes;
        /// <summary>One 32-by-32 narration tilemap.</summary>
        public const int NarrationTilemapBytes = 32 * 32 * sizeof(ushort);
        /// <summary>The 96 two-bit characters the Japanese narration blanks with its blank glyph.</summary>
        public const int JapaneseBlankCharactersByteCount = 96 * 16;
    }

    /// <summary>The opening's text tilemaps.</summary>
    public static class Tilemaps
    {
        /// <summary>Cells of a complete 32-by-32 text tilemap.</summary>
        public const int TextTilemapWordCount = TilemapWidth * TilemapWidth;
        /// <summary>The cells of a text tilemap the narration shows: its first thirty rows.</summary>
        public const int VisibleTextCellCount = 30 * TilemapWidth;
        /// <summary>Width of an opening tilemap in cells.</summary>
        public const int TilemapWidth = 32;
    }

    /// <summary>Blank text cells, the Japanese blank glyph and the final line's placement.</summary>
    public static class Text
    {
        /// <summary>The blank cell written over the gameplay text region.</summary>
        public static readonly SnesBgTilemapWord Blank = new(0x002f);
        /// <summary>The blank cell of the Japanese narration layout: the blank glyph in front, palette seven.</summary>
        public static readonly SnesBgTilemapWord JapaneseBlank = new(0x3c29);
        /// <summary>The blank cell around the final text line: the blank glyph, palette seven.</summary>
        public static readonly SnesBgTilemapWord FinalBlank = new(0x1c29);
        /// <summary>The first gameplay text cell cleared.</summary>
        public const int GameplayBlankStartIndex = 128;
        /// <summary>Gameplay text cells cleared.</summary>
        public const int GameplayBlankWordCount = 640;
        /// <summary>Where the Japanese blank glyph starts in the font: its 41st character.</summary>
        public const int JapaneseBlankSourceOffset = 41 * 16;
        /// <summary>Cells from the upper to the lower Japanese blank region.</summary>
        public const int JapaneseBlankBottomDelta = 896;
        /// <summary>The first cell the final text line replaces.</summary>
        public const int FinalLineFirstCell = 768;
        /// <summary>Cells copied for the final text line.</summary>
        public const int FinalLineWordCount = 128;
        /// <summary>The cell blanked immediately left of the final text.</summary>
        public const int FinalBlankLeftIndex = 911;
        /// <summary>The cell blanked immediately right of the final text.</summary>
        public const int FinalBlankRightIndex = 912;
    }

    /// <summary>The colour runs the repeating flashback and narration crossfades fade.</summary>
    public static class Palette
    {
        /// <summary>The crossfade counter's starting value.</summary>
        public const ushort CrossfadeInitialCounter = 127;
        /// <summary>Updates per crossfade colour step.</summary>
        public const int CrossfadeStepFrames = 4;
        /// <summary>Colour runs faded for the gameplay flashback.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Gameplay { get; } = [GameplayBackground, GameplayEnvironment, GameplaySamus];
        /// <summary>Colour runs cleared when the gameplay flashback begins.</summary>
        public static IReadOnlyList<IntroPaletteSpan> GameplayClear { get; } = [GameplayClearBackground, GameplayEnvironment, GameplaySamus];
        /// <summary>Colour runs faded for the narration screens.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Narration { get; } = [NarrationText, NarrationBackground, NarrationPortrait, NarrationObjects];
        /// <summary>Colour runs faded for the Metroid discovery scene.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Discovery { get; } = [DiscoveryBackground, DiscoveryObjects];

        /// <summary>The gameplay flashback's first background row and four colours of the next.</summary>
        private static IntroPaletteSpan GameplayBackground => new(0, 16 + 4);
        /// <summary>Gameplay initialization clears only the first background row.</summary>
        private static IntroPaletteSpan GameplayClearBackground => new(0, 16);
        /// <summary>The gameplay flashback's background row 3.</summary>
        private static IntroPaletteSpan GameplayEnvironment => new(3 * 16, 16);
        /// <summary>Samus's six colours at sprite row 6, colour 9.</summary>
        private static IntroPaletteSpan GameplaySamus => new((8 + 6) * 16 + 9, 6);
        /// <summary>The narration text's three colours at background row 1, colour 4.</summary>
        private static IntroPaletteSpan NarrationText => new(16 + 4, 3);
        /// <summary>The narration's background row 7.</summary>
        private static IntroPaletteSpan NarrationBackground => new(7 * 16, 16);
        /// <summary>The narration portrait's two sprite rows starting at row 4.</summary>
        private static IntroPaletteSpan NarrationPortrait => new((8 + 4) * 16, 2 * 16);
        /// <summary>The narration's sprite row 7.</summary>
        private static IntroPaletteSpan NarrationObjects => new((8 + 7) * 16, 16);
        /// <summary>The discovery scene's background row 2.</summary>
        private static IntroPaletteSpan DiscoveryBackground => new(2 * 16, 16);
        /// <summary>The scientists' nine colours in sprite row 6.</summary>
        private static IntroPaletteSpan DiscoveryObjects => new((8 + 6) * 16, 9);
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

    /// <summary>The actors' palettes and sounds.</summary>
    public static class Objects
    {
        /// <summary>The sprite palette of the discovery scene's actors.</summary>
        public const int DiscoveryPalette = 7;
        /// <summary>The sprite palette of the scientists.</summary>
        public const int ScientistPalette = 6;
        /// <summary>The sprite palette of the cinematic explosions.</summary>
        public const int ExplosionPalette = 5;
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
