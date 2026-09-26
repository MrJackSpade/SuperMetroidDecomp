using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable entrance, eye-glow, and grey-transition RGB5 colors for the Tourian statue.</summary>
public sealed class TourianStatueColorCatalog
{
    private readonly ushort[] baseColors;
    private readonly ushort[] statueColors;
    private readonly ushort[][] eyeColors;
    private readonly ushort[] greyColors;

    private TourianStatueColorCatalog(ushort[] baseColors, ushort[] statueColors,
        ushort[][] eyeColors, ushort[] greyColors)
    {
        this.baseColors = baseColors;
        this.statueColors = statueColors;
        this.eyeColors = eyeColors;
        this.greyColors = greyColors;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveBase(int color) => baseColors[color];
    public ushort ResolveStatue(int color) => statueColors[color];
    public ushort ResolveEye(int row, int color) => eyeColors[row][color];
    public ushort ResolveGrey(int color) => greyColors[color];

    public void ApplyEntrance(SnesCgram cgram)
    {
        Apply(cgram, baseColors, TourianStatuePaletteRomData.BaseCgramIndex);
        Apply(cgram, statueColors, TourianStatuePaletteRomData.StatueCgramIndex);
    }

    public void ApplyEye(SnesCgram cgram, ushort doubledBossParameter)
    {
        if (doubledBossParameter > 6 || (doubledBossParameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(doubledBossParameter));
        Apply(cgram, eyeColors[doubledBossParameter >> 1],
            TourianStatuePaletteRomData.EyeCgramIndex);
    }

    public void ApplyGrey(SnesCgram cgram, int destinationColor) =>
        Apply(cgram, greyColors, destinationColor);

    public static TourianStatueColorCatalog Load(Stream json)
    {
        TourianStatueColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<TourianStatueColorDocument>(Options)
                ?? throw new InvalidDataException("Tourian statue colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Tourian statue color JSON.", error);
        }
        if (document.Version != TourianStatueColorFormat.Version ||
            document.Eye is null || document.Eye.Length != TourianStatuePaletteRomData.EyeRowCount)
            throw new InvalidDataException("Tourian statue colors require four eye rows at the supported version.");
        var eye = new ushort[document.Eye.Length][];
        for (int row = 0; row < eye.Length; row++)
            eye[row] = Compile(document.Eye[row], TourianStatuePaletteRomData.EyeColorCount,
                $"eye row {row}");
        return new(
            Compile(document.Base, TourianStatuePaletteRomData.BaseColorCount, "base"),
            Compile(document.Statue, TourianStatuePaletteRomData.StatueColorCount, "statue"),
            eye,
            Compile(document.Grey, TourianStatuePaletteRomData.GreyColorCount, "grey"));
    }

    public static byte[] Write(TourianStatueColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? colors, int count, string label)
    {
        if (colors is null || colors.Length != count)
            throw new InvalidDataException($"Tourian statue {label} requires {count} colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < compiled.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Tourian statue {label} color {color} requires RGB5 channels.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void Apply(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(destination + color, colors[color]);
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException("Duplicate Tourian statue color property.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record TourianStatueColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Base { get; init; }
    public required PaletteRgb5[] Statue { get; init; }
    public required PaletteRgb5[][] Eye { get; init; }
    public required PaletteRgb5[] Grey { get; init; }
}

/// <summary>Versioned, editable Tourian statue visual palette resource.</summary>
public static class TourianStatueColorFormat
{
    public const string FileName = "tourian-statue-colors.json";
    public const int Version = 1;
}
