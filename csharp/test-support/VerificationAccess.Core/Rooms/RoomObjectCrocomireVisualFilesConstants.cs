
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectCrocomireVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectCrocomireVisualFilesConstants
{
    /// <summary>Checks stock arena provenance, digest, schema, five-layout coverage, and equality with compiled appearances.</summary>
    /// <param name="directory">Installed stock Crocomire arena directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectCrocomireVisualFiles.Load(directory);
}
