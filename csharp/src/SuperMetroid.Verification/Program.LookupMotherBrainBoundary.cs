using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
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

    private static void VerifyMotherBrainBoundaryCollision(SuperMetroidAddressSpace rom) =>
        VerifyMotherBrainBoundaryCells(rom, false);

    private static void VerifyMotherBrainWallVisuals(SuperMetroidAddressSpace rom) =>
        VerifyMotherBrainBoundaryCells(rom, true);

    private static void VerifyMotherBrainBoundaryCells(SuperMetroidAddressSpace rom, bool wallVisual)
    {
        foreach (ushort pointer in new ushort[] {0x94a3,0x94b1})
        {
            if (wallVisual && pointer != 0x94a3) continue;
            MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(pointer, out var shape);
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(pointer, out var dto);
            int cursor = pointer;
            int mask = wallVisual ? 0xfff : 0xf000;
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
