using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Pause BG2 presentation geometry and import-only native sources.</summary>
public static class PauseBackdropDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-backdrops.json";
    public const string MapAtlas = "Map";
    public const string InterfaceAtlas = "Interface";
    public const int Columns = 32, Rows = 32;
    public const int Cells = Columns * Rows, ByteCount = Cells * sizeof(ushort);
    public const int AtlasColumns = 32, AtlasRows = 8;
    public const int AtlasTileCount = AtlasColumns * AtlasRows;
    public const int ButtonRows = 16, ButtonCells = Columns * ButtonRows;
    /// <summary>$82:8EDA copies the mutable button image from $B6:E400 to WRAM $3400.</summary>
    public const int ButtonSource = PauseMenuRomData.ButtonTilemap;
    /// <summary>$82:8EDA LoadPauseScreen_BaseTilemaps copies the $B6:E000 BG2 image.</summary>
    public const int FrameSource = PauseMenuRomData.BackgroundTilemap;
    /// <summary>$82:93C3 loads the twelve-word area label through $82:965F.</summary>
    public const int LabelPointers = PauseMenuRomData.AreaMapLabelPointerTable;
    /// <summary>Area-label fragments use the fixed bank $82.</summary>
    public const int LabelBank = 0x820000;
    /// <summary>$82:93C3 writes the label at BG2 word $38AA, 170 words into its page.</summary>
    public const int LabelCell = 170, LabelWords = 12;
}
