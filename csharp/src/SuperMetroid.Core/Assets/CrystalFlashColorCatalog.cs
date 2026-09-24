using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Crystal Flash body and bubble colors. Native timers, frame cursors,
/// and completion still belong to <see cref="SamusCrystalFlashState"/>.
/// </summary>
public sealed class CrystalFlashColorCatalog
{
    private readonly ushort[][] body;
    private readonly ushort[][] bubble;

    private CrystalFlashColorCatalog(ushort[][] body, ushort[][] bubble)
    {
        this.body = body;
        this.bubble = bubble;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveBody(int frame, int color) => Resolve(body, frame, color);
    public ushort ResolveBubble(int frame, int color) => Resolve(bubble, frame, color);

    public void ApplyBody(SnesCgram cgram, int frame) => Apply(cgram, body, frame,
        SamusPaletteRomData.CrystalFlash.BodyCgramStart);

    public void ApplyBubble(SnesCgram cgram, int frame) => Apply(cgram, bubble, frame,
        SamusPaletteRomData.CrystalFlash.BubbleCgramStart);

    public static CrystalFlashColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CrystalFlashColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CrystalFlashColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Crystal Flash color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Crystal Flash color JSON.", error);
        }
        if (document.Version != CrystalFlashColorFormat.Version)
            throw new InvalidDataException("Crystal Flash colors require the supported version.");
        return new(Compile(document.Body, CrystalFlashColorFormat.BodyFrameCount,
                CrystalFlashColorFormat.BodyColorCount, "body"),
            Compile(document.Bubble, CrystalFlashColorFormat.BubbleFrameCount,
                CrystalFlashColorFormat.BubbleColorCount, "bubble"));
    }

    public static byte[] Write(CrystalFlashColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Resolve(ushort[][] source, int frame, int color)
    {
        if ((uint)frame >= source.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= source[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
        return source[frame][color];
    }

    private static void Apply(SnesCgram cgram, ushort[][] source, int frame, int start)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= source.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        ushort[] colors = source[frame];
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(start + index, colors[index]);
    }

    private static ushort[][] Compile(PaletteRgb5[][]? source, int frameCount,
        int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException($"Crystal Flash {name} requires {frameCount} frames.");
        var result = new ushort[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            PaletteRgb5[]? colors = source[frame];
            if (colors is null || colors.Length != colorCount)
                throw new InvalidDataException(
                    $"Crystal Flash {name} frame {frame} requires {colorCount} colors.");
            result[frame] = new ushort[colorCount];
            for (int index = 0; index < colorCount; index++)
            {
                PaletteRgb5? rgb = colors[index];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Crystal Flash {name} frame {frame}, color {index} requires RGB5 channels 0..31.");
                result[frame][index] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Crystal Flash color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record CrystalFlashColorDocument
{
    public required int Version { get; init; }
    /// <summary>Ten native body records in playback order, ten colors each.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Six independent bubble frames in playback order, six colors each.</summary>
    public required PaletteRgb5[][] Bubble { get; init; }
}

public static class CrystalFlashColorFormat
{
    public const string FileName = "crystal-flash-colors.json";
    public const int Version = 1;
    public const int BodyFrameCount = SamusPaletteRomData.CrystalFlash.BodyRecordCount;
    public const int BubbleFrameCount = SamusPaletteRomData.CrystalFlash.BubblePaletteCount;
    public const int BodyColorCount = SamusPaletteRomData.CrystalFlash.BodyColorCount;
    public const int BubbleColorCount = SamusPaletteRomData.CrystalFlash.BubbleColorCount;
}
