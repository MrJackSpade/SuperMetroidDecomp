using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for reachable Draygon cannons.</summary>
public static class RoomPlmDraygonCannonVisualFiles
{
    /// <summary>JSON filename for reachable left- and right-facing Draygon cannon artwork.</summary>
    public const string VisualFileName = "draygon-cannons.json";

    /// <summary>Verifies the twelve reachable native cannon layouts and creates flattened shield/damaged-frame visual JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of complete cannon level words and two- or three-run geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled reachable cannon definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Draygon cannon", DraygonCannonPlmDrawDefinitions.All,
            DraygonCannonPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock cannon art against its manifest and compiled frames, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the cannon JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose cannon JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected reachable cannon art, leaving hit thresholds, physical collision, and control writes compiled.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate frame coverage or visual-word rules.</exception>
    public static RoomPlmDraygonCannonVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Draygon cannon", DraygonCannonPlmDrawDefinitions.All,
            DraygonCannonPlmDrawDefinitions.VisualId,
            entries => new RoomPlmDraygonCannonVisualCatalog(entries.Select(entry =>
                new RoomPlmDraygonCannonVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Checks stock cannon provenance, digest, schema, twelve-frame coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="directory">Installed stock Draygon cannon directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
