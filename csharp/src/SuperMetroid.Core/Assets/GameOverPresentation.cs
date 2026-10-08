using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable game-over artwork, static text layout, actor compositions, palettes and
/// screen anchors. Menu control, answer semantics, music and Baby sequence dispatch stay
/// compiled in the application.
/// </summary>
public sealed class GameOverPresentation
{
    private readonly byte[]? tilemap;
    private readonly Dictionary<string, SpriteComposition> sprites;
    private readonly GameOverBabyColorCatalog babyPalettes;

    private GameOverPresentation(
        byte[] tilemap,
        Dictionary<string, SpriteComposition> sprites,
        GameOverBabyColorCatalog babyPalettes,
        GameOverPresentationDocument document,
        string contentIdentity)
    {
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            if (BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(cell * sizeof(ushort))) !=
                GameOverPresentationDefinitions.TilemapWord(cell))
            {
                this.tilemap = tilemap;
                break;
            }
        this.sprites = sprites;
        this.babyPalettes = babyPalettes;
        BabyAnchor = document.BabyAnchor;
        CursorX = document.CursorX;
        YesCursorY = document.YesCursorY;
        NoCursorY = document.NoCursorY;
        BabyPaletteIndex = document.BabyPalette;
        EggPaletteIndex = document.EggPalette;
        CursorPaletteIndex = document.CursorPalette;
        CursorFrameDuration = document.CursorFrameDuration;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Shared drawing origin of the Baby and egg compositions in screen pixels, with both coordinates in 0-255.</summary>
    public MapLabelPoint BabyAnchor { get; }
    /// <summary>Missile-cursor horizontal screen coordinate in pixels, shared by both answers and bounded to 0-255.</summary>
    public int CursorX { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the Yes answer, bounded to 0-255.</summary>
    public int YesCursorY { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the No answer, bounded to 0-255.</summary>
    public int NoCursorY { get; }
    /// <summary>OBJ palette selector 0-7 used when drawing the Baby composition; palette animation writes to its fixed native CGRAM destination.</summary>
    public int BabyPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 used when drawing the egg at the shared Baby anchor.</summary>
    public int EggPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 used by all four missile-cursor frames.</summary>
    public int CursorPaletteIndex { get; }
    /// <summary>Menu updates per frame of the looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Copies the complete game-over tilemap into VRAM, calculating stock text words when the loaded page has no edits.</summary>
    /// <param name="vram">Mutable destination video memory.</param>
    /// <param name="destinationWord">Starting VRAM word address, converted to a byte offset for the 2048-byte transfer.</param>
    public void LoadTilemapTo(SnesVram vram, int destinationWord)
    {
        if (tilemap is not null)
        {
            vram.LoadBytes(destinationWord * sizeof(ushort), tilemap);
            return;
        }
        Span<byte> transfer = stackalloc byte[GameOverPresentationDefinitions.TilemapByteCount];
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(transfer.Slice(cell * sizeof(ushort)),
                GameOverPresentationDefinitions.TilemapWord(cell));
        vram.LoadBytes(destinationWord * sizeof(ushort), transfer);
    }

    /// <summary>Appends the chosen Baby sprite composition at the installed anchor using its OBJ palette selector.</summary>
    /// <param name="oam">Current frame's mutable object buffer; this call does not finalize it.</param>
    /// <param name="frame">Closed, middle, or open Baby frame selected by the compiled animation sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not a supported Baby frame.</exception>
    public void DrawBaby(OamBuffer oam, GameOverBabyFrame frame) =>
        sprites[GameOverPresentationDefinitions.BabyFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)BabyAnchor.X), checked((ushort)BabyAnchor.Y),
            PaletteBits(BabyPaletteIndex));

    /// <summary>Appends the egg composition at the shared Baby anchor using the installed egg palette selector.</summary>
    /// <param name="oam">Current frame's mutable object buffer; this call does not finalize it.</param>
    public void DrawEgg(OamBuffer oam) =>
        sprites[GameOverPresentationDefinitions.EggFrame].DrawOnScreen(
            oam, checked((ushort)BabyAnchor.X), checked((ushort)BabyAnchor.Y),
            PaletteBits(EggPaletteIndex));

