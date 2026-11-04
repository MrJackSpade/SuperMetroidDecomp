
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmCrocomireVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmCrocomireVisualFilesConstants
{
    /// <summary>Checks stock arena provenance, digest, schema, five-layout coverage, and equality with compiled appearances; ignores overrides.</summary>
    /// <param name="directory">Installed stock Crocomire arena directory.</param>
    public static void ValidateStock(string directory) => _ = RoomPlmCrocomireVisualFiles.Load(directory, null);
}
