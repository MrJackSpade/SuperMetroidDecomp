namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>Schema names and geometry for <c>game-over.json</c>.</summary>
public static class GameOverPresentationDefinitions
{
    /// <summary>Supported game-over presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for game-over artwork, palette phases, and layout.</summary>
    public const string FileName = "game-over.json";
    /// <summary>1024 row-major background tile cells in the complete 32-by-32 game-over page.</summary>
    public const int TilemapCellCount = GameOverRomData.TilemapWidth * GameOverRomData.TilemapHeight;
    /// <summary>2048 bytes required to transfer the page's sixteen-bit tilemap words.</summary>
    public const int TilemapByteCount = TilemapCellCount * sizeof(ushort);
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
    private const string ObjectiveText = "FIND THE METROID LARVA!";
    /// <summary>$81:9334 Tilemap_GameOver_tryAgain includes a space before its question mark.</summary>
    private const string PromptText = "TRY AGAIN ?";
    /// <summary>$81:937C-939C small-font continuation below the YES answer.</summary>
    private const string ReturnToGameText = " (RETURN TO GAME)";
    /// <summary>$81:93CA-93E4 small-font continuation below the N O answer.</summary>
    private const string ReturnToTitleText = " (GO TO TITLE)";
    /// <summary>$81:9304-93E4 small uppercase menu atlas: A starts at $6A and letters are consecutive.</summary>
    private const int SmallLetterA = 0x6a;
    /// <summary>$81:9330, Tilemap_GameOver_findTheMetroidLarva punctuation.</summary>
    private const int ExclamationTile = 0x84;
    /// <summary>$81:9348, Tilemap_GameOver_tryAgain punctuation.</summary>
    private const int QuestionTile = 0x85;
    /// <summary>$81:937E/$939C, answer explanation opening/closing glyphs occupy adjacent atlas cells.</summary>
    private const int OpeningParenthesisTile = 0x8a;

    /// <summary>Blank page plus the five native game-over text elements; no stored tilemap rows.</summary>
    internal static ushort TilemapWord(int cell)
    {
        if ((uint)cell >= TilemapCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        for (int index = 0; index < GameOverRomData.Text.Count; index++)
        {
            var element = (GameOverTextElement)index;
            GameOverTextStream stream = GameOverRomData.Text.Get(element);
            int origin = stream.DestinationByteOffset / sizeof(ushort);
            int column = cell % GameOverRomData.TilemapWidth - origin % GameOverRomData.TilemapWidth;
            int row = cell / GameOverRomData.TilemapWidth - origin / GameOverRomData.TilemapWidth;
            if (column < 0 || row is < 0 or > 1) continue;
            switch (element)
            {
                case GameOverTextElement.Title:
                    if (column < stream.Description.Length) return LargeLetter(stream.Description[column], row);
                    break;
                case GameOverTextElement.Objective:
                    if (row == 0 && column < ObjectiveText.Length) return SmallLetter(ObjectiveText[column]);
                    break;
                case GameOverTextElement.Prompt:
                    if (row == 0 && column < PromptText.Length) return SmallLetter(PromptText[column]);
                    break;
                case GameOverTextElement.ReturnToGame:
                case GameOverTextElement.ReturnToTitle:
                    string answer = element == GameOverTextElement.ReturnToGame ? "YES" : "N O";
                    if (column < answer.Length) return LargeLetter(answer[column], row);
                    string explanation = element == GameOverTextElement.ReturnToGame ? ReturnToGameText : ReturnToTitleText;
                    if (row == 1 && column - answer.Length < explanation.Length)
                        return SmallLetter(explanation[column - answer.Length]);
                    break;
            }
        }
        return GameOverRomData.BlankTile.Raw;
    }

    /// <summary>Maps a character in the native small-font text lines to its tile word.</summary>
    private static ushort SmallLetter(char letter) => letter switch
    {
        >= 'A' and <= 'Z' => (ushort)(SmallLetterA + letter - 'A'),
        ' ' => GameOverRomData.BlankTile.Raw,
        '!' => ExclamationTile,
        '?' => QuestionTile,
        '(' => OpeningParenthesisTile,
        ')' => OpeningParenthesisTile + 1,
        _ => throw new ArgumentOutOfRangeException(nameof(letter)),
    };

    /// <summary>$81:92DC/92F0 and $934C/9376/$93A0/93C4 select large-font halves.
    /// A/E/M/N/S use the atlas row stride; G/R/V/Y reuse distinct halves; O uses the round zero glyph.</summary>
    private static ushort LargeLetter(char letter, int row) => letter switch
    {
        ' ' => GameOverRomData.BlankTile.Raw,
        'A' => (ushort)(0x0a + 16 * row),
        'E' => (ushort)(0x0e + 16 * row),
        'M' => (ushort)(0x26 + 16 * row),
        'N' => (ushort)(0x27 + 16 * row),
        'S' => (ushort)(0x2b + 16 * row),
        'O' => (ushort)(16 * row),
        'G' => (ushort)(row == 0 ? 0x0c : 0x30),
        'R' => (ushort)(row == 0 ? 0x0d : 0x3a),
        'V' => (ushort)(row == 0 ? 0x2d : 0x3e),
        'Y' => (ushort)(row == 0 ? 0x41 : 0x17),
        _ => throw new ArgumentOutOfRangeException(nameof(letter)),
    };
    /// <summary>Maps a compiled Baby animation frame to its authored sprite composition identity.</summary>
    /// <param name="frame">Closed, middle, or open mouth frame.</param>
    /// <returns>The corresponding <c>Baby.</c> sprite dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is unsupported.</exception>
    public static string BabyFrameName(GameOverBabyFrame frame) => frame switch
    {
        GameOverBabyFrame.Closed => "Baby.Closed",
        GameOverBabyFrame.Middle => "Baby.Middle",
        GameOverBabyFrame.Open => "Baby.Open",
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Returns the authored composition identity for a missile-cursor frame.</summary>
    /// <param name="frame">Zero-based animation frame 0-3.</param>
    /// <returns><c>Cursor.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public static string CursorFrameName(int frame) => frame switch
    {
        0 => "Cursor.0",
        1 => "Cursor.1",
        2 => "Cursor.2",
        3 => "Cursor.3",
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    /// <param name="palette">Idle, closed-cry, middle-cry, or open-cry palette phase.</param>
    /// <returns>The corresponding <c>Baby.</c> palette dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public static string BabyPaletteName(GameOverBabyPalette palette) => palette switch
    {
        GameOverBabyPalette.Idle => "Baby.Idle",
        GameOverBabyPalette.ClosedCry => "Baby.ClosedCry",
        GameOverBabyPalette.MiddleCry => "Baby.MiddleCry",
        GameOverBabyPalette.OpenCry => "Baby.OpenCry",
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    /// <summary>$82:CFF6: the closed Baby's sixteen-pixel cell.</summary>
    internal const int ClosedBabyCell = 0x90;

    /// <summary>$82:D004: the open Baby's sixteen-pixel cell.</summary>
    internal const int OpenBabyCell = 0x9b;

    /// <summary>$82:CFE0: the eight-pixel container-cap cell, reflected to form all four caps.</summary>
    internal const int ContainerCapCell = 0x9a;
}
