using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals.</summary>
public static class RoomPlmLinkedRestoreVisualFiles
{
    /// <summary>Family-relative JSON filename for the six bomb/contact-crumble horizontal, vertical, and square restoration layouts.</summary>
    public const string VisualFileName = "linked-restores.json";

    /// <summary>Verifies the native bank-$84 linked-restore draw data and exports visible block/flip words while keeping bomb and contact-crumble mechanics compiled.</summary>
    /// <param name="bus">Cartridge source for all three bomb and three contact-crumble restoration draws.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank provenance hash supplied by the importer, not recomputed here; stock loading requires the supported cartridge identity.</param>
    /// <remarks>Visual words are flattened in native run/cell order. Files use create-new semantics, and a partial JSON/manifest write is not rolled back.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native run shapes, physical words, or offsets do not match compiled definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Linked restore", RoomPlmLinkedRestoreDrawDefinitions.All,
            RoomPlmLinkedRestoreDrawDefinitions.VisualId);

    /// <summary>Validates the shared stock restoration artwork and selects an optional complete six-layout visual replacement.</summary>
    /// <param name="stockDirectory">Family directory with required stock JSON and version-one manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; an absent override JSON or null uses validated stock.</param>
    /// <returns>A ROM-independent appearance catalog; distinct bomb/contact collision classes and restoration programs remain unchanged.</returns>
    /// <remarks>Stock provenance, byte hash, and compiled visual equality are mandatory before any override is read. Overrides need complete identities/shapes and twelve-bit visual words, not a manifest or physical level words.</remarks>
    /// <exception cref="InvalidDataException">Stock integrity/provenance, JSON version, coverage, identity, shape, or visual-only words are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmLinkedRestoreVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Linked restore", RoomPlmLinkedRestoreDrawDefinitions.All,
            RoomPlmLinkedRestoreDrawDefinitions.VisualId,
            entries => new RoomPlmLinkedRestoreVisualCatalog(entries.Select(entry =>
                new RoomPlmLinkedRestoreVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Checks only installed stock restoration visuals through <see cref="Load"/>, without overrides, cartridge access, or writes.</summary>
    /// <param name="directory">Linked-restoration family directory with stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
