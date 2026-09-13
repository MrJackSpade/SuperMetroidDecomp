namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$81 definition tables shared by the saved-game area and room maps.</summary>
public static class FileSelectMapRomData
{
    /// <summary>Menu OBSEL=$03 selects the shared menu OBJ character base and size pair.</summary>
    public const byte ObjectSelection = 3;
    /// <summary>Room-select BG2 vertical scroll, aligning the fixed frame to the visible map viewport.</summary>
    public const ushort RoomFrameVerticalScroll = 24;
    /// <summary>Bank $82 contains menu spritemaps and save-point coordinate lists.</summary>
    public const int MenuObjectBank = 0x820000;
    /// <summary><c>$81:B71A</c>, AreaSelectForegroundTilemap, one 32x32 BG1 page.</summary>
    public const int AreaForeground = 0x81b71a;
    /// <summary><c>$81:BF1A</c>, six 32x32 BG3 area-background pages.</summary>
    public const int AreaBackgrounds = 0x81bf1a;
    /// <summary><c>$81:A4CE</c>, active-area palette-program offsets.</summary>
    public const int ActivePaletteOffsets = 0x81a4ce;
    /// <summary><c>$81:A4DA</c>, inactive-area palette-program offsets.</summary>
    public const int InactivePaletteOffsets = 0x81a4da;
    /// <summary><c>$81:A4E6</c>, source/destination palette byte offsets terminated by $FFFF.</summary>
    public const int PalettePrograms = 0x81a4e6;
    /// <summary><c>$81:A40E</c>, palette colors copied in five-word groups.</summary>
    public const int PaletteColors = 0x81a40e;
    /// <summary>Menu BG3SC=$5C: area-background tilemap VRAM word base.</summary>
    public const ushort AreaBackgroundVram = 0x5c00;
    /// <summary>Menu BG34NBA=$04: BG3 character VRAM word base.</summary>
    public const ushort AreaBackgroundCharacters = 0x4000;
    /// <summary>One native menu tilemap DMA is $800 bytes.</summary>
    public const int TilemapBytes = 0x800;
    /// <summary><c>$82:C749</c>, PLANET ZEBES spritemap ID followed by geographic-area label IDs.</summary>
    public const int LabelSpritemapBase = 0x82c749;
    /// <summary><c>$82:C80B</c>, bank-$82 save-point map-coordinate-list pointers per area.</summary>
    public const int SavePointMapPointers = 0x82c80b;
    /// <summary><c>$81:AA1C</c>, FileSelectMap_Labels_Positions: X/Y words per geographic area.</summary>
    public const int LabelPositions = 0x81aa1c;
    /// <summary><c>$81:AA34</c>, RoomSelectMap_ExpandingSquare_Velocities: four low/high pairs per area.</summary>
    public const int WindowVelocities = 0x81aa34;
    /// <summary><c>$81:AA94</c>, RoomSelectMap_ExpandingSquare_Timers: completion occurs on signed underflow.</summary>
    public const int WindowTimers = 0x81aa94;
    /// <summary><c>$81:AAA0</c>, FileSelectMapArea_IndexTable: display-order to geographic-area mapping.</summary>
    public const int DisplayAreaIndices = 0x81aaa0;
    /// <summary>Six Zebes areas participate; Ceres has no world-map selection entry.</summary>
    public const int AreaCount = 6;
    /// <summary>$B6:E000, first 800 words of the room-selection BG2 frame.</summary>
    public const int RoomFrame = 0xb6e000;
    /// <summary>$81:B14B, words 1..160 supply BG2 frame words 800..959.</summary>
    public const int RoomFrameFooter = 0x81b14b;
    /// <summary>$81:A725 copies the first 800 room-frame words before filling the remainder.</summary>
    public const int RoomFrameHeaderWords = 800;
    /// <summary>$81:A7CA copies control-footer words 1..160 into frame words 800..959.</summary>
    public const int RoomFrameFooterWords = 160;
    /// <summary>$82:9628 writes the selected area name starting at frame word 170.</summary>
    public const int RoomLabelDestinationWord = 170;
    /// <summary>$82:9628 copies twelve area-label tile words.</summary>
    public const int RoomLabelWords = 12;
    /// <summary>$82:965F, twelve-word area label pointers in bank $82.</summary>
    public const int RoomLabelPointers = 0x82965f;
    /// <summary>BG12NBA=$33 selects the map and frame character base.</summary>
    public const ushort RoomCharacters = 0x3000;
    /// <summary>$81:A725 fills the unused BG2 frame words with $2801.</summary>
    public const ushort RoomFrameBlank = 0x2801;
    /// <summary>$82:9628 clears palette bit $1000 in the room-select area label.</summary>
    public const ushort RoomLabelMask = 0xefff;
    /// <summary>$82:B6DD draws spritemap $12 behind the station marker on even animation loops.</summary>
    public const ushort StationMarkerBacking = 0x12;
    /// <summary>$82:B6DD selects OBJ palette seven for the load-station marker.</summary>
    public const ushort StationMarkerPalette = 0x0e00;
    /// <summary>$81:AF32, four ten-byte arrow records: X, Y, animation, held-button mask, direction.</summary>
    public const int ScrollArrows = 0x81af32;
    /// <summary>MapScrolling speed table emits its eight-pixel pulse on the fourth update.</summary>
    public const int ScrollPulseTick = 4;
    /// <summary>$82:9299 completes a step when its two-byte speed index reaches $10.</summary>
    public const int ScrollStepTicks = 8;
    /// <summary>MapScrolling speed table's only nonzero displacement is one eight-pixel map cell.</summary>
    public const int ScrollStepPixels = 8;
    /// <summary>$81:AFF6 shortens the return window's area timer by twelve updates.</summary>
    public const int ReturnWindowTimerReduction = 12;
    /// <summary>$81:AFF6 starts the return window eight pixels inside the viewport.</summary>
    public const int ReturnWindowInset = 8;
    /// <summary>$81:AF5A handles both dispatcher entries eleven and twelve before fading.</summary>
    public const int LoadPreludeFrames = 2;
    /// <summary>$81:AF83 waits thirty-two black frames before advancing to gameplay setup.</summary>
    public const int LoadBlackFrames = 32;
    /// <summary>Return setup runs entries fifteen through twenty before window contraction.</summary>
    public const int ReturnSetupFrames = 6;
    /// <summary>$8E:E400, LoadFileSelectPalettes reloads the full menu CGRAM image.</summary>
    public const int EntryPalette = 0x8ee400;
    /// <summary>$82:D9B8 uses denominator fifteen and calls transition colors starting at step one.</summary>
    public const int EntryPaletteDenominator = 15;
    /// <summary>$81:A61C initializes the entry window's upper margin to 111 scanlines.</summary>
    public const int EntryWindowTop = 111;
    /// <summary>$81:A61C initializes the entry window's inclusive left edge at 127.</summary>
    public const int EntryWindowLeft = 127;
    /// <summary>$81:A61C initializes the entry window's inclusive right edge at 129.</summary>
    public const int EntryWindowRight = 129;
    /// <summary>$81:A725 expands each window edge four pixels per update.</summary>
    public const int EntryWindowSpeed = 4;
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
}
