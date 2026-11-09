using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire arena PLM physical draw descriptors against the five native bank-$84 lists.</summary>
internal static class LookupCrocomireDrawsVerification
{
    /// <summary>Compares all sixty physical cells, run widths, and offsets while checking exact pointer ownership and bounds.</summary>
    internal static void VerifyCrocomirePhysicalDrawMapping(ISnesAddressSpace rom)
    {
        ushort[] pointers = [0x9b5b, 0x9b73, 0x9b79, 0x9b7f, 0x9bbb];
        var exported = RoomObjectCrocomireVisualFiles.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "Crocomire physical export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool owned = Array.IndexOf(pointers, pointer) >= 0;
            AssertEqual(owned, CrocomireArenaDraws.TryDescribe(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var shape),
                $"Crocomire descriptor ownership {value:X4}");
            AssertEqual(owned, RoomObjectCrocomireVisualFiles.TryGet(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var dto),
                $"Crocomire DTO ownership {value:X4}");
            if (!owned)
            {
                AssertEqual(default(CrocomireArenaDraws.Draw), shape, "Crocomire missing descriptor");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Crocomire missing DTO");
            }
        }
        int cells = 0;
        InstalledRoomObjectCrocomireVisuals pictures = InstalledRoomObjectCrocomireVisuals.Stock();
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Crocomire export order");
            CrocomireArenaDraws.TryDescribe(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var shape);
            RoomObjectCrocomireVisualFiles.TryGet(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var dto);
            int cursor = pointer, run = 0;
            while (true)
            {
                ushort count = ReadWord(cursor);
                cursor += 2;
                AssertRunHeader(count, shape.Direction, shape.BlocksPerRun, "Crocomire native direction/count");
                AssertRunHeader(count, dto.Runs.Span[run], "Crocomire DTO direction/count");
                AssertRunHeader(count, exported[index].Runs.Span[run], "Crocomire export direction/count");
                int length = count & 0x7fff;
                AssertEqual(length, shape.BlocksPerRun, "Crocomire native width");
                AssertEqual(length, dto.Runs.Span[run].Count, "Crocomire DTO width");
                AssertEqual(length, exported[index].Runs.Span[run].Count, "Crocomire export width");
                AssertEqual((count & 0x8000) != 0, shape.Wall, "Crocomire runtime direction");
                for (int block = 0; block < length; block++, cursor += 2)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadWord(cursor));
                    AssertEqual(expected.Collision, shape.CollisionAt(run, block), "Crocomire native calculated collision");
                    AssertEqual(expected.Collision, dto.Runs.Span[run].Collisions.Span[block], "Crocomire native DTO collision");
                    AssertEqual(expected.Collision, exported[index].Runs.Span[run].Collisions.Span[block], "Crocomire native export collision");
                    AssertEqual(expected.Appearance, pictures.Image(dto.Draw).At(dto.Runs.Span[run].CellAt(block)), "Crocomire installed picture");
                    cells++;
                }
                sbyte nextX = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                sbyte nextY = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                AssertEqual(nextX, RoomObjectRunContinuationCodecConstants.X(dto, run), "Crocomire DTO X");
                AssertEqual(nextY, RoomObjectRunContinuationCodecConstants.Y(dto, run), "Crocomire DTO Y");
                AssertEqual(nextX, RoomObjectRunContinuationCodecConstants.X(exported[index], run), "Crocomire export X");
                AssertEqual(nextY, RoomObjectRunContinuationCodecConstants.Y(exported[index], run), "Crocomire export Y");
                AssertEqual((sbyte)0, nextY, "Crocomire runtime column origin Y");
                foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(run, invalid), "Crocomire cell bounds");
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
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Crocomire run bounds");

                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Crocomire start bounds");
            }
        }
        AssertEqual(60, cells, "Crocomire sixty original physical cells");

        ushort ReadWord(int address) => LittleEndianBytes.Word(CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | address), CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | (address + 1)));
    }

    /// <summary>Reads native draw-list bytes solely as the physical-layout oracle.</summary>
    private static byte CrocomirePhysicalDrawMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
