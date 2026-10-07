using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable default, speed, and shine RGB5 images for Dachora. Native speed/shine
/// timers and their frame-index arithmetic remain compiled enemy behavior.
/// </summary>
public sealed class DachoraColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("DachoraColorCatalog-v1", content =>
        {
            content.AppendWords("normal", normal);
            content.AppendWordFrames("speed", Frames(DachoraPalettePhase.Speed));
            content.AppendWordFrames("shine", Frames(DachoraPalettePhase.Shine));
        });

    private readonly ushort[] normal;
    // Speed and shine frames calculate from the normal colors; only supplied deviations are stored.
    private readonly Dictionary<int, ushort> speedEdits = new();
    private readonly Dictionary<int, ushort> shineEdits = new();

    private DachoraColorCatalog(ushort[] normal, ushort[][] speed, ushort[][] shine)
    {
        this.normal = normal;
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
        {
            int key = frame * DachoraColorRomData.ColorsPerFrame + color;
            if (speed[frame][color] != SpeedColor(frame, color)) speedEdits.Add(key, speed[frame][color]);
            if (shine[frame][color] != ShineColor(frame, color)) shineEdits.Add(key, shine[frame][color]);
        }
    }

    /// <summary>$A7:F245 speed-boost sequence's flash backdrop in its first frame, authored paint.</summary>
    private const ushort SpeedFlashBackdrop = 12 << 5 | 5 << 10;

    /// <summary>
    /// $A7:F245..F2C4: frame k raises each color's blue channel toward min(blue + 16, 31),
    /// rounding to nearest over three steps; red and green keep the normal color. The first
    /// frame's transparent slot is the authored flash backdrop, later frames keep the normal one.
    /// </summary>
    private ushort SpeedColor(int frame, int color)
    {
        ushort basis = normal[color];
        if (color == 0) return frame == 0 ? SpeedFlashBackdrop : basis;
        int blue = basis >> 10 & 31;
        int target = Math.Min(blue + 16, 31);
        int steps = DachoraColorRomData.AnimatedFrameCount - 1;
        int shifted = blue + ((target - blue) * frame + steps / 2) / steps;
        return (ushort)(basis & 0x3ff | shifted << 10);
    }

    /// <summary>
    /// $A7:F2C5..F344: frame k fades every channel toward white by k/5, as
    /// channel + floor(k * (31 - channel) / 5); the transparent slot keeps the normal color.
    /// </summary>
    private ushort ShineColor(int frame, int color)
    {
        ushort basis = normal[color];
        if (color == 0) return basis;
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int channel = basis >> shift & 31;
            result |= channel + frame * (31 - channel) / 5 << shift;
        }
        return (ushort)result;
    }

    private ushort[][] Frames(DachoraPalettePhase phase) =>
        Enumerable.Range(0, DachoraColorRomData.AnimatedFrameCount)
            .Select(frame => Enumerable.Range(0, DachoraColorRomData.ColorsPerFrame)
                .Select(color => Resolve(phase, frame, color)).ToArray()).ToArray();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(DachoraPalettePhase phase, int frame, int color)
    {
        bool animated = phase is DachoraPalettePhase.Speed or DachoraPalettePhase.Shine;
        if (!(phase == DachoraPalettePhase.Default && frame == 0) &&
            !(animated && (uint)frame < DachoraColorRomData.AnimatedFrameCount))
            throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}.");
        if ((uint)color >= DachoraColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = frame * DachoraColorRomData.ColorsPerFrame + color;
        return phase switch
        {
            DachoraPalettePhase.Speed => speedEdits.TryGetValue(key, out ushort speed) ? speed : SpeedColor(frame, color),
            DachoraPalettePhase.Shine => shineEdits.TryGetValue(key, out ushort shine) ? shine : ShineColor(frame, color),
            _ => normal[color],
        };
    }

    public static DachoraColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        DachoraColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<DachoraColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Dachora color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Dachora color JSON.", error);
        }
        if (document.Version != DachoraColorFormat.Version)
            throw new InvalidDataException("Dachora colors require the supported version.");
        return new(Compile(document.Normal, "normal"),
            CompileFrames(document.Speed, "speed"),
            CompileFrames(document.Shine, "shine"));
    }

    public static byte[] Write(DachoraColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[][] CompileFrames(PaletteRgb5[][]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.AnimatedFrameCount)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.AnimatedFrameCount} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.ColorsPerFrame)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.ColorsPerFrame} RGB5 colors.");
        var compiled = new ushort[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Dachora {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Dachora color property {name}."));
}

public sealed record DachoraColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Normal { get; init; }
    public required PaletteRgb5[][] Speed { get; init; }
    public required PaletteRgb5[][] Shine { get; init; }
}

public static class DachoraColorFormat
{
    public const string FileName = "dachora-colors.json";
    public const int Version = 1;
}
