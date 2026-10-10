using SuperMetroid.Core.Hardware;
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
            content.AppendColors("normal", normal);
            content.AppendColorFrames("speed", Frames(DachoraPalettePhase.Speed));
            content.AppendColorFrames("shine", Frames(DachoraPalettePhase.Shine));
        });

    private readonly Bgr555[] normal;
    // Speed and shine frames calculate from the normal colors; only supplied deviations are stored.
    private readonly Dictionary<int, Bgr555> speedEdits = new();
    private readonly Dictionary<int, Bgr555> shineEdits = new();

    private DachoraColorCatalog(Bgr555[] normal, Bgr555[][] speed, Bgr555[][] shine)
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
    private static readonly Bgr555 SpeedFlashBackdrop = new(0, 12, 5);

    /// <summary>
    /// $A7:F245..F2C4: frame k raises each color's blue channel toward min(blue + 16, 31),
    /// rounding to nearest over three steps; red and green keep the normal color. The first
    /// frame's transparent slot is the authored flash backdrop, later frames keep the normal one.
    /// </summary>
    private Bgr555 SpeedColor(int frame, int color)
    {
        Bgr555 basis = normal[color];
        if (color == 0) return frame == 0 ? SpeedFlashBackdrop : basis;
        int blue = basis.Blue;
        int target = Math.Min(blue + 16, 31);
        int steps = DachoraColorRomData.AnimatedFrameCount - 1;
        int shifted = blue + ((target - blue) * frame + steps / 2) / steps;
        return basis.WithBlue(shifted);
    }

    /// <summary>
    /// $A7:F2C5..F344: frame k fades every channel toward white by k/5, as
    /// channel + floor(k * (31 - channel) / 5); the transparent slot keeps the normal color.
    /// </summary>
    private Bgr555 ShineColor(int frame, int color)
    {
        Bgr555 basis = normal[color];
        if (color == 0) return basis;
        return basis.Map((_, channel) => channel + frame * (31 - channel) / 5);
    }

    private Bgr555[][] Frames(DachoraPalettePhase phase) =>
        Enumerable.Range(0, DachoraColorRomData.AnimatedFrameCount)
            .Select(frame => Enumerable.Range(0, DachoraColorRomData.ColorsPerFrame)
                .Select(color => Resolve(phase, frame, color)).ToArray()).ToArray();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Resolves one selected Dachora color, using the normal palette as a calculated animated basis while preserving every supplied speed/shine deviation.</summary>
    /// <param name="phase">Exclusive default, speed-boost, or stored-shine palette phase selected by enemy behavior.</param>
    /// <param name="frame">Zero for the default phase, or zero-based animated frame 0 through 3 for speed and shine.</param>
    /// <param name="color">Palette color index 0 through 15, including the transparent slot at zero.</param>
    /// <returns>SNES RGB555 word with red in bits 0..4, green in bits 5..9, and blue in bits 10..14; the caller chooses the actor's OBJ palette destination.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    public Bgr555 Resolve(DachoraPalettePhase phase, int frame, int color)
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
            DachoraPalettePhase.Speed => speedEdits.TryGetValue(key, out Bgr555 speed) ? speed : SpeedColor(frame, color),
            DachoraPalettePhase.Shine => shineEdits.TryGetValue(key, out Bgr555 shine) ? shine : ShineColor(frame, color),
            DachoraPalettePhase.Default => normal[color],
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Dachora palette phase."),
        };
    }

    /// <summary>Loads the supported camel-case JSON schema, rejecting duplicate or unknown properties, incorrect palette/frame counts, null colors, and RGB5 channels outside 0..31.</summary>
    /// <param name="json">Readable JSON stream, left open after loading.</param>
    /// <returns>Validated default, speed, and shine colors without cartridge reads or ownership of native animation timers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, palette dimensions, or color values are invalid.</exception>
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

    /// <summary>Serializes the editable palettes as indented camel-case UTF-8 JSON and validates the result through <see cref="Load"/> before returning it.</summary>
    /// <param name="document">Supported-version default palette and four-frame speed/shine sequences.</param>
    /// <returns>Validated JSON bytes ready to store as the Dachora color resource.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails catalog validation.</exception>
    public static byte[] Write(DachoraColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static Bgr555[][] CompileFrames(PaletteRgb5[][]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.AnimatedFrameCount)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.AnimatedFrameCount} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    private static Bgr555[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.ColorsPerFrame)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.ColorsPerFrame} RGB5 colors.");
        var compiled = new Bgr555[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Dachora {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Dachora color property {name}."));
}

/// <summary>Editable RGB5 palette payload for ordinary Dachora enemy $E5FF; frame timing, speed thresholds, and shine-state transitions are not schema fields.</summary>
public sealed record DachoraColorDocument
{
    /// <summary>Schema revision required to equal <see cref="DachoraColorFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen default RGB5 colors, including transparent slot zero, corresponding to <c>Palette_Dachora</c> at $A7:F225.</summary>
    public required PaletteRgb5[] Normal { get; init; }
    /// <summary>Four ordered speed-boost frames of sixteen RGB5 colors each, corresponding to $A7:F245-$F2C4; native enemy behavior selects their cadence.</summary>
    public required PaletteRgb5[][] Speed { get; init; }
    /// <summary>Four ordered stored-shine frames of sixteen RGB5 colors each, corresponding to $A7:F2C5-$F344; the separate native shine loop selects the frame.</summary>
    public required PaletteRgb5[][] Shine { get; init; }
}

/// <summary>Installed resource identity and supported schema version for Dachora's default and animated RGB5 palettes.</summary>
public static class DachoraColorFormat
{
    /// <summary>Installed palette resource, <c>dachora-colors.json</c>.</summary>
    public const string FileName = "dachora-colors.json";
    /// <summary>Supported Dachora color JSON schema revision one.</summary>
    public const int Version = 1;
}
