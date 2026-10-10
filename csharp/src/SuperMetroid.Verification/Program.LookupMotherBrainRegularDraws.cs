using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies the eight regular Mother Brain draw layouts against bank-$84, including pointer ownership, all 63 cells, and native run continuations.</summary>
    /// <param name="rom">Cartridge address space containing the native Mother Brain draw lists.</param>
    private static void VerifyMotherBrainRegularDrawMapping(ISnesAddressSpace rom)
    {
        ushort[] pointers = [0x966d, 0x968b, 0x96a9, 0x96b1, 0x96bf, 0x96cb, 0x96ef, 0x9703];
        ushort[] otherPointers = [0x94a3,0x94b1,0x9505,0x9523,0x9541,0x955f,0x957d,0x959b,0x95b9,0x95d7,0x95f5,0x9613,0x9631,0x964f];
        var exported = MotherBrainFakeDeathPlmDrawDefinitions.All.Where(draw => pointers.Contains(draw.Pointer)).ToArray();
        AssertEqual(pointers.Length, exported.Length, "Mother Brain regular physical export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool owned = Array.IndexOf(pointers, pointer) >= 0;
            AssertEqual(owned, MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeRegular(pointer, out var shape),
                $"Mother Brain regular descriptor ownership {value:X4}");
            AssertEqual(owned || otherPointers.Contains(pointer), MotherBrainFakeDeathPlmDrawDefinitions.TryGet(pointer, out var dto),
                $"Mother Brain regular DTO ownership {value:X4}");
            if (!owned)
            {
                AssertEqual(default(MotherBrainFakeDeathPlmDrawDefinitions.RegularDraw), shape, "Mother Brain regular missing descriptor");
                if (!otherPointers.Contains(pointer))
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Mother Brain regular missing DTO");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            AssertEqual(pointer, exported[index].Pointer, "Mother Brain regular export order");
            MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeRegular(pointer, out var shape);
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(pointer, out var dto);
            int cursor = pointer, run = 0;
            while (true)
            {
                ushort count = ReadWord(cursor);
                cursor += 2;
                AssertEqual(count, (ushort)(shape.Count(run) | (shape.Vertical(run) ? 0x8000 : 0)), "Mother Brain regular native direction/count");
                AssertEqual(count, dto.Runs.Span[run].DirectionAndCount, "Mother Brain regular DTO direction/count");
                AssertEqual(count, exported[index].Runs.Span[run].DirectionAndCount, "Mother Brain regular export direction/count");
                int length = count & 0x7fff;
                AssertEqual(length, shape.Count(run), "Mother Brain regular native width");
                AssertEqual(length, dto.Runs.Span[run].LevelWords.Length, "Mother Brain regular DTO width");
                AssertEqual(length, exported[index].Runs.Span[run].LevelWords.Length, "Mother Brain regular export width");
                AssertEqual((count & 0x8000) != 0, shape.Vertical(run), "Mother Brain regular runtime direction");
                for (int block = 0; block < length; block++, cursor += 2)
                {
                    ushort expected = ReadWord(cursor);
                    AssertEqual(expected, shape.WordAt(run, block), "Mother Brain regular native calculated cell");
                    AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[block], "Mother Brain regular native DTO cell");
                    AssertEqual(expected, exported[index].Runs.Span[run].LevelWords.Span[block], "Mother Brain regular native export cell");
                    cells++;
                }
                sbyte nextX = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                sbyte nextY = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                AssertEqual(nextX, shape.NextX(run), "Mother Brain regular native origin-relative X");
                AssertEqual(nextX, dto.Runs.Span[run].NextX, "Mother Brain regular DTO X");
                AssertEqual(nextY, dto.Runs.Span[run].NextY, "Mother Brain regular DTO Y");
                AssertEqual(nextX, exported[index].Runs.Span[run].NextX, "Mother Brain regular export X");
                AssertEqual(nextY, exported[index].Runs.Span[run].NextY, "Mother Brain regular export Y");
                AssertEqual((sbyte)0, nextY, "Mother Brain regular runtime column origin Y");
                foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(run, invalid), "Mother Brain regular cell bounds");
                run++;
                if (nextX == 0 && nextY == 0) break;
            }
            AssertEqual(run, shape.RunCount, "Mother Brain regular native run count");
            AssertEqual(run, dto.Runs.Length, "Mother Brain regular DTO run count");
            AssertEqual(run, exported[index].Runs.Length, "Mother Brain regular export run count");
            AssertEqual(index + 1 < pointers.Length ? pointers[index + 1] : 0x9717,
                cursor, "Mother Brain regular original draw end");
            foreach (int invalid in new[] { int.MinValue, -1, run, int.MaxValue })
            {
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Mother Brain regular run bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "Mother Brain regular offset bounds");
            }
        }
        AssertEqual(63, cells, "Mother Brain regular sixty-three original physical cells");

        ushort ReadWord(int address) => (ushort)(rom.ReadByte(0x840000 | address) |
            rom.ReadByte(0x840000 | (address + 1)) << 8);
    }
}
