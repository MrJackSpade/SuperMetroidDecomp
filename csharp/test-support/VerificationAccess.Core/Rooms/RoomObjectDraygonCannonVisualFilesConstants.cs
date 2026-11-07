
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectDraygonCannonVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectDraygonCannonVisualFilesConstants
{
    /// <summary>Checks stock cannon provenance, digest, schema, twelve-frame coverage, and equality with compiled appearances.</summary>
    /// <param name="directory">Installed stock Draygon cannon directory.</param>
    public static void ValidateStock(string directory) => _ = RoomObjectDraygonCannonVisualFiles.Load(directory);
}
