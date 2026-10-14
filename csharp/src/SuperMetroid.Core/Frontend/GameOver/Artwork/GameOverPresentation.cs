using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Map.Artwork;

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
    /// <summary>Compiled Baby, egg, and missile-cursor compositions indexed by schema frame identity.</summary>
    internal readonly Dictionary<string, SpriteComposition> sprites;
    /// <summary>Installed idle and crying color sets for the Baby's fixed CGRAM row.</summary>
    private readonly GameOverBabyColorCatalog babyPalettes;

    /// <summary>Retains compiled artwork and screen anchors, eliding an unchanged stock tilemap.</summary>
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

    /// <summary>Copies the complete game-over tilemap into VRAM, calculating stock text words when the loaded page has no edits.</summary>
    /// <param name="destination">The menu BG1 tilemap receiving the 2048-byte transfer.</param>
    public void LoadTilemapTo(VramRegion destination)
    {
        if (tilemap is not null)
        {
            destination.Load(tilemap);
            return;
        }
        Span<byte> transfer = stackalloc byte[GameOverPresentationDefinitions.TilemapByteCount];
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(transfer.Slice(cell * sizeof(ushort)),
                GameOverPresentationDefinitions.TilemapWord(cell));
        destination.Load(transfer);
    }

    /// <summary>Appends the chosen Baby sprite composition at the installed anchor using its OBJ palette selector.</summary>
    /// <param name="oam">Current frame's mutable object buffer; this call does not finalize it.</param>
    /// <param name="frame">Closed, middle, or open Baby frame selected by the compiled animation sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not a supported Baby frame.</exception>
    public void DrawBaby(OamBuffer oam, GameOverBabyFrame frame) =>
        sprites[GameOverPresentationDefinitions.BabyFrameName(frame)].DrawOnScreen(
            oam, BabyAnchor,
            PaletteBits(BabyPaletteIndex));

    /// <summary>Appends the egg composition at the shared Baby anchor using the installed egg palette selector.</summary>
    /// <param name="oam">Current frame's mutable object buffer; this call does not finalize it.</param>
    public void DrawEgg(OamBuffer oam) =>
        sprites[GameOverPresentationDefinitions.EggFrame].DrawOnScreen(
            oam, BabyAnchor,
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
            oam, new Position16<OamPixels>(checked((ushort)CursorX), checked((ushort)(selectNo ? NoCursorY : YesCursorY))),
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
            new GameOverPalettes(cgram).Baby.Write(index,
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

        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in GameOverPresentationDefinitions.SpriteNames)
        {
            if (!document.Sprites.TryGetValue(name, out SpriteVisualPart[]? parts) || parts is null)
                throw new InvalidDataException($"Game-over presentation is missing sprite {name}.");
            var compiled = MenuSpriteCompiler.Compile(parts, $"game-over {name}");
            compiled = GameOverSpriteParts.CalculateIfMatching(name, compiled);
            sprites.Add(name, MenuCursorParts.CalculateIfMatching(name, compiled));
        }

        var palettes = new Dictionary<string, Bgr555[]>(StringComparer.Ordinal);
        foreach (string name in GameOverPresentationDefinitions.BabyPaletteNames)
        {
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverRomData.BabyAnimation.PaletteColorCount ||
                colors.Any(color => color > 0x7fff))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(name, Array.ConvertAll(colors, Bgr555.FromWord));
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
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Encodes an installed OBJ palette selector into the packed sprite attribute bits.</summary>
    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;

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
