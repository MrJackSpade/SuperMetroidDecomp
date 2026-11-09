using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Compares one field of a PLM draw cell with the cartridge's block, decoded by the import codec: the layout checks
/// select the collision kind (field one), which the game's draw geometry holds, or the appearance (field two), which the
/// frame's installed picture holds (#1557), never a packed word.
/// </summary>
internal static class TerrainBlockFieldAssertions
{
    /// <summary>The layout checks' field number for the collision kind.</summary>
    internal const int CollisionField = 1;

    /// <summary>
    /// Asserts, for <paramref name="field"/> one, that <paramref name="collision"/> is the cartridge block's collision kind,
    /// and otherwise that the installed <paramref name="picture"/> shows the cartridge block's appearance at <paramref name="cell"/>.
    /// </summary>
    internal static void AssertCellField(int field, RoomTerrainBlock expected, RoomCollisionType collision, RoomObjectFrameImage picture,
        Offset16<RoomBlocks> cell, string context)
    {
        if (field == CollisionField)
            AssertEqual(expected.Collision, collision, $"{context}: collision kind");
        else
            AssertEqual(expected.Appearance, picture.At(cell), $"{context}: installed picture's appearance");
    }
}
