using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks glass-pane run counts, directions, continuation offsets, and word positions against native layout data.</summary>
    private static void VerifyGlassLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 0);

    /// <summary>Checks the collision-related bits of every exported glass-pane block against the native PLM stream.</summary>
    private static void VerifyGlassLayoutCollision(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 1);

    /// <summary>Checks the visual tile bits of every glass-pane layout against the native PLM stream.</summary>
    private static void VerifyGlassLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 2);

    /// <summary>Compares one glass-layout field with native PLM words across all authored frame pointers.</summary>
    /// <param name="rom">Cartridge address space containing the native glass-pane PLM records.</param>
    /// <param name="field">Selector for geometry (0), collision bits (1), or visual tile bits (2).</param>
    private static void VerifyGlassLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9717,0x971d,0x9731,0x9745,0x974f,0x9769,0x9781,0x978f,0x97b7,0x97e7,0x9817];
        string[] ids = ["initial","pane-damage-1","pane-damage-2","pane-transition","shifted-pane-1","shifted-pane-2","shifted-pane-3","shatter-1","shatter-2","shatter-3","cleared"];
        var exported = MotherBrainGlassPlmDrawDefinitions.All.ToArray();
        AssertEqual(11, exported.Length, "Glass layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, MotherBrainGlassPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Glass layout shape domain");
                AssertEqual(owned, MotherBrainGlassPlmDrawDefinitions.TryGet(pointer, out var dto), "Glass layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(MotherBrainGlassPlmDrawDefinitions.Draw), shape, "Glass layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Glass layout missing DTO");
                    AssertThrows<InvalidDataException>(() => MotherBrainGlassPlmDrawDefinitions.VisualId(pointer), "Glass layout missing ID");
                }
            }
            foreach (string id in new[] { "INTACT", "broken", "", "unknown" })
            {
                AssertTrue(!MotherBrainGlassPlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Glass layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Glass layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            MotherBrainGlassPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            MotherBrainGlassPlmDrawDefinitions.TryGet(pointer, out var dto);
            MotherBrainGlassPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Glass layout original export order");
                AssertEqual(ids[index], MotherBrainGlassPlmDrawDefinitions.VisualId(pointer), "Glass layout published ID");
                AssertEqual(pointer, byId.Pointer, "Glass layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "Glass layout run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "Glass layout offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Glass layout word run bounds");
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
                    AssertEqual((ushort)(count | (shape.Vertical(run) ? 0x8000 : 0)), header, "Glass layout native direction");
                    AssertEqual(count, shape.WordCount(run), "Glass layout native calculated count");
                    AssertEqual((ushort)((byte)shape.NextX(run) | (byte)shape.NextY(run) << 8), offset, "Glass layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "Glass layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Glass layout native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "Glass layout native DTO offset");
                    }
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Glass layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Glass layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Glass layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 5, "Glass layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Glass layout native run count");
            AssertEqual(run, dto.Runs.Length, "Glass layout native DTO run count");
        }
        AssertEqual(92, cells, "Glass layout independent physical cell count");
    }
}
