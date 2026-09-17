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
    private readonly byte[] tilemap;
    private readonly Dictionary<string, SpriteComposition> sprites;
    private readonly Dictionary<string, ushort[]> babyPalettes;

    private GameOverPresentation(
        byte[] tilemap,
        Dictionary<string, SpriteComposition> sprites,
        Dictionary<string, ushort[]> babyPalettes,
        GameOverPresentationDocument document,
        string contentIdentity)
    {
        this.tilemap = tilemap;
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

    public string ContentIdentity { get; }
    public MapLabelPoint BabyAnchor { get; }
    public int CursorX { get; }
    public int YesCursorY { get; }
    public int NoCursorY { get; }
    public int BabyPaletteIndex { get; }
    public int EggPaletteIndex { get; }
    public int CursorPaletteIndex { get; }
    public int CursorFrameDuration { get; }

    public void LoadTilemapTo(SnesVram vram, int destinationWord) =>
        vram.LoadBytes(destinationWord * sizeof(ushort), tilemap);

    public void DrawBaby(OamBuffer oam, GameOverBabyFrame frame) =>
        sprites[GameOverPresentationDefinitions.BabyFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)BabyAnchor.X), checked((ushort)BabyAnchor.Y),
            PaletteBits(BabyPaletteIndex));

    public void DrawEgg(OamBuffer oam) =>
        sprites[GameOverPresentationDefinitions.EggFrame].DrawOnScreen(
            oam, checked((ushort)BabyAnchor.X), checked((ushort)BabyAnchor.Y),
            PaletteBits(EggPaletteIndex));

    public void DrawCursor(OamBuffer oam, int frame, bool selectNo)
    {
        if ((uint)frame >= GameOverPresentationDefinitions.CursorFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        sprites[GameOverPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)CursorX),
            checked((ushort)(selectNo ? NoCursorY : YesCursorY)),
            PaletteBits(CursorPaletteIndex));
    }

    public void ApplyBabyPalette(SnesCgram cgram, GameOverBabyPalette palette)
    {
        ushort[] colors = babyPalettes[GameOverPresentationDefinitions.BabyPaletteName(palette)];
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(GameOverRomData.BabyAnimation.PaletteDestinationIndex + index, colors[index]);
    }

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
            document = JsonSerializer.Deserialize<GameOverPresentationDocument>(
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
            sprites.Add(name, MenuSpriteCompiler.Compile(parts, $"game-over {name}"));
        }

        var palettes = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (string name in GameOverPresentationDefinitions.BabyPaletteNames)
        {
            if (!document.BabyPalettes.TryGetValue(name, out ushort[]? colors) ||
                colors is null || colors.Length != GameOverRomData.BabyAnimation.PaletteColorCount ||
                colors.Any(color => color > 0x7fff))
                throw new InvalidDataException(
                    $"Game-over Baby palette {name} requires sixteen SNES BGR555 colors.");
            palettes.Add(name, (ushort[])colors.Clone());
        }

        return new(tilemap, sprites, palettes, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

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

public sealed record GameOverPresentationDocument
{
    public required int Version { get; init; }
    public required MapPresentationCell[] Tilemap { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    public required Dictionary<string, ushort[]> BabyPalettes { get; init; }
    public required MapLabelPoint BabyAnchor { get; init; }
    public required int CursorX { get; init; }
    public required int YesCursorY { get; init; }
    public required int NoCursorY { get; init; }
    public required int BabyPalette { get; init; }
    public required int EggPalette { get; init; }
    public required int CursorPalette { get; init; }
    public required int CursorFrameDuration { get; init; }
}

/// <summary>Schema names and geometry for <c>game-over.json</c>.</summary>
public static class GameOverPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "game-over.json";
    public const int TilemapCellCount = GameOverRomData.TilemapWidth * GameOverRomData.TilemapHeight;
    public const int TilemapByteCount = TilemapCellCount * sizeof(ushort);
    public const int CursorFrameCount = 4;
    public const string EggFrame = "Egg";

    private static readonly string[] spriteNames =
    [
        "Baby.Closed", "Baby.Middle", "Baby.Open", EggFrame,
        "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
    ];

    private static readonly string[] paletteNames =
        ["Baby.Idle", "Baby.ClosedCry", "Baby.MiddleCry", "Baby.OpenCry"];

    public static ReadOnlySpan<string> SpriteNames => spriteNames;
    public static ReadOnlySpan<string> BabyPaletteNames => paletteNames;

    public static string BabyFrameName(GameOverBabyFrame frame) => frame switch
    {
        GameOverBabyFrame.Closed => "Baby.Closed",
        GameOverBabyFrame.Middle => "Baby.Middle",
        GameOverBabyFrame.Open => "Baby.Open",
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    public static string CursorFrameName(int frame) => frame switch
    {
        0 => "Cursor.0",
        1 => "Cursor.1",
        2 => "Cursor.2",
        3 => "Cursor.3",
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    public static string BabyPaletteName(GameOverBabyPalette palette) => palette switch
    {
        GameOverBabyPalette.Idle => "Baby.Idle",
        GameOverBabyPalette.ClosedCry => "Baby.ClosedCry",
        GameOverBabyPalette.MiddleCry => "Baby.MiddleCry",
        GameOverBabyPalette.OpenCry => "Baby.OpenCry",
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };
}
