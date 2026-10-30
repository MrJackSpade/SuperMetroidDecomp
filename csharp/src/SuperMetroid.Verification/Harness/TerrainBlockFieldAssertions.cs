using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Compares one field of a compiled PLM draw cell with the cartridge's block, decoded by the import codec: the layout
/// checks select the collision kind (field one) or the appearance (field two), never a packed word.
/// </summary>
internal static class TerrainBlockFieldAssertions
{
    /// <summary>The layout checks' field number for the collision kind.</summary>
    internal const int CollisionField = 1;

    /// <summary>Asserts the collision kind (<paramref name="field"/> one) or the appearance (otherwise) of two blocks agree.</summary>
    internal static void AssertTerrainField(int field, RoomTerrainBlock expected, RoomTerrainBlock actual, string context)
    {
        if (field == CollisionField)
            AssertEqual(expected.Collision, actual.Collision, $"{context}: collision kind");
        else
            AssertEqual(expected.Appearance, actual.Appearance, $"{context}: appearance");
    }
}
