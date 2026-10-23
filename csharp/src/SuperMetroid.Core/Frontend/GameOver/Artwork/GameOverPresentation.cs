using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Immutable game-over artwork, static text layout, actor compositions, palettes and
/// screen anchors. Menu control, answer semantics, music and Baby sequence dispatch stay
/// compiled in the application.
/// </summary>
public sealed class GameOverPresentation
{
    /// <summary>Stores an authored tilemap only when it differs from the calculated stock game-over text page.</summary>
    internal readonly byte[]? tilemap;
    /// <summary>Each Baby frame's composition.</summary>
    internal readonly Dictionary<GameOverBabyFrame, SpriteComposition> babySprites;
    /// <summary>The egg's composition.</summary>
    internal readonly SpriteComposition eggSprite;
    /// <summary>The missile cursor's composition by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;
    /// <summary>Installed idle and crying color sets for the Baby's fixed CGRAM row.</summary>
    private readonly GameOverBabyColorCatalog babyPalettes;

    /// <summary>Compiles the sprite the document names <paramref name="name"/>.</summary>
    private static SpriteComposition CompileSprite(GameOverPresentationDocument document, string name) =>
        document.Sprites.TryGetValue(name, out SpriteVisualPart[]? parts) && parts is not null
            ? MenuSpriteCompiler.Compile(parts, $"game-over {name}")
            : throw new InvalidDataException($"Game-over presentation is missing sprite {name}.");

    /// <summary>Retains compiled artwork and screen anchors, eliding an unchanged stock tilemap.</summary>
    private GameOverPresentation(
        byte[] tilemap,
        Dictionary<GameOverBabyFrame, SpriteComposition> babySprites,
        SpriteComposition eggSprite,
        SpriteComposition[] cursorSprites,
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
        this.babySprites = babySprites;
        this.eggSprite = eggSprite;
        this.cursorSprites = cursorSprites;
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
    public Position16<OamPixels> BabyAnchor { get; }
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

    /// <summary>Copies the complete game-over tilemap into the menu's BG1 words, calculating stock text words when the loaded page has no edits.</summary>
    /// <param name="destination">The menu BG1 tilemap words.</param>
    internal void LoadTilemapTo(Span<ushort> destination)
    {
        if (tilemap is not null)
        {
            MenuTilemaps.Load(destination, tilemap);
            return;
        }
        Ensure.LengthEqual(destination, GameOverPresentationDefinitions.TilemapCellCount);
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            destination[cell] = GameOverPresentationDefinitions.TilemapWord(cell);
    }

    /// <summary>Appends the chosen Baby sprite composition at the installed anchor using its OBJ palette selector.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="frame">Closed, middle, or open Baby frame selected by the compiled animation sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not a supported Baby frame.</exception>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawBaby(ScreenSprites screenSprites, SpriteSheet sheet, GameOverBabyFrame frame) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationDefinitions.BabyFrameName(frame)), BabyAnchor);

    /// <summary>Appends the egg composition at the shared Baby anchor using the installed egg palette selector.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawEgg(ScreenSprites screenSprites, SpriteSheet sheet) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationDefinitions.EggFrame), BabyAnchor);

    /// <summary>Appends one missile-cursor frame at the installed Yes or No answer position.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="frame">Zero-based looping animation frame 0-3.</param>
    /// <param name="selectNo">True draws at the No position; false draws at the Yes position.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawCursor(ScreenSprites screenSprites, SpriteSheet sheet, int frame, bool selectNo)
    {
        if ((uint)frame >= GameOverPresentationDefinitions.CursorFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationDefinitions.CursorFrameName(frame)),
            new Position16<OamPixels>(checked((ushort)CursorX), checked((ushort)(selectNo ? NoCursorY : YesCursorY))));
    }

    /// <summary>Writes one selected sixteen-colour Baby animation palette over the Baby's sprite palette row.</summary>
    /// <param name="colours">The screen's colours.</param>
    /// <param name="palette">Idle or one of the three crying palettes selected by the compiled Baby sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public void ApplyBabyPalette(ScreenPalette colours, GameOverBabyPalette palette)
    {
        _ = GameOverPresentationDefinitions.BabyPaletteName(palette);
        ColourRun row = Ensure.NotNull(colours).SpriteRow(GameOverPresentationDefinitions.BabySpriteRow);
        for (int index = 0; index < row.Count; index++)
            row.Write(index, babyPalettes.Read(palette, index));
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
                bytes, ArtworkJson.Options) ??
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

        var babySprites = Enum.GetValues<GameOverBabyFrame>().ToDictionary(frame => frame,
            frame => GameOverSpriteParts.CalculateBabyIfMatching(frame, CompileSprite(document, GameOverPresentationDefinitions.BabyFrameName(frame))));
        SpriteComposition eggSprite = GameOverSpriteParts.CalculateEggIfMatching(CompileSprite(document, GameOverPresentationDefinitions.EggFrame));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame =>
            MenuCursorParts.CalculateIfMatching(frame, CompileSprite(document, GameOverPresentationDefinitions.CursorFrameName(frame))))];

        var palettes = new Dictionary<GameOverBabyPalette, Bgr555[]>();
        foreach (GameOverBabyPalette palette in Enum.GetValues<GameOverBabyPalette>())
        {
            string name = GameOverPresentationDefinitions.BabyPaletteName(palette);
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverRomData.BabyAnimation.PaletteColorCount ||
                colors.Any(color => !Bgr555.IsColorWord(color)))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(palette, Array.ConvertAll(colors, Bgr555.FromWord));
        }

        return new(tilemap, babySprites, eggSprite, cursorSprites, new GameOverBabyColorCatalog(palettes), document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document and validates it with the loader before writing any output bytes.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Caller-owned artwork and layout; its collections are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document fails game-over presentation validation.</exception>
    public static void Write(Stream output, GameOverPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Checks both coordinates of a game-over screen anchor fit the native OAM byte range.</summary>
    private static void ValidatePoint(Position16<OamPixels> point, string name)
    {
        ValidateCoordinate(point.X, $"{name} X");
        ValidateCoordinate(point.Y, $"{name} Y");
    }

    /// <summary>Rejects a screen coordinate outside the 0–255 range used by authored menu layout.</summary>
    private static void ValidateCoordinate(int value, string name)
    {
        if ((uint)value > byte.MaxValue)
            throw new InvalidDataException($"{name} must be 0..255.");
    }

    /// <summary>Rejects an OBJ palette selector outside the eight hardware rows.</summary>
    private static void ValidatePaletteIndex(int value, string name)
    {
        if ((uint)value > 7)
            throw new InvalidDataException($"{name} must be 0..7.");
    }
}
