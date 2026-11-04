
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmElevatorPlatformVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmElevatorPlatformVisualFilesConstants
{
    /// <summary>Performs the stock checks from <see cref="RoomPlmElevatorPlatformVisualFiles.Load"/> without overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Elevator-platform family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmElevatorPlatformVisualFiles.Load(directory, null);
}
