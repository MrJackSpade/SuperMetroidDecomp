using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    private readonly ushort[][] body;
    private readonly ushort[][] backLegs;

    private MotherBrainHealthPalettePresentation(ushort[][] body, ushort[][] backLegs)
    {
        this.body = body;
        this.backLegs = backLegs;
    }

    /// <summary>Copies one authored color pair to the three native CGRAM destinations.</summary>
    public void Apply(SnesCgram cgram, int damageState)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)damageState >= MotherBrainHealthPaletteFormat.StateCount)
            throw new ArgumentOutOfRangeException(nameof(damageState));
        for (int color = 0; color < MotherBrainRainbowPaletteRomData.ColorCount; color++)
        {
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, body[damageState][color]);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, body[damageState][color]);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.SecondaryColor + color, backLegs[damageState][color]);
        }
    }

    public static MotherBrainHealthPalettePresentation Load(Stream json)
    {
        MotherBrainHealthPaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<MotherBrainHealthPaletteDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Mother Brain health palette is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Mother Brain health palette JSON.", error);
        }
        if (document.Version != MotherBrainHealthPaletteFormat.Version)
            throw new InvalidDataException("Unsupported Mother Brain health palette version.");
        return new(Convert(document.Body, nameof(document.Body)),
            Convert(document.BackLegs, nameof(document.BackLegs)));

        static ushort[][] Convert(PaletteRgb5[][]? frames, string name)
        {
            if (frames is null || frames.Length != MotherBrainHealthPaletteFormat.StateCount)
                throw new InvalidDataException($"Mother Brain {name} requires four damage states.");
            var values = new ushort[frames.Length][];
            for (int state = 0; state < frames.Length; state++)
            {
                PaletteRgb5[]? colors = frames[state];
                if (colors is null || colors.Length != MotherBrainRainbowPaletteRomData.ColorCount)
                    throw new InvalidDataException($"Mother Brain {name} state {state} requires fifteen colors.");
                values[state] = new ushort[colors.Length];
                for (int color = 0; color < colors.Length; color++)
                {
                    PaletteRgb5? rgb = colors[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Mother Brain {name} state {state} color {color} requires RGB5 components.");
                    values[state][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                }
            }
            return values;
        }
    }

    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MotherBrainHealthPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Body { get; init; }
    public required PaletteRgb5[][] BackLegs { get; init; }
}

public static class MotherBrainHealthPaletteFormat
{
    public const string FileName = "mother-brain-health-palette.json";
    public const int Version = 1;
    public const int StateCount = 4;
}
