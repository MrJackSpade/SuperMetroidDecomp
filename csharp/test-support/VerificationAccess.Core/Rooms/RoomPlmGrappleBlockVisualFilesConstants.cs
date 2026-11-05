
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmGrappleBlockVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmGrappleBlockVisualFilesConstants
{
    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="RoomPlmGrappleBlockVisualFiles.Load"/> without cartridge access, or writes.</summary>
    /// <param name="directory">Grapple-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmGrappleBlockVisualFiles.Load(directory);
}
