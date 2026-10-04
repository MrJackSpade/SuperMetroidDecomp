using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
        {
            content.AppendWords("initial", initial);
            fade.AppendIdentity(content);
        });

    private readonly ushort[] initial;
    private readonly ColorFade fade;

    private BabyMetroidCutsceneColorCatalog(ushort[] initial, ushort[][] fade)
    {
        this.initial = initial;
        this.fade = new(fade);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort InitialColor(int color) =>
        (uint)color < initial.Length ? initial[color] :
            throw new ArgumentOutOfRangeException(nameof(color));

    public ushort FadeColor(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorRomData.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorRomData.FadeColorCount
            ? fade.Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>
    /// $AD:E90C-$E9B3 displays six RGB5 steps of a linear RGB8 fade to black.
    /// Recover a compatible endpoint interval from each channel's quantization
    /// bounds, choosing its midpoint. Endpoints are not unique; every supplied
    /// step must match exactly before its frame table can be discarded.
    /// </summary>
    private sealed class ColorFade
    {
        private const int FadeDivisor = 8 * BabyMetroidCutsceneColorRomData.FadeFrameCount;
        private readonly uint[]? endpointColors;
        private readonly ushort[][]? supplied;

        internal ColorFade(ushort[][] frames)
        {
            var endpoints = new uint[BabyMetroidCutsceneColorRomData.FadeColorCount];
            for (int color = 0; color < endpoints.Length; color++)
            for (int channel = 0; channel < 3; channel++)
            {
                int low = 0, high = 255;
                for (int frame = 0; frame < frames.Length; frame++)
                {
                    int value = (frames[frame][color] >> (channel * 5)) & 31;
                    int remaining = frames.Length - 1 - frame;
                    if (remaining == 0)
                    {
                        if (value != 0)
                            high = -1;
                        break;
                    }
                    // value <= endpoint*remaining/48 < value+1, with integer RGB8 endpoints.
                    low = Math.Max(low, (value * FadeDivisor + remaining - 1) / remaining);
                    high = Math.Min(high, ((value + 1) * FadeDivisor + remaining - 1) / remaining - 1);
                }
                if (low > high)
                {
                    supplied = frames;
                    return;
                }
                endpoints[color] |= (uint)((low + high) / 2) << (channel * 8);
            }
            endpointColors = endpoints;
        }

        internal ushort Resolve(int frame, int color)
        {
            if (endpointColors is null)
                return supplied![frame][color];
            int result = 0;
            int remaining = BabyMetroidCutsceneColorRomData.FadeFrameCount - 1 - frame;
            for (int channel = 0; channel < 3; channel++)
            {
                int endpoint = (int)((endpointColors[color] >> (channel * 8)) & 255);
                result |= (endpoint * remaining / FadeDivisor) << (channel * 5);
            }
            return (ushort)result;
        }

        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("fade", BabyMetroidCutsceneColorRomData.FadeFrameCount);
            Span<ushort> row = stackalloc ushort[BabyMetroidCutsceneColorRomData.FadeColorCount];
            for (int frame = 0; frame < BabyMetroidCutsceneColorRomData.FadeFrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++)
                    row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        }
    }

    public static BabyMetroidCutsceneColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        BabyMetroidCutsceneColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<BabyMetroidCutsceneColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Cutscene Baby color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid cutscene Baby color JSON.", error);
        }
        if (document.Version != BabyMetroidCutsceneColorFormat.Version)
            throw new InvalidDataException("Cutscene Baby colors require the supported version.");
        if (document.Fade is null ||
            document.Fade.Length != BabyMetroidCutsceneColorRomData.FadeFrameCount)
            throw new InvalidDataException("Cutscene Baby fade requires six displayed frames.");
        return new(Compile(document.Initial,
                BabyMetroidCutsceneColorRomData.InitialColorCount, "initial"),
            document.Fade.Select((frame, index) => Compile(frame,
                BabyMetroidCutsceneColorRomData.FadeColorCount,
                $"fade frame {index + 1}")).ToArray());
    }

    public static byte[] Write(BabyMetroidCutsceneColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int required, string name)
    {
        if (source is null || source.Length != required)
            throw new InvalidDataException(
                $"Cutscene Baby {name} requires {required} RGB5 colors.");
        var compiled = new ushort[required];
        for (int color = 0; color < required; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Cutscene Baby {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate cutscene Baby color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record BabyMetroidCutsceneColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Initial { get; init; }
    public required PaletteRgb5[][] Fade { get; init; }
}

public static class BabyMetroidCutsceneColorFormat
{
    public const string FileName = "baby-metroid-cutscene-colors.json";
    public const int Version = 1;
}
