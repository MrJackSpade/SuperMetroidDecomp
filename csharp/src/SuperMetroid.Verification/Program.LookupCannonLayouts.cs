using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCannonLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyCannonLayoutDrawField(rom, 0);
    private static void VerifyCannonLayoutCollision(SuperMetroidAddressSpace rom) => VerifyCannonLayoutDrawField(rom, 1);
    private static void VerifyCannonLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyCannonLayoutDrawField(rom, 2);

    private static void VerifyCannonLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9fcd,0x9fdd,0xa02d,0xa03d,0xa04d,0xa05d,0xa0ed,0xa101,0xa165,0xa179,0xa18d,0xa1a1];
        string[] ids = ["right-shield-a","right-shield-b","right-damaged-a","right-damaged-b","right-damaged-c","right-damaged-d","left-shield-a","left-shield-b","left-damaged-a","left-damaged-b","left-damaged-c","left-damaged-d"];
        var exported = DraygonCannonPlmDrawDefinitions.All.ToArray();
        AssertEqual(12, exported.Length, "Cannon layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, DraygonCannonPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Cannon layout shape domain");
                AssertEqual(owned, DraygonCannonPlmDrawDefinitions.TryGet(pointer, out var dto), "Cannon layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(DraygonCannonPlmDrawDefinitions.Draw), shape, "Cannon layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Cannon layout missing DTO");
                    AssertThrows<InvalidDataException>(() => DraygonCannonPlmDrawDefinitions.VisualId(pointer), "Cannon layout missing ID");
                }
            }
            foreach (string id in new[] { "RIGHT-SHIELD-A", "broken", "", "unknown" })
            {
                AssertTrue(!DraygonCannonPlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Cannon layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Cannon layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            DraygonCannonPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            DraygonCannonPlmDrawDefinitions.TryGet(pointer, out var dto);
            DraygonCannonPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Cannon layout original export order");
                AssertEqual(ids[index], DraygonCannonPlmDrawDefinitions.VisualId(pointer), "Cannon layout published ID");
                AssertEqual(pointer, byId.Pointer, "Cannon layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "Cannon layout run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "Cannon layout offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Cannon layout word run bounds");
                }
            }
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterPlmWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Cannon layout native direction");
                    AssertEqual(count, shape.WordCount(run), "Cannon layout native calculated count");
                    AssertEqual((ushort)((byte)shape.NextX(run) | (byte)shape.NextY(run) << 8), offset, "Cannon layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "Cannon layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Cannon layout native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "Cannon layout native DTO offset");
                    }
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Cannon layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Cannon layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Cannon layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 5, "Cannon layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Cannon layout native run count");
            AssertEqual(run, dto.Runs.Length, "Cannon layout native DTO run count");
        }
        AssertEqual(48, cells, "Cannon layout independent physical cell count");
    }
}
