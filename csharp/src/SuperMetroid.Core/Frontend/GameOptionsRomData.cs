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
    /// <remarks>
    /// Issues #625 and #965: native SpecialSettingRAMAddresses at $82:F0AE maps
    /// special row 0 to IconCancel $09EA and row 1 to Moonwalk $09E4.
    /// The three-entry dispatcher at $82:F088 sends rows 0 and 1 to the toggle
    /// routine and row 2 directly to Exit, so the address table has no row-2
    /// value. Pinned NTSC J/U v1.0 ROM and bank_82.asm match both addresses and
    /// all three dispatch pointers. StepSpecial makes the same named two-row
    /// selection. The two WRAM identities are independent state-policy choices;
    /// their six-byte difference is not a useful generator. Retain the explicit
    /// row-to-setting branches and bounds.
    /// </remarks>
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
    /// <remarks>
    /// Issues #625 and #967: the 13-word phase selector at $82:F2ED is indexed
    /// by twice GameOptionsMenuIndex, for native phases 0..C. Phases 2, 3, and B
    /// select primary X/Y records at $F307; 7 selects controller $F31B;
    /// 8 selects special $F33F. Phases 0, 1, 4, 5, 6, 9, A, and C select zero,
    /// which places the missile offscreen at X=$0180, Y=$0010. A nonzero base
    /// is then indexed by four times MenuOptionIndex. Pinned NTSC J/U v1.0 ROM
    /// and bank_82.asm match all 13 pointers. This is authored phase policy,
    /// not a numerical progression. Managed CursorPosition maps dissolve out/in,
    /// controller scroll, and fade out to intro to their null-table phases;
    /// file-select fade-out retains its native primary-page cursor position.
    /// </remarks>
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
        /// <remarks>
        /// Issues #625 and #958: PreInstruction_MenuSelectionMissile selects nine
        /// interleaved X/Y records from $82:F31B; Y is at $82:F31D+4*i for row
        /// i=0..8. X=$0028 throughout, and Y=$0030+$0018*i-(i&gt;=7 ? $20 : 0).
        /// The final two rows are Exit and Reset, shifted upward by $20 to account
        /// for the controller-page scroll. Pinned NTSC J/U v1.0 ROM and bank_82.asm
        /// match all nine records; GameOptionsMenuState keeps the selected row in
        /// 0..8 across the controller scroll transitions.
        /// </remarks>
        public static ReadOnlySpan<ushort> ControllerY => ControllerRows;
        /// <summary>Special-settings cursor Y for selected row 0..2.</summary>
        /// <remarks>
        /// Issues #625 and #959: PreInstruction_MenuSelectionMissile selects three
        /// interleaved X/Y records from $82:F33F; Y at $82:F341+4*i is exactly
        /// $0040+$0030*i for i=0..2, while X stays $0010. Pinned NTSC J/U v1.0
        /// ROM and bank_82.asm match all three records. GameOptionsMenuState
        /// wraps SpecialCount at three before indexing, and extraction uses the
        /// same three anchors.
        /// </remarks>
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
        /// <summary>BG1 byte destination for each controller-action label, row 0..6.</summary>
        /// <remarks>
        /// Issues #625 and #960: GameOptionsMenu_TilemapOffsets at $82:F639 contains
        /// seven little-endian words exactly $016E+$00C0*i for action row i=0..6.
        /// Pinned NTSC J/U v1.0 ROM and bank_82.asm match all seven. Native uses
        /// twice the row index to read the word; managed label drawing and asset
        /// extraction use the same seven-row domain. The $C0-byte step places each
        /// 3x2 label box three 32-word tilemap rows below the previous one.
        /// The adjacent source-pointer region has its own rule.
        /// </remarks>
        public static ReadOnlySpan<ushort> Destinations => DestinationOffsets;
        /// <summary>Bank-$82 source pointer for each assignable controller-button label.</summary>
        /// <remarks>
        /// Issues #625 and #961: ControllerButton_TilemapPointers at $82:F647
        /// contains nine words. Selectors i=0..6 point to X, A, B, Select, Y, L,
        /// and R label records at $F659+$000C*i. Each record is a 3x2 tilemap
        /// of six words. Adjacent selectors 7 and 8 both point to the distinct
        /// OFF record at $F6AD; the managed assignable-button view stops at 6.
        /// Pinned NTSC J/U v1.0 ROM and bank_82.asm match all nine pointer words.
        /// Managed label drawing and extraction use the seven-entry domain.
        /// </remarks>
        public static ReadOnlySpan<ushort> Sources => SourcePointers;
    }

    /// <summary>Language-dependent palette regions in the primary-page tilemap.</summary>
    /// <remarks>
    /// Issues #625 and #962: pinned NTSC J/U v1.0 ROM and bank_82.asm match all
    /// eight offset/count/palette calls in Set_Language_Text_Option_Highlight.
    /// For region i=0..3, byte offset is $0288+$0040*(i%2)+$00C0*(i/2);
    /// byte count is $18 for i&lt;2 and $32 otherwise. Native AltText=0 selects
    /// palette 0 for the first pair and palette 1 for the second; AltText=1
    /// reverses them. With the consumers' equality test, the native
    /// HighlightWhenJapanese flags are false,false,true,true. Both ROM-page
    /// and editable-presentation rendering consume this same polarity.
    /// </remarks>
    public static ReadOnlySpan<GameOptionsLanguagePaletteRegion> LanguagePaletteRegions =>
        LanguagePaletteRegionData;

    private static readonly GameOptionsLanguagePaletteRegion[] LanguagePaletteRegionData =
    [
        new(0x0288, 0x18, HighlightWhenJapanese: false),
        new(0x02c8, 0x18, HighlightWhenJapanese: false),
        new(0x0348, 0x32, HighlightWhenJapanese: true),
        new(0x0388, 0x32, HighlightWhenJapanese: true),
    ];

    /// <summary>Palette boxes for the Icon Cancel and Moonwalk toggles.</summary>
    /// <remarks>
    /// Issues #625 and #964: the eight native words at $82:F149..F158 are one
    /// interleaved block used by Set_SpecialSetting_Highlights. For toggle
    /// t=0 (Icon Cancel) or 1 (Moonwalk), row r=0..1, and disabled choice
    /// d=0..1, the byte offset is $01E0+$0180*t+$0040*r+$000E*d.
    /// Pinned NTSC J/U v1.0 ROM and bank_82.asm match all eight offsets.
    /// Each palette call covers $0C bytes (six tilemap words); when the setting
    /// is on, the enabled pair uses palette 0 and disabled pair palette 1,
    /// reversing when off. Native indexes the records by toggle 0..1 before
    /// choosing the four boxes. The two managed layouts are views of this
    /// single bounded geometry rule.
    /// </remarks>
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
    /// <remarks>
    /// Issues #625 and #966: for options page p=0..2 (Primary, Controller,
    /// Special), heading-border spritemap ID is $4B+p. The $82:C5FF/C601/C603
    /// pointer-table words and $82:F47E/F48E/F49E instruction lists select
    /// $82:D24B/D2F7/D41B, respectively. Setup routines at $82:F34B/F353/F35B
    /// give X anchors $7C/$84/$80; common setup at $82:F369 gives Y=$10.
    /// Pinned NTSC J/U v1.0 ROM and bank_82.asm match all three tuples.
    /// The IDs have a bounded consecutive rule, while the X anchors are authored
    /// page layout. The disassembly's controller symbol says 49, but its table
    /// position and actual spritemap header identify $4C.
    /// </remarks>
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
