using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies that the five Crocomire arena draw layouts match native bank-$84 encodings, including pointer ownership, all 60 block cells, and run continuations.</summary>
    /// <param name="rom">Cartridge address space containing the native Crocomire draw lists.</param>
    private static void VerifyCrocomirePhysicalDrawMapping(ISnesAddressSpace rom)
    {
        ushort[] pointers = [0x9b5b, 0x9b73, 0x9b79, 0x9b7f, 0x9bbb];
        var exported = CrocomireArenaPlmDrawDefinitions.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "Crocomire physical export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool owned = Array.IndexOf(pointers, pointer) >= 0;
            AssertEqual(owned, CrocomireArenaPlmDrawDefinitions.TryDescribe(pointer, out var shape),
                $"Crocomire descriptor ownership {value:X4}");
            AssertEqual(owned, CrocomireArenaPlmDrawDefinitions.TryGet(pointer, out var dto),
                $"Crocomire DTO ownership {value:X4}");
            if (!owned)
            {
                AssertEqual(default(CrocomireArenaPlmDrawDefinitions.Draw), shape, "Crocomire missing descriptor");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Crocomire missing DTO");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            AssertEqual(pointer, exported[index].Pointer, "Crocomire export order");
            CrocomireArenaPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            CrocomireArenaPlmDrawDefinitions.TryGet(pointer, out var dto);
            int cursor = pointer, run = 0;
            while (true)
            {
                ushort count = ReadWord(cursor);
                cursor += 2;
                AssertEqual(count, shape.DirectionAndCount, "Crocomire native direction/count");
                AssertEqual(count, dto.Runs.Span[run].DirectionAndCount, "Crocomire DTO direction/count");
                AssertEqual(count, exported[index].Runs.Span[run].DirectionAndCount, "Crocomire export direction/count");
                int length = count & 0x7fff;
                AssertEqual(length, shape.WordsPerRun, "Crocomire native width");
                AssertEqual(length, dto.Runs.Span[run].LevelWords.Length, "Crocomire DTO width");
                AssertEqual(length, exported[index].Runs.Span[run].LevelWords.Length, "Crocomire export width");
                AssertEqual((count & 0x8000) != 0, shape.Wall, "Crocomire runtime direction");
                for (int block = 0; block < length; block++, cursor += 2)
                {
                    ushort expected = ReadWord(cursor);
                    AssertEqual(expected, shape.WordAt(run, block), "Crocomire native calculated cell");
                    AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[block], "Crocomire native DTO cell");
                    AssertEqual(expected, exported[index].Runs.Span[run].LevelWords.Span[block], "Crocomire native export cell");
                    cells++;
                }
                sbyte nextX = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                sbyte nextY = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                AssertEqual(nextX, shape.NextX(run), "Crocomire native origin-relative X");
                AssertEqual(nextX, dto.Runs.Span[run].NextX, "Crocomire DTO X");
                AssertEqual(nextY, dto.Runs.Span[run].NextY, "Crocomire DTO Y");
                AssertEqual(nextX, exported[index].Runs.Span[run].NextX, "Crocomire export X");
                AssertEqual(nextY, exported[index].Runs.Span[run].NextY, "Crocomire export Y");
                AssertEqual((sbyte)0, nextY, "Crocomire runtime column origin Y");
                foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(run, invalid), "Crocomire cell bounds");
                run++;
                if (nextX == 0 && nextY == 0) break;
            }
            AssertEqual(run, shape.RunCount, "Crocomire native run count");
            AssertEqual(run, dto.Runs.Length, "Crocomire DTO run count");
            AssertEqual(run, exported[index].Runs.Length, "Crocomire export run count");
            AssertEqual(index + 1 < pointers.Length ? pointers[index + 1] : 0x9bf7,
                cursor, "Crocomire original draw end");
            foreach (int invalid in new[] { int.MinValue, -1, run, int.MaxValue })
            {
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Crocomire run bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "Crocomire offset bounds");
            }
        }
        AssertEqual(60, cells, "Crocomire sixty original physical cells");

        ushort ReadWord(int address) => (ushort)(rom.ReadByte(0x840000 | address) |
            rom.ReadByte(0x840000 | (address + 1)) << 8);
    }
}
