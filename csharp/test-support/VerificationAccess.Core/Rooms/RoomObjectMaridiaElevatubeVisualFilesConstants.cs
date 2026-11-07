
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectMaridiaElevatubeVisualFiles"/> that only verification reads.</summary>
internal static class RoomObjectMaridiaElevatubeVisualFilesConstants
{
    /// <summary>Runs the elevatube stock checks from <see cref="RoomObjectMaridiaElevatubeVisualFiles.Load"/> with no cartridge reads, or writes.</summary>
    /// <param name="directory">Family directory containing elevatube stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomObjectMaridiaElevatubeVisualFiles.Load(directory);
}
