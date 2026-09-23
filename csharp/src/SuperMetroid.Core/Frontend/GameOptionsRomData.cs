namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge resources and immutable screen layout for game-state-$02 options.
/// </summary>
/// <remarks>
/// These values describe ROM data or fixed menu geometry, not mutable navigation state.
/// Keeping them outside <see cref="GameOptionsMenuState"/> leaves that class responsible
/// only for interpreting input, changing pages, and publishing frames.
/// </remarks>
public static class GameOptionsRomData
{
    public const int MenuBank = 0x820000;
    public const int TilemapByteCount = 0x0800;
    public const int MaximumBrightness = 15;
    public const int ControllerScrollLimit = 32;
    public const int ControllerScrollPixelsPerFrame = 2;
    public const int MenuTilemapWidth = 32;
    public const int MenuTilemapHeight = 32;
    public const int MaximumQueuedMenuSounds = 6;

    /// <summary>The five consecutive compressed options pages loaded from $82:EC66 onward.</summary>
    /// <remarks>
    /// Issues #625 and #956: pinned NTSC J/U v1.0 ROM source-load instructions at
    /// $82:EC66, EC77, EC88, EC99, and ECAA select these five bank-$97 streams in
    /// primary, controller English/Japanese, special English/Japanese order.
    /// bank_94..99.asm places them at $97:8DF4, 8FCD, 91C4, 938D, and 953A with
    /// compressed lengths $1D9, $1F7, $1C9, $1AD, and $1BA, respectively; the
    /// final stream ends at $97:96F4. All five ROM immediate pairs and consecutive
    /// source spans match. GameOptionsMenuState and extraction select the five
    /// named resources, each expanding to one $800-byte tilemap. Their offsets are
    /// authored compression boundaries; a prefix sum would just hide the lengths.
    /// Retain this explicit page-to-resource mapping.
    /// </remarks>
    public static class Pages
    {
        public static readonly GameOptionsPageResource Primary =
            new(0x978df4, "primary");
        public static readonly GameOptionsPageResource ControllerEnglish =
            new(0x978fcd, "English controller");
        public static readonly GameOptionsPageResource ControllerJapanese =
            new(0x9791c4, "Japanese controller");
        public static readonly GameOptionsPageResource SpecialEnglish =
            new(0x97938d, "English special-settings");
        public static readonly GameOptionsPageResource SpecialJapanese =
            new(0x97953a, "Japanese special-settings");
    }

    /// <summary>Fixed row identities and scroll boundaries for each menu page.</summary>
    public static class Rows
    {
        public const int PrimaryStartGame = 0;
        public const int PrimaryControllerSettings = 3;
        public const int PrimarySpecialSettings = 4;
        public const int PrimaryCount = 5;
        public const int ControllerActionCount = 7;
        public const int ControllerExit = 7;
        public const int ControllerReset = 8;
        public const int ControllerCount = 9;
        public const int SpecialIconCancel = 0;
        public const int SpecialMoonwalk = 1;
        public const int SpecialExit = 2;
        public const int SpecialCount = 3;
    }

    /// <summary>Cursor positions read by <c>OptionsPreInstr_F2A9</c>.</summary>
    public static class Cursors
    {
        private static readonly ushort[] PrimaryRows = [0x38, 0x58, 0x70, 0x90, 0xb0];
        private static readonly ushort[] ControllerRows =
            [0x30, 0x48, 0x60, 0x78, 0x90, 0xa8, 0xc0, 0xb8, 0xd0];
        private static readonly ushort[] SpecialRows = [0x40, 0x70, 0xa0];

        public const ushort PrimaryX = 0x18;
        public const ushort ControllerX = 0x28;
        /// <summary>$82:F2E0: selector X when the active page phase has no cursor table (including scroll phases).</summary>
        public const ushort HiddenX = 0x180;
        /// <summary>$82:F2E6: selector Y paired with the off-screen X anchor.</summary>
        public const ushort HiddenY = 0x10;
        public const ushort SpecialX = 0x10;
        /// <summary>Primary options cursor Y for selected row 0..4.</summary>
        /// <remarks>
        /// Issues #625 and #957: PreInstruction_MenuSelectionMissile selects five
        /// interleaved X/Y records at $82:F307, with Y every four bytes from $82:F309.
        /// For row i=0..4, X=$0018 and Y=$0038+$0020*i-(i&gt;=2 ? 8 : 0).
        /// The eight-pixel shift after row 1 matches the authored layout gap.
        /// Pinned NTSC J/U v1.0 ROM and bank_82.asm match all five records;
        /// GameOptionsMenuState wraps PrimaryCount at five before indexing.
        /// </remarks>
        public static ReadOnlySpan<ushort> PrimaryY => PrimaryRows;
        /// <summary>$82:F31D and subsequent Y words are screen-space anchors; END/RESET already account for page scrolling.</summary>
        public static ReadOnlySpan<ushort> ControllerY => ControllerRows;
        public static ReadOnlySpan<ushort> SpecialY => SpecialRows;
    }

