namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>Schema names and geometry for <c>game-over.json</c>.</summary>
public static class GameOverPresentationDefinitions
{
    /// <summary>Supported game-over presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for game-over artwork, palette phases, and layout.</summary>
    public const string FileName = "game-over.json";
    /// <summary>Four frames in the looping missile-cursor animation.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>Schema sprite identity of the egg composition, drawn at the Baby anchor.</summary>
    public const string EggFrame = "Egg";
    /// <summary>The menus' picture of the whole game-over page: its title, objective, prompt and both answers (#1467).</summary>
    public const string PageFrame = "GameOver.Page";

    /// <summary>Required Baby, egg, and cursor frame identities in schema order.</summary>
    private static readonly string[] spriteNames =
    [
        "Baby.Closed", "Baby.Middle", "Baby.Open", EggFrame,
        "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
    ];

    /// <summary>Required idle and crying Baby color phases in schema order.</summary>
    private static readonly string[] paletteNames =
        ["Baby.Idle", "Baby.ClosedCry", "Baby.MiddleCry", "Baby.OpenCry"];

    /// <summary>Read-only view of eight required composition identities: three Baby frames, egg, then four cursor frames.</summary>
    public static ReadOnlySpan<string> SpriteNames => spriteNames;
    /// <summary>Read-only view of four required Baby palette identities in idle, closed-cry, middle-cry, and open-cry order.</summary>
    public static ReadOnlySpan<string> BabyPaletteNames => paletteNames;

    /// <summary>Maps a compiled Baby animation frame to its authored sprite composition identity.</summary>
    internal static readonly IReadOnlyDictionary<GameOverBabyFrame, string> BabyFrameNames =
        new Dictionary<GameOverBabyFrame, string>
        {
            [GameOverBabyFrame.Closed] = "Baby.Closed",
            [GameOverBabyFrame.Middle] = "Baby.Middle",
            [GameOverBabyFrame.Open] = "Baby.Open",
        };

    /// <summary>Returns the authored composition identity for a missile-cursor frame.</summary>
    internal static readonly IReadOnlyDictionary<int, string> CursorFrameNames =
        new Dictionary<int, string>
        {
            [0] = "Cursor.0",
            [1] = "Cursor.1",
            [2] = "Cursor.2",
            [3] = "Cursor.3",
        };

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    internal static readonly IReadOnlyDictionary<GameOverBabyPalette, string> BabyPaletteNamesByPhase =
        new Dictionary<GameOverBabyPalette, string>
        {
            [GameOverBabyPalette.Idle] = "Baby.Idle",
            [GameOverBabyPalette.ClosedCry] = "Baby.ClosedCry",
            [GameOverBabyPalette.MiddleCry] = "Baby.MiddleCry",
            [GameOverBabyPalette.OpenCry] = "Baby.OpenCry",
        };
}
