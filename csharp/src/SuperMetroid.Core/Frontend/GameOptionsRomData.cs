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

    /// <summary>The five consecutive compressed options pages loaded by $82:EC77.</summary>
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
        public const ushort SpecialX = 0x10;
        public static ReadOnlySpan<ushort> PrimaryY => PrimaryRows;
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
        public const int MissileFrameDuration = 8;
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
