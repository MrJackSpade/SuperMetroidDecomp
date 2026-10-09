using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only eye-door overrides.</summary>
public static class RoomPlmEyeDoorVisualFiles
{
    /// <summary>JSON filename for editable eye-door eye, middle, bottom, and shared clearing frames.</summary>
    public const string VisualFileName = "eye-doors.json";

    /// <summary>Verifies the twenty-three editable native eye-door layouts and creates visual-only JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of full component level words and their one-, two-, or four-block run shapes.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled editable eye-door definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "eye-door", EyeDoorPlmDrawDefinitions.Editable,
            EyeDoorPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock eye-door art against its manifest and compiled frames, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the eye-door JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose eye-door JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected component art, with the opening-clear alias mirrored from the shared clearing frame and native door mechanics unchanged.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate editable coverage or visual-word rules.</exception>
    public static RoomPlmEyeDoorVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "eye-door", EyeDoorPlmDrawDefinitions.Editable,
            EyeDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmEyeDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmEyeDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    /// <summary>Checks stock eye-door provenance, digest, schema, twenty-three-frame coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="stockDirectory">Installed stock eye-door directory.</param>
    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