    /// <summary>Controller-label source pointers and destination boxes from $82:F639.</summary>
    public static class ControllerLabels
    {
        private static readonly ushort[] DestinationOffsets =
            [0x016e, 0x022e, 0x02ee, 0x03ae, 0x046e, 0x052e, 0x05ee];
        private static readonly ushort[] SourcePointers =
            [0xf659, 0xf665, 0xf671, 0xf67d, 0xf689, 0xf695, 0xf6a1];

        public const int WidthInTiles = 3;
        public const int HeightInTiles = 2;
        public static ReadOnlySpan<ushort> Destinations => DestinationOffsets;
        public static ReadOnlySpan<ushort> Sources => SourcePointers;
    }

    /// <summary>Language-dependent palette regions in the primary-page tilemap.</summary>
    public static ReadOnlySpan<GameOptionsLanguagePaletteRegion> LanguagePaletteRegions =>
        LanguagePaletteRegionData;

    private static readonly GameOptionsLanguagePaletteRegion[] LanguagePaletteRegionData =
    [
        new(0x0288, 0x18, HighlightWhenJapanese: true),
        new(0x02c8, 0x18, HighlightWhenJapanese: true),
        new(0x0348, 0x32, HighlightWhenJapanese: false),
        new(0x0388, 0x32, HighlightWhenJapanese: false),
    ];

    /// <summary>Palette boxes for the Icon Cancel and Moonwalk toggles.</summary>
    public static class SpecialToggles
    {
        public static readonly GameOptionsToggleLayout IconCancel =
            new(0x01e0, 0x0220, 0x01ee, 0x022e);
        public static readonly GameOptionsToggleLayout Moonwalk =
            new(0x0360, 0x03a0, 0x036e, 0x03ae);
        public const int PaletteRegionByteCount = 0x0c;
    }

    /// <summary>Typed palette indices used to select and dim menu text.</summary>
    public static class TilePalettes
    {
        public const int Selected = 0;
        public const int Unselected = 1;
    }

    /// <summary>Static and animated options-screen OBJ definitions.</summary>
    public static class Spritemaps
    {
        private static readonly ushort[] MissileFrames = [0x37, 0x36, 0x35, 0x34];

        public const ushort OptionModeBorder = 0x4b;
        public const ushort OptionModeBorderX = 0x7c;
        public const ushort OptionModeBorderY = 0x10;
        /// <summary>$82:F48E selects $82:D2F7, menu spritemap $4C at table entry $82:C601. The pinned disassembly's label incorrectly says 49.</summary>
        public const ushort ControllerModeBorder = 0x4c;
        /// <summary>$82:F353, controller-heading border setup X position.</summary>
        public const ushort ControllerModeBorderX = 0x84;
        /// <summary>$82:F49E, SPECIAL SETTING MODE border instruction list selects menu spritemap $4D.</summary>
        public const ushort SpecialModeBorder = 0x4d;
        /// <summary>$82:F35B, special-heading border setup X position.</summary>
        public const ushort SpecialModeBorderX = 0x80;
        /// <summary>All four $82:BAAA menu missile timer words equal eight calls.</summary>
        /// <remarks>Issues #625 and #955: pinned NTSC J/U v1.0 ROM and bank_82.asm
        /// match 4/4. The timer reload is shared with file select and game over; the
        /// adjacent $82:BAB2 spritemap IDs are a separate table. Options animation
        /// may use an editable presentation override.</remarks>
        public const int MissileFrameDuration = 8;
        /// <summary>Shared $82:BAB2 menu missile IDs, exactly $0037-frame for frame 0..3.</summary>
        /// <remarks>Issues #625 and #954: options animation wraps modulo this four-word
        /// span, and asset extraction iterates the same bounded domain. File select and
        /// game over expose the same native table; all four words match pinned NTSC J/U
        /// v1.0 ROM and bank_82.asm.</remarks>
        public static ReadOnlySpan<ushort> MissileFrameIds => MissileFrames;
    }
}

/// <summary>One compressed options-page resource and its diagnostic name.</summary>
public readonly record struct GameOptionsPageResource(int Address, string Description);

/// <summary>One primary-page byte range whose palette identifies the active language.</summary>
public readonly record struct GameOptionsLanguagePaletteRegion(
    int ByteOffset,
    int ByteCount,
    bool HighlightWhenJapanese);

/// <summary>Four palette boxes belonging to one binary special-setting row.</summary>
public readonly record struct GameOptionsToggleLayout(
    int EnabledTop,
    int EnabledBottom,
    int DisabledTop,
    int DisabledBottom);
