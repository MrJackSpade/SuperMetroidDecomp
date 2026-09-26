using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    private readonly ushort[] initial;
    private readonly ushort[][] fade;

    private BabyMetroidCutsceneColorCatalog(ushort[] initial, ushort[][] fade)
    {
        this.initial = initial;
        this.fade = fade;
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
            ? fade[paletteIndex - 1][color]
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

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
