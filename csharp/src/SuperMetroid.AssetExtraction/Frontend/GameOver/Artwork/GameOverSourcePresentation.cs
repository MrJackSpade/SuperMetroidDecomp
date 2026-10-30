using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// The installed game-over presentation as the import pipeline reads it (#1463): beside the palettes and anchors the game
/// binds, the page's tiles the menus' picture of the page is composed from (#1467) and the Baby, egg and cursor
/// compositions the game-over sheet is composed from.
/// </summary>
public sealed class GameOverSourcePresentation : GameOverPresentation
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
    private GameOverSourcePresentation(GameOverPresentationDocument document, BackgroundTile[] tilemap, GameOverBabyPaint babyPalettes,
        GameOverSprites sprites, string contentIdentity) : base(document, babyPalettes, contentIdentity)
    {
        page = tilemap;
        babySprites = sprites.Baby;
        eggSprite = sprites.Egg;
        cursorSprites = sprites.Cursors;
    }

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

        if (document.Version != GameOverPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"Game-over presentation version must be {GameOverPresentationDefinitions.Version}.");
        if (document.Tilemap is null ||
            document.Tilemap.Length != GameOverPageDefinitions.TilemapCellCount)
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
            frame => GameOverSpriteParts.CalculateBabyIfMatching(frame, CompileSprite(document, GameOverPresentationNames.BabyFrameName(frame))));
        SpriteComposition eggSprite = GameOverSpriteParts.CalculateEggIfMatching(CompileSprite(document, GameOverPresentationDefinitions.EggFrame));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame =>
            MenuCursorParts.CalculateIfMatching(frame, CompileSprite(document, GameOverPresentationNames.CursorFrameName(frame))))];

        var palettes = new Dictionary<GameOverBabyPalette, RgbColor[]>();
        foreach (GameOverBabyPalette palette in Enum.GetValues<GameOverBabyPalette>())
        {
            string name = GameOverPresentationNames.BabyPaletteName(palette);
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverMenuDefinitions.BabyPaletteColorCount ||
                colors.Any(color => !RgbColor.IsColorWord(color)))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(palette, Array.ConvertAll(colors, RgbColor.FromWord));
        }

        return new(document, tilemap, new GameOverBabyPaint(palettes), new(babySprites, eggSprite, cursorSprites),
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

    /// <summary>The menus' picture of the page: every cell of the page, named for the game to place (#1467).</summary>
    internal ScreenPictureSubject PageSubject() =>
        ScreenPictureSubject.Page(GameOverPresentationDefinitions.PageFrame, page, GameOverPageDefinitions.TilemapWidth);

    /// <summary>The import-side presentation <paramref name="gameOver"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The game-over presentation was not read from installed artwork.</exception>
    public static GameOverSourcePresentation Of(GameOverPresentation gameOver) => gameOver as GameOverSourcePresentation
        ?? throw new InvalidDataException("The game-over presentation was not read from installed artwork.");

    /// <summary>Checks both coordinates of a game-over screen anchor fit the native OAM byte range.</summary>
    private static void ValidatePoint(Position16<SpritePixels> point, string name)
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

    /// <summary>The compiled Baby frames, egg and cursor frames.</summary>
    /// <param name="Baby">Each Baby frame's composition.</param>
    /// <param name="Egg">The egg's composition.</param>
    /// <param name="Cursors">The missile cursor's composition by animation frame.</param>
    private sealed record GameOverSprites(Dictionary<GameOverBabyFrame, SpriteComposition> Baby, SpriteComposition Egg,
        SpriteComposition[] Cursors);
}
