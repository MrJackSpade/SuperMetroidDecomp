using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>File identity, schema names and fixed dimensions of the installed <c>file-select.json</c>.</summary>
public static class FileSelectPresentationFormat
{
    /// <summary>Supported file-select presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for menu pages, fields, sprite compositions, and layout.</summary>
    public const string FileName = "file-select.json";
    /// <summary>Number of row-major tile cells required in each menu page.</summary>
    public const int CellCount = FileSelectPresentationDefinitions.Width * FileSelectPresentationDefinitions.Height;
    /// <summary>Size of each menu tilemap page in tile cells.</summary>
    public static Extent16<TilemapCells> PageSize => new(FileSelectPresentationDefinitions.Width, FileSelectPresentationDefinitions.Height);
    /// <summary>Width in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterColumns = 32;
    /// <summary>Height in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>The selected slot's helmet animation frames.</summary>
    public const int HelmetFrameCount = 8;
    /// <summary>The JSON name of the common background tilemap, separate from the foreground menu pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground pages, in document order.</summary>
    public static IReadOnlyList<FileSelectPage> Pages { get; } = Enum.GetValues<FileSelectPage>();
    /// <summary>The slot-field patches, in document order.</summary>
    public static IReadOnlyList<FileSelectPatch> Patches { get; } = Enum.GetValues<FileSelectPatch>();
}
