using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    private readonly TintPalette body;
    private readonly TintPalette backLegs;

    private MotherBrainHealthPalettePresentation(ushort[][] body, ushort[][] backLegs)
    {
        this.body = new TintPalette(body, backLeg: false);
        this.backLegs = new TintPalette(backLegs, backLeg: true);
    }

    /// <summary>Calculates one damage-tinted color pair to the three native CGRAM destinations.</summary>
    public void Apply(SnesCgram cgram, int damageState)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)damageState >= MotherBrainHealthPaletteFormat.StateCount)
            throw new ArgumentOutOfRangeException(nameof(damageState));
        for (int color = 0; color < MotherBrainRainbowPaletteRomData.ColorCount; color++)
        {
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, body.Color(damageState, color));
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, body.Color(damageState, color));
            cgram.SetColor(MotherBrainRainbowPaletteRomData.SecondaryColor + color, backLegs.Color(damageState, color));
        }
    }

    /// <summary>
    /// The four native palettes fit an RGB8 red tint before RGB5 quantization.
    /// Body strength is state*(state+1)/30; rear legs add 2/30 after state zero.
    /// Independent base colors remain supplied; matching intermediate rows are discarded.
    /// </summary>
    private sealed class TintPalette
    {
        private readonly TintChannel[] channels;
        private readonly bool backLeg;

        public TintPalette(ushort[][] rows, bool backLeg)
        {
            this.backLeg = backLeg;
            channels = new TintChannel[MotherBrainRainbowPaletteRomData.ColorCount * 3];
            for (int color = 0; color < MotherBrainRainbowPaletteRomData.ColorCount; color++)
                for (int component = 0; component < 3; component++)
                {
                    int first = (rows[0][color] >> (component * 5)) & 31;
                    int basis = first * 8;
                    for (; basis < first * 8 + 8; basis++)
                    {
                        bool matches = true;
                        for (int state = 0; state < MotherBrainHealthPaletteFormat.StateCount; state++)
                            if (Tint(basis, component, state, backLeg) !=
                                ((rows[state][color] >> (component * 5)) & 31))
                            { matches = false; break; }
                        if (matches) break;
                    }
                    byte[]? supplied = null;
                    if (basis == first * 8 + 8)
                    {
                        supplied = new byte[MotherBrainHealthPaletteFormat.StateCount];
                        for (int state = 0; state < supplied.Length; state++)
                            supplied[state] = (byte)((rows[state][color] >> (component * 5)) & 31);
                    }
                    channels[color * 3 + component] = new TintChannel(basis, supplied);
                }
        }

        public ushort Color(int state, int color)
        {
            int result = 0;
            for (int component = 0; component < 3; component++)
            {
                TintChannel channel = channels[color * 3 + component];
                int value = channel.Supplied is { } supplied ? supplied[state] :
                    Tint(channel.Basis, component, state, backLeg);
                result |= value << (component * 5);
            }
            return (ushort)result;
        }

        private static int Tint(int basis, int component, int state, bool backLeg)
        {
            int amount = state * (state + 1) + (backLeg && state != 0 ? 2 : 0);
            int redTarget = component == 0 ? 31 * 8 : 0;
            return (basis * (30 - amount) + redTarget * amount) / (30 * 8);
        }

        private readonly record struct TintChannel(int Basis, byte[]? Supplied);
    }
    public static MotherBrainHealthPalettePresentation Load(Stream json)
    {
        MotherBrainHealthPaletteDocument document = JsonAssetDocument.Read<MotherBrainHealthPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain health palette");
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
