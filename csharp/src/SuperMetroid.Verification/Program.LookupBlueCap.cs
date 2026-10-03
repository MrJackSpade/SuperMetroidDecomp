using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBlueCapGeometry(SuperMetroidAddressSpace rom) => VerifyBlueCapField(rom, 0);
    private static void VerifyBlueCapCollision(SuperMetroidAddressSpace rom) => VerifyBlueCapField(rom, 1);
    private static void VerifyBlueCapVisuals(SuperMetroidAddressSpace rom) => VerifyBlueCapField(rom, 2);

    private static void VerifyBlueCapField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa9b3,0xa9bf,0xa9cb,0xa9d7,0xa9ef,0xa9fb,0xaa07,0xaa13,0xaa2b,0xaa37,0xaa43,0xaa4f,0xaa67,0xaa73,0xaa7f,0xaa8b,0xa9a7,0xa9e3,0xaa1f,0xaa5b];
        string[] ids = ["left-frame-0","left-frame-1","left-frame-2","left-frame-3","right-frame-0","right-frame-1","right-frame-2","right-frame-3","up-frame-0","up-frame-1","up-frame-2","up-frame-3","down-frame-0","down-frame-1","down-frame-2","down-frame-3","left-frame-0","right-frame-0","up-frame-0","down-frame-0"];
        var exported = BlueDoorPlmDrawDefinitions.All.ToArray();
        AssertEqual(20, exported.Length, "Blue cap export count");
        AssertTrue(BlueDoorPlmDrawDefinitions.Editable.Select(frame => frame.Pointer).SequenceEqual(pointers.Take(16)), "Blue cap editable identity domain and order");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                ushort alias = pointer switch { 0xa9a7 => 0xa9b3, 0xa9e3 => 0xa9ef, 0xaa1f => 0xaa2b, 0xaa5b => 0xaa67, _ => pointer };
                AssertEqual(alias, BlueDoorPlmDrawDefinitions.VisualSource(pointer), "Blue cap alias and passthrough domain");
                AssertEqual(owned, BlueDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Blue cap descriptor domain");
                AssertEqual(owned, BlueDoorPlmDrawDefinitions.TryGet(pointer, out var dto), "Blue cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(BlueDoorPlmDrawDefinitions.Draw), shape, "Blue cap missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Blue cap missing DTO");
                    AssertThrows<InvalidDataException>(() => BlueDoorPlmDrawDefinitions.VisualId(pointer), "Blue cap missing ID");
                }
            }
            foreach (string id in new[] { "LEFT-FRAME-0", "left-frame-4", "", "unknown" })
            {
                AssertTrue(!BlueDoorPlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Blue cap ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Blue cap missing ID output");
            }
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            BlueDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            BlueDoorPlmDrawDefinitions.TryGet(pointer, out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Blue cap original order");
                AssertEqual(ids[index], BlueDoorPlmDrawDefinitions.VisualId(pointer), "Blue cap published ID");
                AssertTrue(BlueDoorPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Blue cap reverse ID");
                AssertEqual(index < 16 ? pointer : pointers[(index - 16) * 4], byId.Pointer, "Blue cap ID identity");
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), shape.DirectionAndCount, "Blue cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 10)), "Blue cap native final offset");
                foreach (var frame in new[] {dto,exported[index]})
                {
                    AssertEqual(1, frame.Runs.Length, "Blue cap one run");
                    AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), frame.Runs.Span[0].DirectionAndCount, "Blue cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].LevelWords.Length, "Blue cap DTO width");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextX, "Blue cap DTO final X");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextY, "Blue cap DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid), "Blue cap calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                int mask = field == 1 ? 0xf000 : 0xfff;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 2 + row * 2)) & mask;
                AssertEqual(expected, shape.WordAt(row) & mask, "Blue cap original calculated field");
                AssertEqual(expected, dto.Runs.Span[0].LevelWords.Span[row] & mask, "Blue cap original DTO field");
                AssertEqual(expected, exported[index].Runs.Span[0].LevelWords.Span[row] & mask, "Blue cap original export field");
            }
        }
    }
}
