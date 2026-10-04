using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyMapSpriteNameCases()
    {
        // Independent original table from9b54e512, retained only for verification.
        (ushort NativeId, string Name)[] original =
        [
            (0x4, "Arrow.Right"),
            (0x5, "Arrow.Left"),
            (0x6, "Arrow.Up"),
            (0x7, "Arrow.Down"),
            (0x9, "Marker.Boss"),
            (0xa, "Station.Energy"),
            (0xb, "Station.Missile"),
            (0x4e, "Station.Map"),
            (0x62, "Marker.DefeatedBoss"),
            (0x63, "Marker.Gunship"),
            (0x12, "Indicator.Backing"),
            (0x5f, "Indicator.Frame0"),
            (0x60, "Indicator.Frame1"),
            (0x61, "Indicator.Frame2"),
            (0x59, "Elevator.Crateria"),
            (0x5a, "Elevator.Brinstar"),
            (0x5b, "Elevator.Norfair"),
            (0x5c, "Elevator.WreckedShip"),
            (0x5d, "Elevator.Maridia"),
            (0x38, "World.Title"),
            (0x39, "World.Crateria"),
            (0x3a, "World.Brinstar"),
            (0x3b, "World.Norfair"),
            (0x3c, "World.WreckedShip"),
            (0x3d, "World.Maridia"),
            (0x3e, "World.Tourian")
        ];
        AssertEqual(original.Length, MapSpriteDefinitions.Count, "map sprite complete role count");
        AssertTrue(original.SequenceEqual(MapSpriteDefinitions.Frames), "original map sprite document order");
        foreach (var entry in original)
            AssertEqual(entry.Name, MapSpriteDefinitions.Name(entry.NativeId), "original native identity name");
        var supported = original.Select(entry => entry.NativeId).ToHashSet();
        for (int id = 0; id <= ushort.MaxValue; id++)
            AssertEqual(supported.Contains((ushort)id), MapSpriteDefinitions.Contains((ushort)id), "complete map sprite membership domain");
        foreach (ushort invalid in new ushort[] { 0, 3, 8, 12, 0x37, 0x3f, 0x58, 0x5e, 0x64, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteDefinitions.Name(invalid), "unsupported map sprite name");
    }
}
