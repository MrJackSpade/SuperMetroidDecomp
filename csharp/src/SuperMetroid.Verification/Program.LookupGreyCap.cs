using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGreyCapGeometry(SuperMetroidAddressSpace rom) => VerifyGreyCapField(rom, 0);
    private static void VerifyGreyCapCollision(SuperMetroidAddressSpace rom) => VerifyGreyCapField(rom, 1);
    private static void VerifyGreyCapVisuals(SuperMetroidAddressSpace rom) => VerifyGreyCapField(rom, 2);

    private static void VerifyGreyCapField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa677,0xa683,0xa68f,0xa69b,0xa6a7,0xa6b3,0xa6bf,0xa6cb,0xa6d7,0xa6e3,0xa6ef,0xa6fb,0xa707,0xa713,0xa71f,0xa72b,0xa737,0xa743,0xa74f,0xa75b];
        string[] ids = ["clear-left","clear-right","clear-up","clear-down","grey-left-frame-0","grey-left-frame-1","grey-left-frame-2","grey-left-frame-3","grey-right-frame-0","grey-right-frame-1","grey-right-frame-2","grey-right-frame-3","grey-up-frame-0","grey-up-frame-1","grey-up-frame-2","grey-up-frame-3","grey-down-frame-0","grey-down-frame-1","grey-down-frame-2","grey-down-frame-3"];
        var exported = GreyDoorPlmDrawDefinitions.All.ToArray();
        AssertEqual(20, exported.Length, "Grey cap export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, GreyDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Grey cap descriptor domain");
                AssertEqual(owned, GreyDoorPlmDrawDefinitions.TryGet(pointer, out var dto), "Grey cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(GreyDoorPlmDrawDefinitions.Draw), shape, "Grey cap missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Grey cap missing DTO");
                    AssertThrows<InvalidDataException>(() => GreyDoorPlmDrawDefinitions.VisualId(pointer), "Grey cap missing ID");
                }
            }
            foreach (string id in new[] { "CLEAR-LEFT", "grey-left-frame-4", "", "unknown" })
            {
                AssertTrue(!GreyDoorPlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Grey cap ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Grey cap missing ID output");
            }
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            GreyDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            GreyDoorPlmDrawDefinitions.TryGet(pointer, out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Grey cap original order");
                AssertEqual(ids[index], GreyDoorPlmDrawDefinitions.VisualId(pointer), "Grey cap published ID");
                AssertTrue(GreyDoorPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Grey cap reverse ID");
                AssertEqual(pointer, byId.Pointer, "Grey cap ID identity");
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), shape.DirectionAndCount, "Grey cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 10)), "Grey cap native final offset");
                foreach (var frame in new[] {dto,exported[index],byId})
                {
                    AssertEqual(1, frame.Runs.Length, "Grey cap one run");
                    AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), frame.Runs.Span[0].DirectionAndCount, "Grey cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].LevelWords.Length, "Grey cap DTO width");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextX, "Grey cap DTO final X");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextY, "Grey cap DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid), "Grey cap calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                int mask = field == 1 ? 0xf000 : 0xfff;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 2 + row * 2)) & mask;
                AssertEqual(expected, shape.WordAt(row) & mask, "Grey cap original calculated field");
                AssertEqual(expected, dto.Runs.Span[0].LevelWords.Span[row] & mask, "Grey cap original DTO field");
                AssertEqual(expected, exported[index].Runs.Span[0].LevelWords.Span[row] & mask, "Grey cap original export field");
            }
        }
    }
}
