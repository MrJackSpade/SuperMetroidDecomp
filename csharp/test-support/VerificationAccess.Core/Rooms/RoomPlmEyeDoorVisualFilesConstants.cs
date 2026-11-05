
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmEyeDoorVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmEyeDoorVisualFilesConstants
{
    /// <summary>Checks stock eye-door provenance, digest, schema, twenty-three-frame coverage, and equality with compiled appearance.</summary>
    /// <param name="stockDirectory">Installed stock eye-door directory.</param>
    public static void ValidateStock(string stockDirectory) => RoomPlmEyeDoorVisualFiles.Load(stockDirectory);
}
