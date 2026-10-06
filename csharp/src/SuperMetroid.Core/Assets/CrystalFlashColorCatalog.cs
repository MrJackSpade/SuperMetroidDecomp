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
    private readonly ushort[][]? body;
    private readonly Dictionary<int, ushort> bubble = [];

    private CrystalFlashColorCatalog(ushort[][] body, ushort[][] bubble)
    {
        // Discard stock body rows only after checking every supplied color. Edited
        // resources keep their complete, independent palette content.
        bool calculated = true;
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            calculated &= body[frame][color] == CalculateBody(frame, color);
        this.body = calculated ? null : body;
        // The rotating ramp accounts for 35 stock words. The remaining native white
        // is an independent painted color choice; retain only that supplied sample.
        // Arbitrary supplied edits remain exact.
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
            if (bubble[frame][color] != CalculateBubble(frame, color))
                this.bubble.Add(frame * CrystalFlashColorFormat.BubbleColorCount + color, bubble[frame][color]);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveBody(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BodyFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BodyColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return body is null ? CalculateBody(frame, color) : body[frame][color];
    }

    /// <summary>
    /// $9B:96C0-$9773 body colors, selected by ten $91:DC00 records: the first
    /// frame is neutral grey16; subsequent frames pulse 19,23,27,23 twice and
    /// return to19. All nine visible body colors share that intensity. The
    /// transparent color is the fixed RGB5 backdrop (0,0,14) in every frame.
    /// </summary>
    private static ushort CalculateBody(int frame, int color)
    {
        if (color == 0)
            return 14 << 10;
        int grey = frame == 0 ? 16 : 27 - 4 * Math.Abs((frame - 1) % 4 - 2);
        return (ushort)(grey | grey << 5 | grey << 10);
    }
    public ushort ResolveBubble(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BubbleFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BubbleColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return bubble.TryGetValue(frame * CrystalFlashColorFormat.BubbleColorCount + color, out ushort supplied)
            ? supplied : CalculateBubble(frame, color);
    }

    /// <summary>
    /// $9B:96D4 + 32*frame rotates six pink-to-white levels one color per frame.
    /// Red stays31; green/blue descend from31 to25 with nearest-integer interpolation
    /// across five intervals. Frame5/color0 at $9B:9774 is independently painted white
    /// ($7FFF), rather than the ramp's $7BDF. The uniform five-tick traversal at
    /// $91:DBA0-DBBD and six-word copy at $91:DC88-DCAE supply no phase-specific
    /// operation generating that choice. Only that sample remains supplied stock data;
    /// a formula exception would merely reencode its frame/color identity and value.
    /// </summary>
    private static ushort CalculateBubble(int frame, int color)
    {
        int phase = (color - frame + CrystalFlashColorFormat.BubbleColorCount)
            % CrystalFlashColorFormat.BubbleColorCount;
        int channel = 31 - (6 * phase + 2) / 5;
        return (ushort)(31 | channel << 5 | channel << 10);
    }

    public void ApplyBody(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            cgram.SetColor(SamusPaletteRomData.CrystalFlash.BodyCgramStart + color,
                ResolveBody(frame, color));
    }
    public void ApplyBubble(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
            cgram.SetColor(SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color,
                ResolveBubble(frame, color));
    }

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
