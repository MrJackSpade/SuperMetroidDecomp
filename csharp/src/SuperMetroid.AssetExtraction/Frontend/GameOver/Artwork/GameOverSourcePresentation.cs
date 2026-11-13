using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// The game-over presentation as the import pipeline reads it (#1463, #1616, #1638), from the extracted artwork document in
/// the component's import data: the page's tiles the menus' picture of the page is composed from (#1467), the Baby, egg and
/// cursor compositions and palette selectors the game-over sheet is composed from, and the layout
/// (<see cref="Presentation"/>), which the installed layout holds for the game (<see cref="GameOverLayoutFile"/>).
/// </summary>
public sealed class GameOverSourcePresentation
{
    /// <summary>The page's tiles, row by row, which the menus' picture of the page is composed from (#1467).</summary>
    internal readonly BackgroundTile[] page;
    /// <summary>Each Baby frame's composition.</summary>
    internal readonly Dictionary<GameOverBabyFrame, SpriteComposition> babySprites;
    /// <summary>The egg's composition.</summary>
    internal readonly SpriteComposition eggSprite;
    /// <summary>The missile cursor's composition by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;

    /// <summary>Retains the compiled presentation and its compositions.</summary>
    private GameOverSourcePresentation(GameOverPresentationDocument document, GameOverPresentation presentation, BackgroundTile[] tilemap,
        GameOverBabyPaint babyPaint, GameOverSprites sprites, string contentIdentity)
    {
        Presentation = presentation;
        BabyPaint = babyPaint;
        ContentIdentity = contentIdentity;
        BabyPaletteIndex = document.BabyPalette;
        EggPaletteIndex = document.EggPalette;
        CursorPaletteIndex = document.CursorPalette;
        page = tilemap;
        babySprites = sprites.Baby;
        eggSprite = sprites.Egg;
        cursorSprites = sprites.Cursors;
    }

    /// <summary>What the game binds of the presentation: the Baby's and the cursor's anchors and the cursor's frame duration.</summary>
    public GameOverPresentation Presentation { get; }

    /// <summary>The Baby's idle and crying colours, which the import pre-colours the Baby's shades in (#1512).</summary>
    internal GameOverBabyPaint BabyPaint { get; }
    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Sprite palette selector 0-7 the Baby composition is composed in.</summary>
    public int BabyPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 the egg composition is composed in.</summary>
    public int EggPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 all four missile-cursor frames are composed in.</summary>
    public int CursorPaletteIndex { get; }

    /// <summary>Compiles the sprite the document names <paramref name="name"/>.</summary>
    private static SpriteComposition CompileSprite(GameOverPresentationDocument document, string name) =>
        document.Sprites.TryGetValue(name, out SpriteVisualPart[]? parts) && parts is not null
            ? MenuSpriteCompiler.Compile(parts, $"game-over {name}")
            : throw new InvalidDataException($"Game-over presentation is missing sprite {name}.");

    /// <summary>Reads and validates game-over JSON, compiling its tilemap, sprite compositions, palette artwork, and screen layout.</summary>
    /// <param name="source">Readable stream consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning parsed and compiled content independently of the source stream.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, required artwork, colors, coordinates, palette selectors, or cursor duration are invalid.</exception>
    public static GameOverSourcePresentation Load(Stream source)
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

        if (document.Version != GameOverPresentationFormat.Version)
            throw new InvalidDataException(
                $"Game-over presentation version must be {GameOverPresentationFormat.Version}.");
        if (document.Tilemap is null ||
            document.Tilemap.Length != GameOverPageDefinitions.TilemapCellCount)
            throw new InvalidDataException("Game-over presentation requires a complete 32x32 tilemap.");
        if (document.Sprites is null ||
            document.Sprites.Count != GameOverPresentationFormat.SpriteNames.Length)
            throw new InvalidDataException("Game-over presentation requires all eight named sprite compositions.");
        if (document.BabyPalettes is null ||
            document.BabyPalettes.Count != GameOverPresentationFormat.BabyPaletteNames.Length)
            throw new InvalidDataException("Game-over presentation requires all four named Baby palettes.");
        GameOverPresentation presentation = GameOverLayoutFile.Presentation(new GameOverPlacements
        {
            BabyAnchor = document.BabyAnchor,
            CursorX = document.CursorX,
            YesCursorY = document.YesCursorY,
            NoCursorY = document.NoCursorY,
            CursorFrameDuration = document.CursorFrameDuration,
        });
        ValidatePaletteIndex(document.BabyPalette, nameof(document.BabyPalette));
        ValidatePaletteIndex(document.EggPalette, nameof(document.EggPalette));
        ValidatePaletteIndex(document.CursorPalette, nameof(document.CursorPalette));

        var tilemap = new BackgroundTile[GameOverPageDefinitions.TilemapCellCount];
        for (int index = 0; index < document.Tilemap.Length; index++)
        {
            MapPresentationCell? cell = document.Tilemap[index];
            if (cell is null ||
                (uint)cell.TileColumn >= MapTileAtlasFormat.TileColumns ||
                (uint)cell.TileRow >= MapPresentationFormat.AtlasRows ||
                (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException(
                    $"Game-over tilemap cell {index} has an invalid atlas coordinate or palette.");
            tilemap[index] = new BackgroundTile(cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn, cell.Palette,
                cell.FlipX, cell.FlipY, cell.Priority);
        }

        var babySprites = Enum.GetValues<GameOverBabyFrame>().ToDictionary(frame => frame,
            frame => CompileSprite(document, GameOverPresentationNames.BabyFrameName(frame)));
        SpriteComposition eggSprite = CompileSprite(document, GameOverPresentationDefinitions.EggFrame);
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame =>
            CompileSprite(document, GameOverPresentationNames.CursorFrameName(frame)))];

        var palettes = new Dictionary<GameOverBabyPalette, RgbColor[]>();
        foreach (GameOverBabyPalette palette in Enum.GetValues<GameOverBabyPalette>())
        {
            string name = GameOverBabyPaletteNames.BabyPaletteName(palette);
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverPresentationFormat.BabyPaletteColorCount ||
                colors.Any(color => !RgbColor.IsColorWord(color)))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(palette, Array.ConvertAll(colors, RgbColor.FromWord));
        }

        return new(document, presentation, tilemap, new GameOverBabyPaint(palettes), new(babySprites, eggSprite, cursorSprites),
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Caller-owned artwork and layout; its collections are not modified.</param>
    public static void Write(Stream output, GameOverPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

    /// <summary>The menus' picture of the page: every cell of the page, named for the game to place (#1467).</summary>
    internal ScreenPictureSubject PageSubject() =>
        ScreenPictureSubject.Page(GameOverPresentationDefinitions.PageFrame, page, GameOverPageDefinitions.TilemapWidth);

    /// <summary>Rejects an OBJ palette selector outside the eight hardware rows.</summary>
    private static void ValidatePaletteIndex(int value, string name)
    {
        if ((uint)value > 7)
            throw new InvalidDataException($"{name} must be 0..7.");
    }

    /// <summary>The compiled Baby frames, egg and cursor frames.</summary>
    /// <param name="Baby">Each Baby frame's composition.</param>
    /// <param name="Egg">The egg's composition.</param>
    /// <param name="Cursors">The missile cursor's composition by animation frame.</param>
    private sealed record GameOverSprites(Dictionary<GameOverBabyFrame, SpriteComposition> Baby, SpriteComposition Egg,
        SpriteComposition[] Cursors);
}
