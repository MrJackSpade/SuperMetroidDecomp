using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Editable initial 256-color title palette and ambient title-screen color frames.
/// Animation programs and destination selection remain compiled engine behavior.
/// </summary>
public sealed class TitlePalettePresentation : IPaletteFxColorSource
{
    /// <summary>Compiled initial CGRAM image in native slot order.</summary>
    internal readonly Bgr555[] colors;
    /// <summary>Ambient samples retained only when they differ from the calculated paint.</summary>
    internal readonly Dictionary<ushort, Bgr555> animatedColors;

    /// <summary>Retains the selected initial image, ambient edits, and independent skip-only inks.</summary>
    private TitlePalettePresentation(
        Bgr555[] colors,
        Dictionary<ushort, Bgr555> animatedColors,
        Bgr555 skipCopyrightWhite,
        Bgr555 skipCopyrightRed)
    {
        this.colors = colors;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightRed = skipCopyrightRed;
    }

    /// <summary>Selected BGR555 copyright highlight restored at CGRAM color 201 by the title fast-skip route and title-screen presentation rebinding.</summary>
    public Bgr555 SkipCopyrightWhite { get; }
    /// <summary>Selected BGR555 copyright tint restored at CGRAM color 202 on fast skip; the legacy Red name does not restrict its channels, and the stock word is $7D80.</summary>
    public Bgr555 SkipCopyrightRed { get; }

    /// <inheritdoc />
    /// <param name="pointer">Bank-$8D address of a color operand in the tube-light or display program, not its duration or instruction word.</param>
    /// <param name="color">Selected native BGR555 word when found, otherwise zero.</param>
    /// <returns>True when the pointer identifies an authored or matching calculated ambient color; false for unrelated program addresses.</returns>
    public bool TryReadColor(ushort pointer, out Bgr555 color) =>
        animatedColors.TryGetValue(pointer, out color) ||
        TitleAmbientColorDefinitions.TryCalculate(pointer, colors, animatedColors, out color);

    /// <summary>Loads the complete authored palette into native CGRAM slots zero through 255.</summary>
    /// <param name="destination">Color memory whose full initial palette is replaced; ambient animation and skip-only replacements are applied separately by the title state.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
    public void Apply(SnesCgram destination)
    {
        Ensure.NotNull(destination);
        new CgramPalettes(destination).LoadImage(colors);
    }

    /// <summary>Validates initial, ambient-animation and skip-only RGB5 colors and compiles independently owned native palette words.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable color source; ambient samples are calculated from selected paint only where the calculated output matches the supplied sample, preserving independent edits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or array dimensions are wrong, or a required color is null or has a channel outside 0..31.</exception>
    public static TitlePalettePresentation Load(Stream json)
    {
        TitlePaletteDocument document = JsonAssetDocument.Read<TitlePaletteDocument>(
            json, ArtworkJson.Options, "title palette presentation");

        if (document.Version != TitlePaletteFormat.Version ||
            document.Colors is null ||
            document.Colors.Length != CgramPalettes.ColorCount)
        {
            throw new InvalidDataException(
                $"Title palette requires version {TitlePaletteFormat.Version} and " +
                $"{CgramPalettes.ColorCount} RGB5 colors.");
        }

        var colors = new Bgr555[CgramPalettes.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
            {
                throw new InvalidDataException(
                    $"Title palette color {index} requires RGB components from 0 to 31.");
            }
            colors[index] = color.ToBgr555();
        }
        var animatedColors = new Dictionary<ushort, Bgr555>();
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteFxColorDefinitions.All)
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
                        definition.FramePointer(frame) + Bgr555.ByteCount +
                        index * Bgr555.ByteCount));
                    animatedColors.Add(
                        pointer,
                        color.ToBgr555());
                }
            }
        }

        foreach (var program in TitleScreenAmbientPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            ushort pointer = (ushort)(program.FramePointer(frame) + Bgr555.ByteCount * (index + 1));
            if (TitleAmbientColorDefinitions.TryCalculate(pointer, colors, animatedColors, out Bgr555 calculated) &&
                animatedColors[pointer] == calculated) animatedColors.Remove(pointer);
        }
        return new TitlePalettePresentation(colors, animatedColors,
            PackColor(document.SkipCopyrightWhite, "skip copyright white"),
            PackColor(document.SkipCopyrightRed, "skip copyright red"));
    }

    /// <summary>Validates one skip-only RGB5 color before packing its CGRAM word.</summary>
    private static Bgr555 PackColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
            (uint)color.Blue > 31)
            throw new InvalidDataException($"Title {name} requires RGB components from 0 to 31.");
        return color.ToBgr555();
    }

    /// <summary>Serializes and validates the complete title palette document before writing any UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Editable RGB5 collections read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema, frame-size or RGB5 validation.</exception>
    public static void Write(Stream json, TitlePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
