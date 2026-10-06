using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable entrance, eye-glow, and grey-transition RGB5 colors for the Tourian statue.</summary>
public sealed class TourianStatueColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("TourianStatueColorCatalog-v1", content =>
        {
            Span<ushort> row = stackalloc ushort[TourianStatuePaletteRomData.BaseColorCount];
            for (int color = 0; color < row.Length; color++) row[color] = ResolveBase(color);
            content.AppendWords("baseColors", row);
            content.AppendWords("statueColors", statueColors);
            for (int color = 0; color < TourianStatuePaletteRomData.GreyColorCount; color++) row[color] = ResolveGrey(color);
            content.AppendWords("greyColors", row[..TourianStatuePaletteRomData.GreyColorCount]);
            content.AppendWordFrames("eyeColors", eyeColors);
        });

    private readonly PaletteRamp baseColors;
    private readonly ushort[] statueColors;
    private readonly ushort[][] eyeColors;
    private readonly PaletteRamp greyColors;

    private TourianStatueColorCatalog(ushort[] baseColors, ushort[] statueColors,
        ushort[][] eyeColors, ushort[] greyColors)
    {
        this.baseColors = new(baseColors, 1, 8);
        this.statueColors = statueColors;
        this.eyeColors = eyeColors;
        this.greyColors = new(greyColors, 1, 7);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveBase(int color) => baseColors.Read(color);
    public ushort ResolveStatue(int color) => statueColors[color];
    public ushort ResolveEye(int row, int color) => eyeColors[row][color];
    public ushort ResolveGrey(int color) => greyColors.Read(color);

    public void ApplyEntrance(SnesCgram cgram)
    {
        baseColors.Apply(cgram, TourianStatuePaletteRomData.BaseCgramIndex);
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
        greyColors.Apply(cgram, destinationColor);

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

    /// <summary>
    /// Endpoint interpolation for $AA:D785 base colors1..8 and $87:839C grey colors1..7.
    /// The native base ramp is exact; the grey ramp has one green-channel residual.
    /// Endpoints, outside colors and residual choices remain unresolved artwork under
    /// issue1165; no artistic exception is inferred from the near-linear ramps.
    /// </summary>
    private sealed class PaletteRamp
    {
        private readonly int count;
        private readonly int start;
        private readonly int length;
        private readonly ushort first;
        private readonly ushort last;
        private readonly ushort[] outside;
        private readonly Dictionary<int, ushort> edits = [];

        internal PaletteRamp(ushort[] supplied, int start, int length)
        {
            count = supplied.Length;
            this.start = start;
            this.length = length;
            first = supplied[start];
            last = supplied[start + length - 1];
            outside = new ushort[count - length];
            for (int color = 0; color < count; color++)
            {
                if (color < start || color >= start + length)
                    outside[color < start ? color : color - length] = supplied[color];
                else if (supplied[color] != Interpolate(color - start))
                    edits.Add(color, supplied[color]);
            }
        }

        private ushort Interpolate(int step)
        {
            int denominator = length - 1;
            int word = 0;
            for (int shift = 0; shift <= 10; shift += 5)
            {
                int channel = (((first >> shift) & 31) * (denominator - step)
                    + ((last >> shift) & 31) * step + denominator / 2) / denominator;
                word |= channel << shift;
            }
            return (ushort)word;
        }

        internal ushort Read(int color)
        {
            if ((uint)color >= count) throw new IndexOutOfRangeException();
            if (color < start || color >= start + length)
                return outside[color < start ? color : color - length];
            return edits.TryGetValue(color, out ushort edited) ? edited : Interpolate(color - start);
        }

        internal void Apply(SnesCgram destination, int firstColor)
        {
            ArgumentNullException.ThrowIfNull(destination);
            for (int color = 0; color < count; color++)
                destination.SetColor(firstColor + color, Read(color));
        }
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
