
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmNoobTubeVisualFiles"/> that only verification reads.</summary>
internal static class RoomPlmNoobTubeVisualFilesConstants
{
    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled n00b-tube visuals.</summary>
    /// <param name="stockDirectory">Stock n00b-tube directory.</param>
    public static void ValidateStock(string stockDirectory) => RoomPlmNoobTubeVisualFiles.Load(stockDirectory);
}
