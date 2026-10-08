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
            content.AppendWords("statueColors", statueColors.Words());
            for (int color = 0; color < TourianStatuePaletteRomData.GreyColorCount; color++) row[color] = ResolveGrey(color);
            content.AppendWords("greyColors", row[..TourianStatuePaletteRomData.GreyColorCount]);
            content.AppendWordFrames("eyeColors", Enumerable.Range(0, TourianStatuePaletteRomData.EyeRowCount).Select(row =>
                Enumerable.Range(0, TourianStatuePaletteRomData.EyeColorCount).Select(color => ResolveEye(row, color)).ToArray()).ToArray());
        });

    private readonly PaletteBand baseColors;
    private readonly PaletteBand statueColors;
    private readonly PaletteBand eyeColors;
    private readonly PaletteBand greyColors;

    private TourianStatueColorCatalog(ushort[] baseColors, ushort[] statueColors,
        ushort[][] eyeColors, ushort[] greyColors)
    {
        this.baseColors = new(TourianStatuePaintBand.Base, baseColors);
        this.statueColors = new(TourianStatuePaintBand.Statue, statueColors);
        this.eyeColors = new(TourianStatuePaintBand.Eye, eyeColors.SelectMany(row => row).ToArray());
        this.greyColors = new(TourianStatuePaintBand.Grey, greyColors);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Gets packed RGB5 base-decoration ink 0–15 corresponding to $AA:D785, including its retained transparent-slot word.</summary>
    public ushort ResolveBase(int color) => baseColors.Read(color);
    /// <summary>Gets packed RGB5 eye ink 0–3 from row 0–3 of $86:B91E, in Phantoon, Ridley, Draygon, Kraid order.</summary>
    public ushort ResolveEye(int row, int color)
    {
        if ((uint)row >= TourianStatuePaletteRomData.EyeRowCount ||
            (uint)color >= TourianStatuePaletteRomData.EyeColorCount) throw new IndexOutOfRangeException();
        return eyeColors.Read(row * TourianStatuePaletteRomData.EyeColorCount + color);
    }
    /// <summary>Gets packed RGB5 ink 0–7 from the statue's grey-transition band corresponding to $87:839C; destination and transition ordering remain animation-owned.</summary>
    public ushort ResolveGrey(int color) => greyColors.Read(color);

    /// <summary>Installs all sixteen base-decoration inks at CGRAM 240–255 and all sixteen statue inks at 160–175, matching the entrance initializer's two OBJ palette bands.</summary>
    public void ApplyEntrance(SnesCgram cgram)
    {
        baseColors.Apply(cgram, TourianStatuePaletteRomData.BaseCgramIndex);
        statueColors.Apply(cgram, TourianStatuePaletteRomData.StatueCgramIndex);
    }

    /// <summary>Installs the selected four-color eye glow at CGRAM 249–252 without altering boss completion or animated-tile state.</summary>
    /// <param name="cgram">Current palette receiving the selected eye colors.</param>
    /// <param name="doubledBossParameter">Native doubled selector: Phantoon 0, Ridley 2, Draygon 4, or Kraid 6.</param>
    public void ApplyEye(SnesCgram cgram, ushort doubledBossParameter)
    {
        if (doubledBossParameter > 6 || (doubledBossParameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(doubledBossParameter));
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < TourianStatuePaletteRomData.EyeColorCount; color++)
            cgram.SetColor(TourianStatuePaletteRomData.EyeCgramIndex + color, ResolveEye(doubledBossParameter >> 1, color));
    }

    /// <summary>Installs the eight selected grey inks in current CGRAM at the animation instruction's destination; this method does not own target-palette fade progression.</summary>
    /// <param name="cgram">Current palette to update.</param>
    /// <param name="destinationColor">First CGRAM color index, obtained by dividing the native byte-index operand by two.</param>
    public void ApplyGrey(SnesCgram cgram, int destinationColor) =>
        greyColors.Apply(cgram, destinationColor);

    /// <summary>Loads version-1 <c>tourian-statue-colors.json</c>, validating sixteen-color base/statue bands, four four-color eye rows, eight grey inks, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled selected colors retaining independent edits over calculated stock paint bands.</returns>
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

    /// <summary>Serializes the editable colors as indented camel-case UTF-8 JSON, validating version, palette dimensions, and RGB5 bounds before returning the bytes.</summary>
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

    /// <summary>One independent installed domain: stock colors calculate; arbitrary supplied words own sparse overrides.</summary>
    private sealed class PaletteBand
    {
        private readonly TourianStatuePaintBand band;
        private readonly int count;
        private readonly Dictionary<int, ushort> edits = [];
        internal PaletteBand(TourianStatuePaintBand band, ReadOnlySpan<ushort> supplied)
        {
            this.band = band;
            count = supplied.Length;
            for (int color = 0; color < count; color++)
                if (supplied[color] != TourianStatuePaintDefinitions.Color(band, color)) edits.Add(color, supplied[color]);
        }
        internal ushort Read(int color)
        {
            if ((uint)color >= count) throw new IndexOutOfRangeException();
            return edits.TryGetValue(color, out ushort edited) ? edited : TourianStatuePaintDefinitions.Color(band, color);
        }
        internal ushort[] Words()
        {
            var result = new ushort[count];
            for (int color = 0; color < count; color++) result[color] = Read(color);
            return result;
        }
        internal void Apply(SnesCgram destination, int firstColor)
        {
            ArgumentNullException.ThrowIfNull(destination);
            for (int color = 0; color < count; color++) destination.SetColor(firstColor + color, Read(color));
        }
    }
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Tourian statue color property."));
}

/// <summary>Editable RGB5 entrance and unlock-effect colors for the Tourian statue, with boss selectors, animation timing, and palette destinations kept in runtime code.</summary>
public sealed record TourianStatueColorDocument
{
    /// <summary>Palette schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen ordered base-decoration inks corresponding to $AA:D785 and OBJ palette 7, including transparent slot zero.</summary>
    public required PaletteRgb5[] Base { get; init; }
    /// <summary>Sixteen ordered statue inks corresponding to $AA:D765 and OBJ palette 2, including transparent slot zero.</summary>
    public required PaletteRgb5[] Statue { get; init; }
    /// <summary>Four rows of four eye inks corresponding to $86:B91E, ordered Phantoon, Ridley, Draygon, Kraid and installed over base-decoration colors 9–12.</summary>
    public required PaletteRgb5[][] Eye { get; init; }
    /// <summary>Eight ordered grey-transition inks corresponding to $87:839C; the animated-tile instruction supplies the destination palette band.</summary>
    public required PaletteRgb5[] Grey { get; init; }
}

/// <summary>Versioned, editable Tourian statue visual palette resource.</summary>
public static class TourianStatueColorFormat
{
    /// <summary>Installed editable JSON filename for entrance base/statue bands, boss-eye glows, and grey-transition colors.</summary>
    public const string FileName = "tourian-statue-colors.json";
    /// <summary>Supported schema revision, requiring the exact native palette-band and eye-row geometry.</summary>
    public const int Version = 1;
}