    /// <summary>Appends one missile-cursor frame at the installed Yes or No answer position.</summary>
    /// <param name="oam">Current frame's mutable object buffer; this call does not finalize it.</param>
    /// <param name="frame">Zero-based looping animation frame 0-3.</param>
    /// <param name="selectNo">True draws at the No position; false draws at the Yes position.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public void DrawCursor(OamBuffer oam, int frame, bool selectNo)
    {
        if ((uint)frame >= GameOverPresentationDefinitions.CursorFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        sprites[GameOverPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)CursorX),
            checked((ushort)(selectNo ? NoCursorY : YesCursorY)),
            PaletteBits(CursorPaletteIndex));
    }

    /// <summary>Writes one selected sixteen-color Baby animation palette to the fixed native CGRAM range $C0-$CF.</summary>
    /// <param name="cgram">Mutable destination color memory.</param>
    /// <param name="palette">Idle or one of the three crying palettes selected by the compiled Baby sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public void ApplyBabyPalette(SnesCgram cgram, GameOverBabyPalette palette)
    {
        _ = GameOverPresentationDefinitions.BabyPaletteName(palette);
        for (int index = 0; index < GameOverRomData.BabyAnimation.PaletteColorCount; index++)
            cgram.SetColor(GameOverRomData.BabyAnimation.PaletteDestinationIndex + index,
                babyPalettes.Read(palette, index));
    }

    /// <summary>Reads and validates game-over JSON, compiling its tilemap, sprite compositions, palette artwork, and screen layout.</summary>
    /// <param name="source">Readable stream consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning parsed and compiled content independently of the source stream.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, required artwork, colors, coordinates, palette selectors, or cursor duration are invalid.</exception>
    public static GameOverPresentation Load(Stream source)
    {
        byte[] bytes;
        using (var copy = new MemoryStream())
        {
            source.CopyTo(copy);
            bytes = copy.ToArray();
        }

        GameOverPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameOverPresentationDocument>(
                bytes, MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Game-over presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid game-over presentation JSON.", error);
        }

        if (document.Version != GameOverPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"Game-over presentation version must be {GameOverPresentationDefinitions.Version}.");
        if (document.Tilemap is null ||
            document.Tilemap.Length != GameOverPresentationDefinitions.TilemapCellCount)
            throw new InvalidDataException("Game-over presentation requires a complete 32x32 tilemap.");
        if (document.Sprites is null ||
            document.Sprites.Count != GameOverPresentationDefinitions.SpriteNames.Length)
            throw new InvalidDataException("Game-over presentation requires all eight named sprite compositions.");
        if (document.BabyPalettes is null ||
            document.BabyPalettes.Count != GameOverPresentationDefinitions.BabyPaletteNames.Length)
            throw new InvalidDataException("Game-over presentation requires all four named Baby palettes.");
        ValidatePoint(document.BabyAnchor, "Baby anchor");
        ValidateCoordinate(document.CursorX, nameof(document.CursorX));
        ValidateCoordinate(document.YesCursorY, nameof(document.YesCursorY));
        ValidateCoordinate(document.NoCursorY, nameof(document.NoCursorY));
        ValidatePaletteIndex(document.BabyPalette, nameof(document.BabyPalette));
        ValidatePaletteIndex(document.EggPalette, nameof(document.EggPalette));
        ValidatePaletteIndex(document.CursorPalette, nameof(document.CursorPalette));
        if (document.CursorFrameDuration is < 1 or > ushort.MaxValue)
            throw new InvalidDataException("Game-over cursor frame duration must be 1..65535.");

        var tilemap = new byte[GameOverPresentationDefinitions.TilemapByteCount];
        for (int index = 0; index < document.Tilemap.Length; index++)
        {
            MapPresentationCell? cell = document.Tilemap[index];
            if (cell is null ||
                (uint)cell.TileColumn >= MapTileAtlasFormat.TileColumns ||
                (uint)cell.TileRow >= MapPresentationFormat.AtlasRows ||
                (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException(
                    $"Game-over tilemap cell {index} has an invalid atlas coordinate or palette.");
            ushort word = checked((ushort)(
                cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn |
                cell.Palette << MapPresentationFormat.PaletteShift |
                (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
                (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.AsSpan(index * sizeof(ushort)), word);
        }

        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in GameOverPresentationDefinitions.SpriteNames)
        {
            if (!document.Sprites.TryGetValue(name, out SpriteVisualPart[]? parts) || parts is null)
                throw new InvalidDataException($"Game-over presentation is missing sprite {name}.");
            var compiled = MenuSpriteCompiler.Compile(parts, $"game-over {name}");
            compiled = GameOverSpriteParts.CalculateIfMatching(name, compiled);
            sprites.Add(name, MenuCursorParts.CalculateIfMatching(name, compiled));
        }

        var palettes = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (string name in GameOverPresentationDefinitions.BabyPaletteNames)
        {
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverRomData.BabyAnimation.PaletteColorCount ||
                colors.Any(color => color > 0x7fff))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(name, colors);
        }

        return new(tilemap, sprites, new GameOverBabyColorCatalog(palettes), document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document and validates it with the loader before writing any output bytes.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Caller-owned artwork and layout; its collections are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document fails game-over presentation validation.</exception>
    public static void Write(Stream output, GameOverPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;

    private static void ValidatePoint(MapLabelPoint? point, string name)
    {
        if (point is null)
            throw new InvalidDataException($"{name} is required.");
        ValidateCoordinate(point.X, $"{name} X");
        ValidateCoordinate(point.Y, $"{name} Y");
    }

    private static void ValidateCoordinate(int value, string name)
    {
        if ((uint)value > byte.MaxValue)
            throw new InvalidDataException($"{name} must be 0..255.");
    }

    private static void ValidatePaletteIndex(int value, string name)
    {
        if ((uint)value > 7)
            throw new InvalidDataException($"{name} must be 0..7.");
    }
}

/// <summary>Authored game-over JSON with background cells, actor compositions, Baby palette phases, and screen layout.</summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries; the stream loader parses its own independent document.</remarks>
public sealed record GameOverPresentationDocument
{
    /// <summary>Schema version that must equal <see cref="GameOverPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>1024 row-major cells composing the complete 32-by-32 game-over background tilemap.</summary>
    public required MapPresentationCell[] Tilemap { get; init; }
    /// <summary>Exactly eight named sprite compositions: three Baby frames, the egg, and four missile-cursor frames.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>Four named palette phases, each holding sixteen packed SNES BGR555 words in the range $0000-$7FFF.</summary>
    public required Dictionary<string, ushort[]> BabyPalettes { get; init; }
    /// <summary>Shared Baby and egg drawing origin in screen pixels, X=0-255 and Y=0-255.</summary>
    public required MapLabelPoint BabyAnchor { get; init; }
    /// <summary>Common horizontal missile-cursor position in screen pixels, 0-255.</summary>
    public required int CursorX { get; init; }
    /// <summary>Vertical missile-cursor position for the Yes answer in screen pixels, 0-255.</summary>
    public required int YesCursorY { get; init; }
    /// <summary>Vertical missile-cursor position for the No answer in screen pixels, 0-255.</summary>
    public required int NoCursorY { get; init; }
    /// <summary>OBJ palette selector 0-7 for Baby sprites; does not change the fixed destination of Baby palette animation writes.</summary>
    public required int BabyPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for the egg composition.</summary>
    public required int EggPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for missile-cursor compositions.</summary>
    public required int CursorPalette { get; init; }
    /// <summary>Duration 1-65535 in menu updates for each looping missile-cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}

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

    private static readonly string[] spriteNames =
    [
        "Baby.Closed", "Baby.Middle", "Baby.Open", EggFrame,
        "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
    ];

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
}
