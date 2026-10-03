using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyTubeGeometry(SuperMetroidAddressSpace rom) => VerifyTubeDrawField(rom, 0);
    private static void VerifyTubeCollision(SuperMetroidAddressSpace rom) => VerifyTubeDrawField(rom, 1);
    private static void VerifyTubeVisuals(SuperMetroidAddressSpace rom) => VerifyTubeDrawField(rom, 2);

    private static void VerifyTubeDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x98d1,0x98d7,0x98dd,0x98e3,0x9953,0x9991,0x99e5];
        string[] ids = ["intact","damaged","opened","cleared","broken-late","opened-rows","broken-full"];
        var exported = NoobTubePlmDrawDefinitions.All.ToArray();
        AssertEqual(7, exported.Length, "Tube export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, NoobTubePlmDrawDefinitions.TryDescribe(pointer, out var shape), "Tube shape domain");
                AssertEqual(owned, NoobTubePlmDrawDefinitions.TryGet(pointer, out var dto), "Tube DTO domain");
                if (!owned)
                {
                    AssertEqual(default(NoobTubePlmDrawDefinitions.Draw), shape, "Tube missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Tube missing DTO");
                    AssertThrows<InvalidDataException>(() => NoobTubePlmDrawDefinitions.VisualId(pointer), "Tube missing ID");
                }
            }
            foreach (string id in new[] { "INTACT", "broken", "", "unknown" })
            {
                AssertTrue(!NoobTubePlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Tube ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Tube rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            NoobTubePlmDrawDefinitions.TryDescribe(pointer, out var shape);
            NoobTubePlmDrawDefinitions.TryGet(pointer, out var dto);
            NoobTubePlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Tube original export order");
                AssertEqual(ids[index], NoobTubePlmDrawDefinitions.VisualId(pointer), "Tube published ID");
                AssertEqual(pointer, byId.Pointer, "Tube reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "Tube run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "Tube offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Tube word run bounds");
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
                    AssertEqual((ushort)count, header, "Tube native horizontal run");
                    AssertEqual(count, shape.WordCount(run), "Tube native calculated count");
                    AssertEqual((ushort)(unchecked((byte)shape.NextY(run)) << 8), offset, "Tube native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "Tube native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Tube native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "Tube native DTO offset");
                    }
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Tube cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Tube original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Tube original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 5, "Tube native run termination");
            }
            AssertEqual(run, shape.RunCount, "Tube native run count");
            AssertEqual(run, dto.Runs.Length, "Tube native DTO run count");
        }
        AssertEqual(149, cells, "Tube independent physical cell count");
    }
}
