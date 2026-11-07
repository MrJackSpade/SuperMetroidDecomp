
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectElevatorPlatformVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectElevatorPlatformVisualFilesConstants
{
    /// <summary>Performs the stock checks from <see cref="RoomObjectElevatorPlatformVisualFiles.Load"/> without cartridge reads, or file writes.</summary>
    /// <param name="directory">Elevator-platform family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectElevatorPlatformVisualFiles.Load(directory);
}
