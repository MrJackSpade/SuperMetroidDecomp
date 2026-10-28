using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// Compares a compiled PLM run with the header word the cartridge stores before it (#1452). Core runs hold a direction
/// and their level words; the checks decode the cartridge header through the import codec and assert the direction and
/// the block count it states, never the packed word.
/// </summary>
internal static class PlmRunHeaderAssertions
{
    /// <summary>Asserts that <paramref name="direction"/> and <paramref name="count"/> are those <paramref name="nativeHeader"/> states.</summary>
    internal static void AssertRunHeader(int nativeHeader, PlmRunDirection direction, int count, string context)
    {
        AssertEqual(PlmRunHeaderCodecConstants.Direction(nativeHeader), direction, $"{context}: direction");
        AssertEqual(PlmRunHeaderCodecConstants.Count(nativeHeader), count, $"{context}: block count");
    }

    /// <summary>Asserts that <paramref name="run"/> has the direction and block count <paramref name="nativeHeader"/> states.</summary>
    internal static void AssertRunHeader(int nativeHeader, RoomPlmShotBlockDrawDefinitions.Run run, string context) =>
        AssertRunHeader(nativeHeader, run.Direction, run.Count, context);
}
