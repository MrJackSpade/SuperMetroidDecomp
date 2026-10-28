namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>Schema names and geometry for <c>game-over.json</c>.</summary>
public static class GameOverPresentationDefinitions
{
    /// <summary>Supported game-over presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for game-over artwork, palette phases, and layout.</summary>
    public const string FileName = "game-over.json";
    /// <summary>Width of the game-over page in tile cells.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Height of the complete game-over page in tile cells, including rows outside the visible viewport.</summary>
    public const int TilemapHeight = 32;
    /// <summary>1024 row-major background tile cells in the complete 32-by-32 game-over page.</summary>
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
    /// <summary>The menu atlas cell of the blank glyph, shown by every page cell no text element writes.</summary>
    public const ushort BlankTile = 15;
    /// <summary>The sprite row the Baby's animated colours change.</summary>
    public const int BabySpriteRow = 4;
    /// <summary>Four frames in the looping missile-cursor animation.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>Schema sprite identity of the egg composition, drawn at the Baby anchor.</summary>
    public const string EggFrame = "Egg";

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

    /// <summary>$81:9304 Tilemap_GameOver_findTheMetroidLarva, the native one-row objective.</summary>
    internal const string ObjectiveText = "FIND THE METROID LARVA!";
    /// <summary>$81:9334 Tilemap_GameOver_tryAgain includes a space before its question mark.</summary>
    internal const string PromptText = "TRY AGAIN ?";
    /// <summary>$81:937C-939C small-font continuation below the YES answer.</summary>
    internal const string ReturnToGameText = " (RETURN TO GAME)";
    /// <summary>$81:93CA-93E4 small-font continuation below the N O answer.</summary>
    internal const string ReturnToTitleText = " (GO TO TITLE)";
    /// <summary>$81:9304-93E4 small uppercase menu atlas: A starts at $6A and letters are consecutive.</summary>
    internal const int SmallLetterA = 0x6a;
    /// <summary>$81:9330, Tilemap_GameOver_findTheMetroidLarva punctuation.</summary>
    internal const int ExclamationTile = 0x84;
    /// <summary>$81:9348, Tilemap_GameOver_tryAgain punctuation.</summary>
    internal const int QuestionTile = 0x85;
    /// <summary>$81:937E/$939C, answer explanation opening/closing glyphs occupy adjacent atlas cells.</summary>
    internal const int OpeningParenthesisTile = 0x8a;

    /// <summary>$81:92DC/92F0 and $934C/9376/$93A0/93C4 select large-font halves.
    /// A/E/M/N/S use the atlas row stride; G/R/V/Y reuse distinct halves; O uses the round zero glyph.</summary>
    internal static readonly IReadOnlyDictionary<char, GameOverLargeGlyph> LargeLetters = new Dictionary<char, GameOverLargeGlyph>
    {
        [' '] = new(BlankTile, BlankTile),
        ['A'] = new(0x0a, 0x0a + 16),
        ['E'] = new(0x0e, 0x0e + 16),
        ['M'] = new(0x26, 0x26 + 16),
        ['N'] = new(0x27, 0x27 + 16),
        ['S'] = new(0x2b, 0x2b + 16),
        ['O'] = new(0, 16),
        ['G'] = new(0x0c, 0x30),
        ['R'] = new(0x0d, 0x3a),
        ['V'] = new(0x2d, 0x3e),
        ['Y'] = new(0x41, 0x17),
    };
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

    /// <summary>$82:CFF6: the closed Baby's sixteen-pixel cell.</summary>
    internal const int ClosedBabyCell = 0x90;

    /// <summary>$82:D004: the open Baby's sixteen-pixel cell.</summary>
    internal const int OpenBabyCell = 0x9b;

    /// <summary>$82:CFE0: the eight-pixel container-cap cell, reflected to form all four caps.</summary>
    internal const int ContainerCapCell = 0x9a;
}
