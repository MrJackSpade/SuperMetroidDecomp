using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire arena frames' block shapes and the import's run form against the five native bank-$84 lists.</summary>
internal static class LookupCrocomireDrawsVerification
{
    /// <summary>
    /// Compares all sixty cells, the import's run widths and offsets, and each frame's block shape with the cartridge,
    /// checking exact pointer ownership, that each shape covers exactly the cells its list writes, and that it rejects a
    /// cell past every run's end.
    /// </summary>
    internal static void VerifyCrocomirePhysicalDrawMapping(ISnesAddressSpace rom)
    {
        ushort[] pointers = [0x9b5b, 0x9b73, 0x9b79, 0x9b7f, 0x9bbb];
        var exported = RoomObjectCrocomireVisualFiles.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "Crocomire physical export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool owned = Array.IndexOf(pointers, pointer) >= 0;
            AssertEqual(owned, CrocomireArenaDraws.TryDescribe(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out RoomBlockShape? shape),
                $"Crocomire shape ownership {value:X4}");
            AssertEqual(owned, RoomObjectCrocomireVisualFiles.TryGet(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var dto),
                $"Crocomire DTO ownership {value:X4}");
            if (!owned)
            {
                AssertTrue(shape is null, "Crocomire missing shape");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Crocomire missing DTO");
            }
        }
        int cells = 0;
        InstalledRoomObjectCrocomireVisuals pictures = InstalledRoomObjectCrocomireVisuals.Stock();
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Crocomire export order");
            if (!CrocomireArenaDraws.TryDescribe(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out RoomBlockShape? described))
                throw new InvalidOperationException($"Crocomire frame {pointer:X4} has no shape.");
            RoomBlockShape shape = described;
            RoomObjectCrocomireVisualFiles.TryGet(NativeObjectDraws.At<CrocomireArenaDraw>(pointer), out var dto);
            int cursor = pointer, run = 0, frameCells = 0;
            while (true)
            {
                ushort count = ReadWord(cursor);
                cursor += 2;
                AssertRunHeader(count, dto.Runs.Span[run], "Crocomire DTO direction/count");
                AssertRunHeader(count, exported[index].Runs.Span[run], "Crocomire export direction/count");
                int length = count & 0x7fff;
                AssertEqual(length, dto.Runs.Span[run].Count, "Crocomire DTO width");
                AssertEqual(length, exported[index].Runs.Span[run].Count, "Crocomire export width");
                for (int block = 0; block < length; block++, cursor += 2)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadWord(cursor));
                    Offset16<RoomBlocks> place = dto.Runs.Span[run].CellAt(block);
                    AssertEqual(expected.Collision, shape.CollisionAt(place), "Crocomire native shape collision");
                    AssertEqual(expected.Collision, dto.Runs.Span[run].Collisions.Span[block], "Crocomire native DTO collision");
                    AssertEqual(expected.Collision, exported[index].Runs.Span[run].Collisions.Span[block], "Crocomire native export collision");
                    AssertEqual(expected.Appearance, pictures.Image(dto.Draw).At(place), "Crocomire installed picture");
                    cells++;
                    frameCells++;
                }
                RoomObjectShotBlockDrawDefinitions.Run written = dto.Runs.Span[run];
                Offset16<RoomBlocks> pastEnd = written.Start + (written.Direction == RoomObjectRunDirection.Vertical
                    ? new Offset16<RoomBlocks>(0, length)
                    : new Offset16<RoomBlocks>(length, 0));
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(pastEnd), "Crocomire shape ends where its run ends");
                sbyte nextX = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                sbyte nextY = unchecked((sbyte)CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | cursor++));
                AssertEqual(nextX, RoomObjectRunContinuationCodecConstants.X(dto, run), "Crocomire DTO X");
                AssertEqual(nextY, RoomObjectRunContinuationCodecConstants.Y(dto, run), "Crocomire DTO Y");
                AssertEqual(nextX, RoomObjectRunContinuationCodecConstants.X(exported[index], run), "Crocomire export X");
                AssertEqual(nextY, RoomObjectRunContinuationCodecConstants.Y(exported[index], run), "Crocomire export Y");
                run++;
                if (nextX == 0 && nextY == 0) break;
            }
            AssertEqual(run, dto.Runs.Length, "Crocomire DTO run count");
            AssertEqual(run, exported[index].Runs.Length, "Crocomire export run count");
            AssertEqual(frameCells, shape.Cells.Count, "Crocomire shape covers exactly the cells its list writes");
            foreach (Offset16<RoomBlocks> outside in new Offset16<RoomBlocks>[] { new(-1, 0), new(0, -1) })
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(outside), "Crocomire shape starts at the object's block");
            AssertEqual(index + 1 < pointers.Length ? pointers[index + 1] : 0x9bf7,
                cursor, "Crocomire original draw end");
        }
        AssertEqual(60, cells, "Crocomire sixty original physical cells");

        ushort ReadWord(int address) => LittleEndianBytes.Word(CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | address), CrocomirePhysicalDrawMappingRomByte(rom, 0x840000 | (address + 1)));
    }

    /// <summary>Reads native draw-list bytes solely as the physical-layout oracle.</summary>
    private static byte CrocomirePhysicalDrawMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
