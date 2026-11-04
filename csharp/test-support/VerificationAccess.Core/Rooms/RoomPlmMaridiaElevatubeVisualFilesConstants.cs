
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmMaridiaElevatubeVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmMaridiaElevatubeVisualFilesConstants
{
    /// <summary>Runs the elevatube stock checks from <see cref="RoomPlmMaridiaElevatubeVisualFiles.Load"/> with no override, cartridge reads, or writes.</summary>
    /// <param name="directory">Family directory containing elevatube stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = RoomPlmMaridiaElevatubeVisualFiles.Load(directory, null);
}
