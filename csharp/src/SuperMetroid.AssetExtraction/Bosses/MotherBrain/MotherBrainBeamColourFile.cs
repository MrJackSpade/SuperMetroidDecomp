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

    /// <summary>Supported schema revision: 2 writes each colour as <c>#rrggbb</c> (#2564).</summary>
    internal const int Version = 2;

    /// <summary>The document holding <paramref name="colours"/>.</summary>
    /// <param name="colours">The beam backdrop's first colour and its cycle.</param>
    /// <returns>UTF-8 JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(MotherBrainRainbowBeamColours colours)
    {
        Ensure.NotNull(colours);
        return JsonSerializer.SerializeToUtf8Bytes(new MotherBrainBeamColourDocument
        {
            Version = Version,
            First = InstalledColourText.Format(colours.BeamInitialColor),
            Cycle = [.. colours.BeamCycle.Select(InstalledColourText.Format)],
        }, ArtworkJson.Options);
    }

    /// <summary>Reads the beam backdrop's colours a document holds, as installed.</summary>
    /// <param name="json">UTF-8 JSON consumed from its current position and left open.</param>
    /// <returns>The beam backdrop's first colour and its cycle.</returns>
    /// <exception cref="InvalidDataException">
    /// The JSON is malformed, names another version, holds no cycle colour, or a colour is not written as <c>#rrggbb</c> or
    /// is off the game's fade scale.
    /// </exception>
    public static MotherBrainRainbowBeamColours Read(Stream json)
    {
        Ensure.NotNull(json);
        MotherBrainBeamColourDocument document = JsonAssetDocument.Read<MotherBrainBeamColourDocument>(
            json, ArtworkJson.Options, "Mother Brain beam colours");
        if (document.Version != Version)
            throw new InvalidDataException($"Mother Brain beam colours require version {Version}.");
        if (document.Cycle is not { Length: > 0 })
            throw new InvalidDataException("Mother Brain beam colours require at least one cycle colour.");
        return new MotherBrainRainbowBeamColours(Compile(document.First, "first"),
            [.. document.Cycle.Select((colour, index) => Compile(colour, $"cycle {index}"))]);
    }

    /// <summary>One colour of the backdrop, on the game's fade scale.</summary>
    /// <exception cref="InvalidDataException">The colour is absent, not written as <c>#rrggbb</c> or off the scale.</exception>
    private static RgbColor Compile(string? colour, string name) =>
        InstalledColourText.ParseOnFadeScale(colour, $"Mother Brain's {name} beam colour");
}
