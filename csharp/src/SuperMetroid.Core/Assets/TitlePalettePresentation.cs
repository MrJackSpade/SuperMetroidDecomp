using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable initial 256-color title palette. Animation programs and destination
/// selection remain compiled engine behavior rather than authored palette data.
/// </summary>
public sealed class TitlePalettePresentation
{
    private readonly ushort[] colors;

    private TitlePalettePresentation(ushort[] colors) => this.colors = colors;

    public ReadOnlySpan<ushort> Colors => colors;

    /// <summary>Loads the complete authored palette into native CGRAM slots zero through 255.</summary>
    public void Apply(SnesCgram destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        for (int index = 0; index < colors.Length; index++)
            destination.SetColor(index, colors[index]);
    }

    public static TitlePalettePresentation Load(Stream json)
    {
        TitlePaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<TitlePaletteDocument>(
                json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Title palette presentation is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title palette presentation JSON.", error);
        }

        if (document.Version != TitlePaletteFormat.Version ||
            document.Colors is null ||
            document.Colors.Length != SnesCgram.ColorCount)
        {
            throw new InvalidDataException(
                $"Title palette requires version {TitlePaletteFormat.Version} and " +
                $"{SnesCgram.ColorCount} RGB5 colors.");
        }

        var colors = new ushort[SnesCgram.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
            {
                throw new InvalidDataException(
                    $"Title palette color {index} requires RGB components from 0 to 31.");
            }
            colors[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return new TitlePalettePresentation(colors);
    }

    public static void Write(Stream json, TitlePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record TitlePaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

public static class TitlePaletteFormat
{
    public const string FileName = "title-palette.json";
    public const int Version = 1;
}
