namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$81 definition tables shared by the saved-game area and room maps.</summary>
public static class FileSelectMapRomData
{
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
    /// <summary>$82:965F, twelve-word area label pointers in bank $82.</summary>
    public const int RoomLabelPointers = 0x82965f;
    /// <summary>BG12NBA=$33 selects the map and frame character base.</summary>
    public const ushort RoomCharacters = 0x3000;
    /// <summary>$81:A725 fills the unused BG2 frame words with $2801.</summary>
    public const ushort RoomFrameBlank = 0x2801;
    /// <summary>$82:9628 clears palette bit $1000 in the room-select area label.</summary>
    public const ushort RoomLabelMask = 0xefff;
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
}
