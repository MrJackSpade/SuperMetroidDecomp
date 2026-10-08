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

    /// <summary>Selected BGR555 copyright highlight restored at CGRAM color 201 by the title fast-skip route and title-screen presentation rebinding.</summary>
    public ushort SkipCopyrightWhite { get; }
    /// <summary>Selected BGR555 copyright tint restored at CGRAM color 202 on fast skip; the legacy Red name does not restrict its channels, and the stock word is $7D80.</summary>
    public ushort SkipCopyrightRed { get; }

    /// <inheritdoc />
    /// <param name="pointer">Bank-$8D address of a color operand in the tube-light or display program, not its duration or instruction word.</param>
    /// <param name="color">Selected native BGR555 word when found, otherwise zero.</param>
    /// <returns>True when the pointer identifies an authored or matching calculated ambient color; false for unrelated program addresses.</returns>
    public bool TryReadColor(ushort pointer, out ushort color) =>
        animatedColors.TryGetValue(pointer, out color) ||
        TitleAmbientColorDefinitions.TryCalculate(pointer, colors, animatedColors, out color);

    /// <summary>Loads the complete authored palette into native CGRAM slots zero through 255.</summary>
    /// <param name="destination">Color memory whose full initial palette is replaced; ambient animation and skip-only replacements are applied separately by the title state.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
    public void Apply(SnesCgram destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        for (int index = 0; index < colors.Length; index++)
            destination.SetColor(index, colors[index]);
    }

    /// <summary>Validates initial, ambient-animation and skip-only RGB5 colors and compiles independently owned native palette words.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable color source; ambient samples are calculated from selected paint only where the calculated output matches the supplied sample, preserving independent edits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or array dimensions are wrong, or a required color is null or has a channel outside 0..31.</exception>
    public static TitlePalettePresentation Load(Stream json)
    {
        TitlePaletteDocument document = JsonAssetDocument.Read<TitlePaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "title palette presentation");

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

        foreach (var program in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            ushort pointer = (ushort)(program.FramePointer(frame) + sizeof(ushort) * (index + 1));
            if (TitleAmbientColorDefinitions.TryCalculate(pointer, colors, animatedColors, out ushort calculated) &&
                animatedColors[pointer] == calculated) animatedColors.Remove(pointer);
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

    /// <summary>Serializes and validates the complete title palette document before writing any UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Editable RGB5 collections read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema, frame-size or RGB5 validation.</exception>
    public static void Write(Stream json, TitlePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable RGB5 schema for the initial title palette, ambient color frames and copyright replacements; nested arrays remain caller-mutable until compilation.</summary>
public sealed record TitlePaletteDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitlePaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 nonnull colors in CGRAM index order, corresponding to Palettes_TitleScreen at $8C:E1E9; every channel is 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
    /// <summary>Eight ordered four-color RGB5 rows for $8D:C7FE's tube-light loop, targeting CGRAM 42..45; each row's ten-update duration remains compiled mechanics.</summary>
    public required PaletteRgb5[][] BabyMetroidTubeLight { get; init; }
    /// <summary>Two ordered two-color RGB5 rows for $8D:C866's display-flicker loop, targeting CGRAM 46..47; alternating one-update durations remain compiled mechanics.</summary>
    public required PaletteRgb5[][] FlickeringDisplays { get; init; }
    /// <summary>Nonnull RGB5 copyright highlight restored at CGRAM 201 on fast skip, independently editable from the initial palette's same slot.</summary>
    public required PaletteRgb5 SkipCopyrightWhite { get; init; }
    /// <summary>Nonnull RGB5 copyright tint restored at CGRAM 202 on fast skip, independently editable from the initial palette; Red is a legacy role name rather than a color constraint.</summary>
    public required PaletteRgb5 SkipCopyrightRed { get; init; }
}

/// <summary>Installed file identity and schema revision for title palette paint and its selected ambient animation colors.</summary>
public static class TitlePaletteFormat
{
    /// <summary>JSON filename loaded by the title presentation catalog for initial, ambient and skip-only colors.</summary>
    public const string FileName = "title-palette.json";
    /// <summary>Revision 3 of the schema, requiring all 256 initial colors, both ambient frame sets and both copyright replacements.</summary>
    public const int Version = 3;
}
