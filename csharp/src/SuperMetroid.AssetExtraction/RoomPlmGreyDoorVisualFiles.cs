using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable grey-door and shared clear-cap visual references.</summary>
public static class RoomPlmGreyDoorVisualFiles
{
    /// <summary>Family-relative JSON filename for twenty grey-door and shared clear-cap layouts, used in both stock and override directories.</summary>
    public const string VisualFileName = "grey-doors.json";

    /// <summary>Checks the bank-$84 grey-door/clear-cap draw shapes, physical words, and offsets, then exports their twelve-bit visual words in native run order.</summary>
    /// <param name="bus">Cartridge source supplying the compiled draw region $84:A677–A766.</param>
    /// <param name="directory">Family asset directory, created if absent; receives <see cref="VisualFileName"/> and <c>manifest.json</c> as new files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied cartridge hash recorded as provenance; loading requires the supported cartridge identity.</param>
    /// <remarks>Existing files are not overwritten. The JSON and manifest are separate writes, so a later failure can leave an earlier file in place.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A cartridge draw differs from the compiled native layout.</exception>
    /// <exception cref="IOException">An output already exists or a filesystem read/write fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Grey-door", GreyDoorPlmDrawDefinitions.All,
            GreyDoorPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock grey-door and shared clear-cap artwork before selecting an optional complete visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing the required version-one manifest and stock JSON.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent <see cref="VisualFileName"/> uses validated stock.</param>
    /// <returns>A ROM-independent catalog for all twenty layouts, preserving compiled door mechanics and geometry.</returns>
    /// <remarks>Stock must match its byte hash, supported-cartridge provenance, and compiled visual words even when an override exists. Override JSON needs all frame identities/shapes and visual-only words, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Stock integrity, provenance, JSON format, frame coverage, shapes, or visual word bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmGreyDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Grey-door", GreyDoorPlmDrawDefinitions.All,
            GreyDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmGreyDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmGreyDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    /// <summary>Runs stock manifest, hash, schema, and compiled-appearance checks without selecting overrides, reading a cartridge, or writing files.</summary>
    /// <param name="directory">Grey-door family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="Load"/>.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
