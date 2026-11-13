using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Reads and writes the installed colours of Mother Brain's rainbow beam backdrop (#1637): the only colours of her rainbow
/// attack the game works with, named by what they colour, without the colour groups and places the cartridge kept them at.
/// </summary>
public static class MotherBrainBeamColourFile
{
    /// <summary>The installed document's file name, beside the map presentation.</summary>
    public const string FileName = "mother-brain-beam-colours.json";

    /// <summary>Supported schema revision.</summary>
    public const int Version = 1;

    /// <summary>The document holding <paramref name="colours"/>.</summary>
    /// <param name="colours">The beam backdrop's first colour and its cycle.</param>
    /// <returns>UTF-8 JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(MotherBrainRainbowPalettePresentation colours)
    {
        Ensure.NotNull(colours);
        return JsonSerializer.SerializeToUtf8Bytes(new MotherBrainBeamColourDocument
        {
            Version = Version,
            First = PaletteRgb5.From(colours.BeamInitialColor),
            Cycle = [.. Enumerable.Range(0, colours.BeamCycleLength).Select(index => PaletteRgb5.From(colours.BeamColor(index)))],
        }, ArtworkJson.Options);
    }

    /// <summary>Reads the beam backdrop's colours a document holds, as installed.</summary>
    /// <param name="json">UTF-8 JSON consumed from its current position and left open.</param>
    /// <returns>The beam backdrop's first colour and its cycle.</returns>
    /// <exception cref="InvalidDataException">
    /// The JSON is malformed, names another version, holds no cycle colour, or a channel is outside 0..31.
    /// </exception>
    public static MotherBrainRainbowPalettePresentation Read(Stream json)
    {
        Ensure.NotNull(json);
        MotherBrainBeamColourDocument document = JsonAssetDocument.Read<MotherBrainBeamColourDocument>(
            json, ArtworkJson.Options, "Mother Brain beam colours");
        if (document.Version != Version)
            throw new InvalidDataException($"Mother Brain beam colours require version {Version}.");
        if (document.Cycle is not { Length: > 0 })
            throw new InvalidDataException("Mother Brain beam colours require at least one cycle colour.");
        return new MotherBrainRainbowPalettePresentation(Compile(document.First, "first"),
            [.. document.Cycle.Select((colour, index) => Compile(colour, $"cycle {index}"))]);
    }

    /// <summary>One colour of the backdrop, each channel 0..31.</summary>
    /// <exception cref="InvalidDataException">The colour is absent or a channel is out of range.</exception>
    private static RgbColor Compile(PaletteRgb5? colour, string name) =>
        colour is { Red: >= 0 and <= FadeLevelColour.FullLevel, Green: >= 0 and <= FadeLevelColour.FullLevel, Blue: >= 0 and <= FadeLevelColour.FullLevel }
            ? colour.ToRgbColor()
            : throw new InvalidDataException($"Mother Brain's {name} beam colour requires red, green and blue in 0..{FadeLevelColour.FullLevel}.");
}
