
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmLinkedRestoreVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmLinkedRestoreVisualFilesConstants
{
    /// <summary>Checks only installed stock restoration visuals through <see cref="RoomPlmLinkedRestoreVisualFiles.Load"/>, without cartridge access, or writes.</summary>
    /// <param name="directory">Linked-restoration family directory with stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmLinkedRestoreVisualFiles.Load(directory);
}
