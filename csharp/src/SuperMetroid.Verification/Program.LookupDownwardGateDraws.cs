using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks downward-gate draw run counts, directions, cell widths, continuations, pointer ownership, and record boundaries.</summary>
    /// <param name="rom">Retail address space supplying the native bank-$84 draw records.</param>
    private static void VerifyDownwardGateDrawGeometry(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 0));

    /// <summary>Checks that calculated downward-gate collision flags match the high nibble of each native tile word.</summary>
    /// <param name="rom">Retail address space containing the native draw words.</param>
    private static void VerifyDownwardGateDrawCollision(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 1));

    /// <summary>Checks that calculated downward-gate tile identities match the low twelve bits of each native tile word.</summary>
    /// <param name="rom">Retail address space containing the native draw words.</param>
    private static void VerifyDownwardGateDrawVisuals(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 2));

    /// <summary>Compares all fourteen resident-frame and trigger draw lists against their native records for the selected field.</summary>
    /// <param name="rom">Retail address space supplying native bank-$84 draw words.</param>
    /// <param name="field">Zero checks geometry and ownership, one checks collision flags, and two checks visual tile identities.</param>
    private static void VerifyDownwardGateDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa517,0xa525,0xa533,0xa541,0xa54f,0xa55d,
            0xa5d7,0xa5e3,0xa5eb,0xa5f7,0xa5ff,0xa60b,0xa613,0xa61f];
        string[] ids = ["column-frame-0","column-frame-1","column-frame-2","column-frame-3",
            "column-frame-4","column-frame-5","blue-left-trigger","blue-right-trigger",
            "green-left-trigger","green-right-trigger","red-left-trigger","red-right-trigger",
            "yellow-left-trigger","yellow-right-trigger"];
        var exported = DownwardGatePlmDrawDefinitions.All.ToArray();
        AssertEqual(14, exported.Length, "Gate draw export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, DownwardGatePlmDrawDefinitions.TryDescribe(pointer, out var shape), "Gate descriptor ownership");
                AssertEqual(owned, DownwardGatePlmDrawDefinitions.TryGet(pointer, out var dto), "Gate DTO ownership");
                if (!owned)
                {
                    AssertEqual(default(DownwardGatePlmDrawDefinitions.Draw), shape, "Gate missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Gate missing DTO");
                    AssertThrows<InvalidDataException>(() => DownwardGatePlmDrawDefinitions.VisualId(pointer), "Gate missing ID");
                }
            }
            foreach (string invalid in new[] { "column-frame-6", "column-frame-00", "Blue-left-trigger", "", "unknown" })
            {
                AssertTrue(!DownwardGatePlmDrawDefinitions.TryGetByVisualId(invalid, out var missing), "Gate ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Gate missing ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            DownwardGatePlmDrawDefinitions.TryDescribe(pointer, out var shape);
            DownwardGatePlmDrawDefinitions.TryGet(pointer, out var dto);
            AssertEqual(pointer, exported[index].Pointer, "Gate export order");
            AssertEqual(ids[index], DownwardGatePlmDrawDefinitions.VisualId(pointer), "Gate published ID mapping");
            AssertTrue(DownwardGatePlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Gate ID ownership");
            AssertEqual(pointer, byId.Pointer, "Gate reverse ID mapping");
            int cursor = pointer, run = 0;
            while (true)
            {
                ushort count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor);
                cursor += 2;
                int length = count & 0x7fff;
                if (field == 0)
                {
                    AssertEqual(count, shape.DirectionAndCount(run), "Gate native count/direction");
                    AssertEqual(length, shape.WordCount(run), "Gate native run width");
                    AssertEqual((count & 0x8000) != 0, shape.Column, "Gate runtime direction");
                    AssertEqual(count, dto.Runs.Span[run].DirectionAndCount, "Gate DTO count/direction");
                    AssertEqual(count, exported[index].Runs.Span[run].DirectionAndCount, "Gate export count/direction");
                    AssertEqual(length, dto.Runs.Span[run].LevelWords.Length, "Gate DTO width");
                    AssertEqual(length, exported[index].Runs.Span[run].LevelWords.Length, "Gate export width");
                    foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(run, invalid), "Gate cell bounds");
                }
                for (int word = 0; word < length; word++, cursor += 2)
                {
                    if (field != 0)
                    {
                        int mask = field == 1 ? 0xf000 : 0xfff;
                        int expected = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & mask;
                        AssertEqual(expected, shape.WordAt(run, word) & mask, "Gate original calculated field");
                        AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[word] & mask, "Gate original DTO field");
                        AssertEqual(expected, exported[index].Runs.Span[run].LevelWords.Span[word] & mask, "Gate original export field");
                    }
                    cells++;
                }
                sbyte x = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                sbyte y = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                if (field == 0)
                {
                    AssertEqual(x, shape.NextX(run), "Gate original continuation X");
                    AssertEqual((sbyte)0, y, "Gate runtime continuation Y");
                    AssertEqual(x, dto.Runs.Span[run].NextX, "Gate DTO X");
                    AssertEqual(y, dto.Runs.Span[run].NextY, "Gate DTO Y");
                    AssertEqual(x, exported[index].Runs.Span[run].NextX, "Gate export X");
                    AssertEqual(y, exported[index].Runs.Span[run].NextY, "Gate export Y");
                }
                run++;
                if (x == 0 && y == 0) break;
            }
            if (field == 0)
            {
                AssertEqual(run, shape.RunCount, "Gate original run count");
                AssertEqual(run, dto.Runs.Length, "Gate DTO run count");
                AssertEqual(run, exported[index].Runs.Length, "Gate export run count");
                int end = index == 5 ? 0xa56b : index == 13 ? 0xa627 : pointers[index + 1];
                AssertEqual(end, cursor, "Gate original record end");
                foreach (int invalid in new[] { int.MinValue, -1, run, int.MaxValue })
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Gate run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "Gate offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.DirectionAndCount(invalid), "Gate direction bounds");
                }
            }
        }
        AssertEqual(46, cells, "Gate complete original cell domain");
    }
}
