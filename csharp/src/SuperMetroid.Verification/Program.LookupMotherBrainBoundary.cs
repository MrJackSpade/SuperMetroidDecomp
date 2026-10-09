using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks that the two exported Mother Brain boundary records match native PLM runs, ownership, offsets, and record boundaries.</summary>
    /// <param name="rom">Cartridge address space containing the native room PLM data.</param>
    private static void VerifyMotherBrainBoundaryGeometry(SuperMetroidAddressSpace rom)
    {
        var exports = MotherBrainFakeDeathPlmDrawDefinitions.All.Take(2).ToArray();
        ushort[] pointers = [0x94a3,0x94b1];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int index = Array.IndexOf(pointers, (ushort)raw);
            AssertEqual(index >= 0, MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary((ushort)raw, out var shape),
                "Mother Brain wall/door ownership");
            if (index < 0)
            {
                AssertEqual(default(MotherBrainFakeDeathPlmDrawDefinitions.BoundaryDraw), shape, "Mother Brain missing wall/door descriptor");
                continue;
            }
            AssertEqual((ushort)raw, exports[index].Pointer, "Mother Brain boundary export order");
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet((ushort)raw, out var dto);
            int cursor = raw;
            for (int run = 0; run < 2; run++)
            {
                ushort direction = ReadSamusEaterPlmWord(rom, 0x840000 | cursor);
                int count = direction & 0x7fff;
                AssertEqual((ushort)(0x8000 | shape.Count(run)), direction, "Mother Brain vertical boundary run");
                cursor += 2 + count * 2;
                sbyte x = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                sbyte y = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                AssertEqual(x, shape.NextX(run), "Mother Brain native boundary X");
                AssertEqual(y, shape.NextY(run), "Mother Brain native boundary Y");
                foreach (var draw in new[] {dto,exports[index]})
                {
                    AssertEqual(2, draw.Runs.Length, "Mother Brain two boundary runs");
                    AssertEqual(direction, draw.Runs.Span[run].DirectionAndCount, "Mother Brain DTO direction/count");
                    AssertEqual(count, draw.Runs.Span[run].LevelWords.Length, "Mother Brain DTO run width");
                    AssertEqual(x, draw.Runs.Span[run].NextX, "Mother Brain DTO X");
                    AssertEqual(y, draw.Runs.Span[run].NextY, "Mother Brain DTO Y");
                }
                foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(run,bad), "Mother Brain boundary cell bounds");
            }
            AssertEqual(index == 0 ? 0x94b1 : 0x94c9, cursor, "Mother Brain exact boundary record end");
            foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
            {
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(bad,0), "Mother Brain boundary run bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextX(bad), "Mother Brain boundary X bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextY(bad), "Mother Brain boundary Y bounds");
            }
        }
    }

    /// <summary>Verifies the collision-relevant high bits of every Mother Brain wall and door boundary cell.</summary>
    /// <param name="rom">Cartridge address space used as the native cell-data reference.</param>
    private static void VerifyMotherBrainBoundaryCollision(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 0));

    /// <summary>Checks the visual cell values in the left wall boundary PLM record.</summary>
    /// <param name="rom">Cartridge address space used as the native cell-data reference.</param>
    private static void VerifyMotherBrainWallVisuals(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 1));

    /// <summary>Checks the visual cell values in the right door boundary PLM record.</summary>
    /// <param name="rom">Cartridge address space used as the native cell-data reference.</param>
    private static void VerifyMotherBrainDoorVisuals(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 2));

    /// <summary>Compares selected native boundary-cell fields with both the runtime geometry and exported draw descriptor.</summary>
    /// <param name="rom">Cartridge address space containing the native boundary records.</param>
    /// <param name="field">Field selector: zero checks collision bits; one checks the wall's visual word; two checks the door's visual word.</param>
    private static void VerifyMotherBrainBoundaryCells(SuperMetroidAddressSpace rom, int field)
    {
        foreach (ushort pointer in new ushort[] {0x94a3,0x94b1})
        {
            if (field == 1 && pointer != 0x94a3 || field == 2 && pointer != 0x94b1) continue;
            MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(pointer, out var shape);
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(pointer, out var dto);
            int cursor = pointer;
            int mask = field == 0 ? 0xf000 : 0xfff;
            for (int run = 0; run < 2; run++)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                for (int block = 0; block < count; block++, cursor += 2)
                {
                    int expected = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & mask;
                    AssertEqual(expected, shape.WordAt(run,block) & mask, "Mother Brain native boundary field");
                    AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[block] & mask, "Mother Brain DTO boundary field");
                }
                cursor += 2;
            }
        }
    }
}
