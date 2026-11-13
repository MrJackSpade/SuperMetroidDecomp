using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks a frame's block shape (#1608) against the cells the cartridge's draw list writes.</summary>
internal static class RoomBlockShapeAssertions
{
    /// <summary>
    /// Checks that <paramref name="shape"/> covers exactly the cells the cartridge's draw list at <paramref name="pointer"/>
    /// writes, each collides as the cartridge's does, the list writes no cell twice (so the order the shape is drawn in
    /// is unobservable), and a cell beyond the shape is not covered.
    /// </summary>
    /// <param name="rom">The cartridge.</param>
    /// <param name="shape">The frame's block shape.</param>
    /// <param name="pointer">Where the cartridge stores the frame's draw list.</param>
    /// <param name="name">The frame's published name, for messages.</param>
    internal static void AssertShapeAsNative(CartridgeImportAddressSpace rom, RoomBlockShape shape, int pointer, string name)
    {
        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> native = NativeTerrain(rom, pointer, out int written);
        AssertEqual(native.Count, written, $"{name} draw list writes each cell once");
        AssertEqual(native.Count, shape.Cells.Count, $"{name} shape covers as many cells as its draw list writes");
        foreach (KeyValuePair<Offset16<RoomBlocks>, RoomTerrainBlock> cell in native)
        {
            AssertTrue(shape.Covers(cell.Key), $"{name} shape covers its draw list's cell {cell.Key}");
            AssertEqual(cell.Value.Collision, shape.CollisionAt(cell.Key), $"{name} shape cell {cell.Key} collision");
        }
        Offset16<RoomBlocks> beyond = new(shape.Cells.Max(cell => cell.Cell.X) + 1, 0);
        AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(beyond), $"{name} shape covers only its cells");
    }
}
