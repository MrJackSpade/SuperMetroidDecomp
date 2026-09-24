using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable initial 256-color title palette and ambient title-screen color frames.
/// Animation programs and destination selection remain compiled engine behavior.
/// </summary>
public sealed class TitlePalettePresentation : IPaletteFxColorSource
{
    private readonly ushort[] colors;
    private readonly Dictionary<ushort, ushort> animatedColors;

    private TitlePalettePresentation(
        ushort[] colors,
        Dictionary<ushort, ushort> animatedColors,
        ushort skipCopyrightWhite,
        ushort skipCopyrightRed)
    {
        this.colors = colors;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightRed = skipCopyrightRed;
    }

    public ReadOnlySpan<ushort> Colors => colors;

    /// <summary>The two copyright glyph colors restored by the title's fast-skip route.</summary>
    public ushort SkipCopyrightWhite { get; }
    public ushort SkipCopyrightRed { get; }

    /// <inheritdoc />
    public bool TryReadColor(ushort pointer, out ushort color) =>
        animatedColors.TryGetValue(pointer, out color);

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
        var animatedColors = new Dictionary<ushort, ushort>();
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in
                 TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight =>
                    document.BabyMetroidTubeLight,
                TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays =>
                    document.FlickeringDisplays,
                _ => throw new InvalidDataException(
                    $"Unsupported title ambient palette owner {definition.Owner}."),
            };
            if (frames is null || frames.Length != definition.FrameCount)
            {
                throw new InvalidDataException(
                    $"Title palette {definition.Owner} requires exactly " +
                    $"{definition.FrameCount} frames.");
            }

            for (int frame = 0; frame < frames.Length; frame++)
            {
                PaletteRgb5[]? frameColors = frames[frame];
                if (frameColors is null || frameColors.Length != definition.ColorsPerFrame)
                {
                    throw new InvalidDataException(
                        $"Title palette {definition.Owner} frame {frame} requires exactly " +
                        $"{definition.ColorsPerFrame} colors.");
                }

                for (int index = 0; index < frameColors.Length; index++)
                {
                    PaletteRgb5? color = frameColors[index];
                    if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                        (uint)color.Blue > 31)
                    {
                        throw new InvalidDataException(
                            $"Title palette {definition.Owner} frame {frame} color {index} " +
                            "requires RGB components from 0 to 31.");
                    }

                    ushort pointer = unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort) +
                        index * sizeof(ushort)));
                    animatedColors.Add(
                        pointer,
                        (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
                }
            }
        }

        return new TitlePalettePresentation(colors, animatedColors,
            PackColor(document.SkipCopyrightWhite, "skip copyright white"),
            PackColor(document.SkipCopyrightRed, "skip copyright red"));
    }

    private static ushort PackColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
            (uint)color.Blue > 31)
            throw new InvalidDataException($"Title {name} requires RGB components from 0 to 31.");
        return (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
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
    public required PaletteRgb5[][] BabyMetroidTubeLight { get; init; }
    public required PaletteRgb5[][] FlickeringDisplays { get; init; }
    public required PaletteRgb5 SkipCopyrightWhite { get; init; }
    public required PaletteRgb5 SkipCopyrightRed { get; init; }
}

public static class TitlePaletteFormat
{
    public const string FileName = "title-palette.json";
    public const int Version = 3;
}
