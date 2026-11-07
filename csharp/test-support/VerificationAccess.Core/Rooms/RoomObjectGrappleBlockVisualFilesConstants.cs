
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectGrappleBlockVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectGrappleBlockVisualFilesConstants
{
    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="RoomObjectGrappleBlockVisualFiles.Load"/> without cartridge access, or writes.</summary>
    /// <param name="directory">Grapple-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectGrappleBlockVisualFiles.Load(directory);
}
