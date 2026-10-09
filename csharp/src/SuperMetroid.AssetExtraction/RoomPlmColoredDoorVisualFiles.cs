using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable yellow, green, and red door-cap visual references.</summary>
public static class RoomPlmColoredDoorVisualFiles
{
    /// <summary>JSON filename for yellow, green, and red door-cap visual frames.</summary>
    public const string VisualFileName = "colored-doors.json";

    /// <summary>Verifies all forty-eight native colored-door frames and creates four-word visual entries plus their provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of door-cap level words and horizontal or vertical run geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled colored-door definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Colored-door", ColoredDoorPlmDrawDefinitions.All,
            ColoredDoorPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock colored-door art against its manifest and compiled frames, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the colored-door JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose colored-door JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected three-color, four-orientation cap artwork; collision and door-opening mechanics remain compiled.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate frame coverage or visual-word rules.</exception>
    public static RoomPlmColoredDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Colored-door", ColoredDoorPlmDrawDefinitions.All,
            ColoredDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmColoredDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmColoredDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    /// <summary>Checks stock colored-door provenance, digest, schema, forty-eight-frame coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="directory">Installed stock colored-door directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
