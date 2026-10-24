using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable Ceres-door tile DMA and RGB5 palettes; actor timing remains engine-owned.</summary>
public sealed class CeresDoorVisualCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
        {
            content.Append("tiles", tiles.Characters.Span);
            Span<Bgr555> normalColors = stackalloc Bgr555[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = normal.ColorAt(color);
            content.AppendColors("normal", normalColors);
            Span<Bgr555> escapeColors = stackalloc Bgr555[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = EscapeColor(color);
            content.AppendColors("escape", escapeColors);
            content.Append("animation", CeresDoorVisualRomData.AnimationRowCount);
            Span<Bgr555> rowColors = stackalloc Bgr555[CeresDoorVisualRomData.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualRomData.AnimationRowCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = AnimationColor(row, color);
                content.AppendColors("row", rowColors);
            }
            content.Append("mode7-frames", CeresDoorVisualRomData.Mode7FrameCount);
            Span<byte> platform = stackalloc byte[CeresDoorVisualRomData.Mode7FrameByteCount];
            for (int frame = 0; frame < CeresDoorVisualRomData.Mode7FrameCount; frame++)
            {
                for (int index = 0; index < platform.Length; index++) platform[index] = PlatformTile(frame, index);
                content.Append("mode7-frame", platform);
            }
        });

    /// <summary>Validated 4bpp door characters used for the VRAM transfer.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Normal setup colors, including any supplied edits to stock values.</summary>
    private readonly CeresDoorNormalPaintDefinitions normal;
    /// <summary>Escape setup colors resolved against the normal palette.</summary>
    internal readonly CeresDoorEscapePaintDefinitions escape;
    /// <summary>Eight animation rows resolved against the normal palette.</summary>
    internal readonly CeresDoorAnimationPaintDefinitions animation;
    /// <summary>Mode 7 strip bytes that differ from the canonical platform frames.</summary>
    internal readonly Dictionary<int, byte> platformEdits = [];

    /// <summary>Retains validated visual data and extracts independently edited Mode 7 cells.</summary>
    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, Bgr555[] normal,
        Bgr555[] escape, Bgr555[][] animation, byte[][] mode7DoorFrames)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
        for (int frame = 0; frame < mode7DoorFrames.Length; frame++)
        for (int index = 0; index < CeresDoorVisualRomData.Mode7FrameByteCount; index++)
            if (mode7DoorFrames[frame][index] != PlatformTile(frame, index))
                platformEdits.Add(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, mode7DoorFrames[frame][index]);
    }

    /// <summary>Loads the indexed 4bpp door sheet and version-1 visual JSON, validating native transfer lengths, RGB5 channels, and eight-bit Mode 7 character selectors without importing door timing or variant logic.</summary>
    /// <param name="tilePng">Caller-owned <c>ceres-door-tiles.png</c> stream compiling to $0400 bytes.</param>
    /// <param name="paletteJson">Caller-owned <c>ceres-door-colors.json</c> stream containing setup colors, animation rows, and Mode 7 strip cells.</param>
    /// <returns>The compiled visual snapshot; streams are consumed from their current positions and left open.</returns>
    public static CeresDoorVisualCatalog Load(Stream tilePng, Stream paletteJson)
    {
        Ensure.NotNull(tilePng);
        Ensure.NotNull(paletteJson);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            CeresDoorVisualRomData.TileByteCount);
        CeresDoorVisualDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDoorVisualDocument>(paletteJson, Options)
                ?? throw new InvalidDataException("Ceres-door visual palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres-door visual palette JSON.", error);
        }
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualRomData.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        Bgr555[] normal = Compile(document.Normal, CeresDoorVisualRomData.SetupColorCount,
            "normal");
        Bgr555[] escape = Compile(document.Escape, CeresDoorVisualRomData.SetupColorCount,
            "escape");
        Bgr555[][] animation = document.Animation.Select((row, index) =>
            Compile(row, CeresDoorVisualRomData.AnimationColorCount,
                $"animation row {index}")).ToArray();
        byte[][] mode7DoorFrames = CompileMode7Frames(document.Mode7DoorFrames);
        return new CeresDoorVisualCatalog(tiles, normal, escape, animation,
            mode7DoorFrames);
    }

    /// <summary>Serializes visual JSON as indented camel-case UTF-8, validating version, palette dimensions, RGB5 bounds, and Mode 7 frame bytes before returning it; PNG admission is a separate boundary.</summary>
    public static byte[] Write(CeresDoorVisualDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        // The PNG is validated independently; validate color structure here.
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualRomData.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        _ = Compile(document.Normal, CeresDoorVisualRomData.SetupColorCount, "normal");
        _ = Compile(document.Escape, CeresDoorVisualRomData.SetupColorCount, "escape");
        for (int row = 0; row < document.Animation.Length; row++)
            _ = Compile(document.Animation[row], CeresDoorVisualRomData.AnimationColorCount,
                $"animation row {row}");
        _ = CompileMode7Frames(document.Mode7DoorFrames);
        return json;
    }

    /// <summary>Loads $0400 selected 4bpp bytes, or 32 characters, at VRAM byte $E000, replacing variant two's native $B0:C400 DMA source.</summary>
    public void LoadTiles(SnesVram vram) =>
        tiles.LoadTo(new CeresDoorVram(vram).Tiles);

    /// <summary>Installs fifteen normal nontransparent inks corresponding to $A6:F4EE in current CGRAM, exposing the native completed setup-fade target directly.</summary>
    /// <param name="destination">The door's colours: an inactive door's, or the active door's.</param>
    public void LoadNormalColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            destination.Write(color, normal.ColorAt(color));
    }

    /// <summary>Installs fifteen escape-state nontransparent inks corresponding to $A6:F50E in current CGRAM; escape-state selection remains actor-owned.</summary>
    /// <param name="destination">The active door's colours.</param>
    public void LoadEscapeColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            destination.Write(color, EscapeColor(color));
    }

    /// <summary>Resolves one escape-state ink from the selected palette.</summary>
    private Bgr555 EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs the door animation's six colours from animation row 0–7 corresponding to $A6:F871; the actor derives the row from its frame bits 3–5.</summary>
    public void LoadAnimationColors(BackgroundColourSets colours, int row)
    {
        ColourRun animated = CeresDoorColourDefinitions.Animation.In(colours);
        if ((uint)row >= CeresDoorVisualRomData.AnimationRowCount) throw new ArgumentOutOfRangeException(nameof(row));
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            animated.Write(color, AnimationColor(row, color));
    }

    /// <summary>Resolves one ink from the actor-selected door animation row.</summary>
    private Bgr555 AnimationColor(int row, int color) => animation.ColorAt(row, color);

    /// <summary>$A6:F918/F91C: resolve shared canonical platform imagery or an independent supplied cell edit.</summary>
    private byte PlatformTile(int frame, int index)
    {
        byte stock = CeresMode7TransferDefinitions.PlatformTile(frame, index);
        return platformEdits.TryGetValue(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, out byte edit) ? edit : stock;
    }

    /// <summary>The selected light/dark platform frame 0 or 1: the four Mode 7 map cells the native $A6:F918/$F91C strip writes.</summary>
    public byte[] Mode7DoorFrame(int frame)
    {
        var tiles = new byte[CeresDoorVisualRomData.Mode7FrameByteCount];
        for (int index = 0; index < tiles.Length; index++) tiles[index] = PlatformTile(frame, index);
        return tiles;
    }

    /// <summary>Validates exactly two four-cell Mode 7 strips and converts their character selectors to bytes.</summary>
    private static byte[][] CompileMode7Frames(int[][]? source)
    {
        if (source is null || source.Length != CeresDoorVisualRomData.Mode7FrameCount)
            throw new InvalidDataException("Ceres-door visuals require two Mode-7 tilemap frames.");
        var result = new byte[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
        {
            int[]? values = source[frame];
            if (values is null || values.Length != CeresDoorVisualRomData.Mode7FrameByteCount)
                throw new InvalidDataException($"Ceres-door Mode-7 frame {frame} needs four bytes.");
            result[frame] = new byte[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                if ((uint)values[index] > byte.MaxValue)
                    throw new InvalidDataException(
                        $"Ceres-door Mode-7 frame {frame} value {index} is not a byte.");
                result[frame][index] = (byte)values[index];
            }
        }
        return result;
    }

    /// <summary>Validates a palette's size and RGB5 channel bounds before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int expectedCount, string name)
    {
        if (source is null || source.Length != expectedCount)
            throw new InvalidDataException($"Ceres-door {name} needs {expectedCount} RGB5 colors.");
        var result = new Bgr555[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Ceres-door {name} color {index} must be RGB5.");
            result[index] = color.ToBgr555();
        }
        return result;
    }

    /// <summary>Strict camel-case JSON settings shared by the door visual loader and writer.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
