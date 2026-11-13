using SuperMetroid.Core.Frontend.GameOver.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>File identity and schema names of the installed <c>game-over.json</c>.</summary>
public static class GameOverPresentationFormat
{
    /// <summary>Supported game-over presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Installed filename of the game-over layout the game binds (#1638).</summary>
    public const string FileName = "game-over.json";
    /// <summary>Import-data filename of the extracted game-over document: its page, compositions, palette phases and layout (#1638).</summary>
    public const string ArtworkFileName = "game-over-artwork.json";
    /// <summary>The colours in each Baby palette phase.</summary>
    public const int BabyPaletteColorCount = 16;

    /// <summary>Required Baby, egg, and cursor frame identities in schema order.</summary>
    private static readonly string[] spriteNames =
    [
        "Baby.Closed", "Baby.Middle", "Baby.Open", GameOverPresentationDefinitions.EggFrame,
        "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
    ];

    /// <summary>Required idle and crying Baby color phases in schema order.</summary>
    private static readonly string[] paletteNames =
        ["Baby.Idle", "Baby.ClosedCry", "Baby.MiddleCry", "Baby.OpenCry"];

    /// <summary>Read-only view of eight required composition identities: three Baby frames, egg, then four cursor frames.</summary>
    public static ReadOnlySpan<string> SpriteNames => spriteNames;
    /// <summary>Read-only view of four required Baby palette identities in idle, closed-cry, middle-cry, and open-cry order.</summary>
    public static ReadOnlySpan<string> BabyPaletteNames => paletteNames;
}
