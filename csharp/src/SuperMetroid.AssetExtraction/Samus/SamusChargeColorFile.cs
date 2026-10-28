using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus charge colour document.</summary>
public static class SamusChargeColorFile
{
    /// <summary>Loads version-1 <c>samus-charge-colors.json</c>, validating three suits with six phases each, ten Hyper-shot frames, sixteen colors per row, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled selected colors, sharing stock repeated phases and calculated fades only when supplied values agree.</returns>
    public static InstalledSamusChargeColors Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusChargeColorDocument document;
        document = JsonAssetDocument.Read<SamusChargeColorDocument>(json, ArtworkJson.Options, "Samus charge color");
        if (document.Version != SamusChargeColorFormat.Version)
            throw new InvalidDataException("Samus charge colors require the supported version.");
        return new(CompileCharge(document.ChargedBeam, "charged beam"),
            CompileCharge(document.PseudoScrew, "pseudo-Screw"),
            CompileHyper(document.HyperShot));
    }

    /// <summary>Serializes the charge/pseudo-Screw/Hyper-shot colors as indented camel-case UTF-8 JSON, validating schema, array dimensions, and RGB5 channel bounds before returning the bytes.</summary>
    public static byte[] Write(SamusChargeColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Requires three suits and six complete RGB5 rows per suit before building the charge or pseudo-Screw view.</summary>
    private static RgbColor[][][] CompileCharge(PaletteRgb5[][][]? source, string name)
    {
        if (source is null || source.Length != SamusChargeColorFormat.SuitCount)
            throw new InvalidDataException($"{name} requires three suits.");
        var result = new RgbColor[source.Length][][];
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? phases = source[suit];
            if (phases is null || phases.Length != SamusChargeColorFormat.PhasesPerSuit)
                throw new InvalidDataException($"{name} suit {suit} requires six phases.");
            result[suit] = new RgbColor[phases.Length][];
            for (int phase = 0; phase < phases.Length; phase++)
                result[suit][phase] = CompileColors(phases[phase],
                    $"{name} suit {suit}, phase {phase}");
        }
        return result;
    }

    /// <summary>Uses the same palette algorithms for the reverse Hyper-shot playback view.</summary>
    /// <remarks>Original91D82B..D83D and91D99E..D9B0 contain the same
    /// ten pointers. Shot playback decrements its table offset from20 to2,
    /// reversing the full-body cycle. Inputs remain asset-local: edits to the
    /// shot asset do not alter the separately supplied full-body cycle.</remarks>
    private static InstalledSamusHyperBeamColors CompileHyper(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != SamusChargeColorFormat.HyperFrameCount)
            throw new InvalidDataException("Hyper shot requires ten frames.");
        var cycleOrder = new PaletteRgb5[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
            cycleOrder[source.Length - 1 - frame] = source[frame];
        return SamusHyperBeamColorFile.FromFrames(cycleOrder);
    }

    /// <summary>Requires all sixteen palette slots and five-bit RGB channels before packing a row as BGR555.</summary>
    private static RgbColor[] CompileColors(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusChargeColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"{name} requires sixteen colors.");
        var result = new RgbColor[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException($"{name} color {index} requires RGB5 channels 0..31.");
            result[index] = rgb.ToRgbColor();
        }
        return result;
    }
}
