using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire arena PLM physical draw descriptors against the five native bank-$84 lists.</summary>
internal static class LookupCrocomireDrawsVerification
{
    /// <summary>Compares all sixty physical cells, run widths, and offsets while checking exact pointer ownership and bounds.</summary>
    internal static void VerifyCrocomirePhysicalDrawMapping(ISnesAddressSpace rom)
    {
        ushort[] pointers = [0x9b5b, 0x9b73, 0x9b79, 0x9b7f, 0x9bbb];
        var exported = CrocomireArenaPlmDrawDefinitions.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "Crocomire physical export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool owned = Array.IndexOf(pointers, pointer) >= 0;
            AssertEqual(owned, CrocomireArenaPlmDrawDefinitions.TryDescribe(NativePlmDraws.At<CrocomireArenaDraw>(pointer), out var shape),
                $"Crocomire descriptor ownership {value:X4}");
            AssertEqual(owned, CrocomireArenaPlmDrawDefinitions.TryGet(NativePlmDraws.At<CrocomireArenaDraw>(pointer), out var dto),
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
            AssertEqual(pointer, PlmDrawAddressCodec.Of(exported[index].Draw), "Crocomire export order");
            CrocomireArenaPlmDrawDefinitions.TryDescribe(NativePlmDraws.At<CrocomireArenaDraw>(pointer), out var shape);
            CrocomireArenaPlmDrawDefinitions.TryGet(NativePlmDraws.At<CrocomireArenaDraw>(pointer), out var dto);
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
                sbyte nextX = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                sbyte nextY = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                AssertEqual(nextX, shape.NextX(run), "Crocomire native origin-relative X");
                AssertEqual(nextX, dto.Runs.Span[run].Next.X, "Crocomire DTO X");
                AssertEqual(nextY, dto.Runs.Span[run].Next.Y, "Crocomire DTO Y");
                AssertEqual(nextX, exported[index].Runs.Span[run].Next.X, "Crocomire export X");
                AssertEqual(nextY, exported[index].Runs.Span[run].Next.Y, "Crocomire export Y");
                AssertEqual((sbyte)0, nextY, "Crocomire runtime column origin Y");
                foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(run, invalid), "Crocomire cell bounds");
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
                AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(invalid, 0), "Crocomire run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.NextX(invalid), "Crocomire offset bounds");
            }
        }
        AssertEqual(60, cells, "Crocomire sixty original physical cells");

        ushort ReadWord(int address) => SnesLittleEndian.Word(CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | address), CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | (address + 1)));
    }

    /// <summary>Reads native draw-list bytes solely as the physical-layout oracle.</summary>
    private static byte CrocomirePhysicalDrawMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
