using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks the 20 station visual IDs and their order, exact reverse lookup behavior, and rejection of unsupported IDs and pointers.</summary>
    private static void VerifyStationVisualIds()
    {
        // Original exported identity contract, independent of the replacement naming code.
        (ushort Pointer, string Id)[] expected =
        [
            (0x9f25,"map-frame-0"),(0x9f6d,"energy-frame-0"),(0x9f91,"missile-frame-0"),
            (0x9f31,"map-frame-1"),(0x9f79,"energy-frame-1"),(0x9f9d,"missile-frame-1"),
            (0x9f3d,"map-frame-2"),(0x9f85,"energy-frame-2"),(0x9fa9,"missile-frame-2"),
            (0x9a3f,"save-idle"),(0x9a9f,"save-active-a"),(0x9a6f,"save-active-b"),
            (0x9f49,"map-right-retracted"),(0x9f55,"map-right-extended"),
            (0x9f5b,"map-left-retracted"),(0x9f67,"map-left-extended"),
            (0x9fb5,"resource-right-retracted"),(0x9fbb,"resource-right-extended"),
            (0x9fc1,"resource-left-retracted"),(0x9fc7,"resource-left-extended"),
        ];
        var exported = RoomPlmStationDrawDefinitions.All.ToArray();
        AssertEqual(20, exported.Length, "Station original artwork identity count");
        for (int index = 0; index < expected.Length; index++)
        {
            var entry = expected[index];
            AssertEqual(entry.Pointer, exported[index].Pointer, "Station original export order");
            AssertEqual(entry.Id, RoomPlmStationDrawDefinitions.VisualId(entry.Pointer), "Station original published ID");
            AssertTrue(RoomPlmStationDrawDefinitions.TryGetByVisualId(entry.Id, out var resolved), "Station original reverse identity");
            AssertEqual(exported[index].Pointer, resolved.Pointer, "Station reverse identity selects the complete original draw");
            foreach (string invalid in new[] { entry.Id.ToUpperInvariant(), " " + entry.Id, entry.Id + " " })
            {
                AssertTrue(!RoomPlmStationDrawDefinitions.TryGetByVisualId(invalid, out var missing), "Station IDs remain ordinal and exact");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Station rejected ID output");
            }
        }
        foreach (string invalid in new[] { "", "map-frame-3", "energy-frame--1", "missile-frame-01", "save-active-c" })
            AssertTrue(!RoomPlmStationDrawDefinitions.TryGetByVisualId(invalid, out _), "Station unsupported named frame");
        AssertTrue(!RoomPlmStationDrawDefinitions.TryGetByVisualId(null!, out _), "Station null ID remains rejected");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!expected.Any(entry => entry.Pointer == raw))
                AssertThrows<InvalidDataException>(() => RoomPlmStationDrawDefinitions.VisualId((ushort)raw), "Station full unsupported pointer domain");
    }
}
