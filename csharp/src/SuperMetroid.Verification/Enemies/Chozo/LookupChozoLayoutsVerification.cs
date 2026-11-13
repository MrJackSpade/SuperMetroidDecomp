using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks three Chozo statue PLM draw layouts against native geometry and decoded terrain blocks.</summary>
internal static class LookupChozoLayoutsVerification
{
    /// <summary>Checks draw-domain ownership, continuation offsets, and physical block placement.</summary>
    internal static void VerifyChozoLayoutGeometry(CartridgeImportAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 0);
    /// <summary>Compares the collision kind of all Chozo layout cells with the decoded native draw blocks.</summary>
    internal static void VerifyChozoLayoutCollision(CartridgeImportAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 1);
    /// <summary>Compares the appearance of all Chozo layout cells with the decoded native draw blocks.</summary>
    internal static void VerifyChozoLayoutVisuals(CartridgeImportAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 2);

    /// <summary>Checks the selected geometry or cell field across pointer and visual-ID views.</summary>
    private static void VerifyChozoLayoutDrawField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa2b5,0x9cc5,0x9d0f];
        string[] ids = ["lower-norfair-cleared-hand","wrecked-ship-clear-slope-access","wrecked-ship-block-slope-access"];
        var exported = RoomObjectChozoStatueVisualFiles.All.ToArray();
        AssertEqual(3, exported.Length, "Chozo layout export count");
        InstalledRoomObjectChozoStatueVisuals pictures = InstalledRoomObjectChozoStatueVisuals.Stock();
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ChozoStatueDraws.TryDescribe(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out RoomBlockShape? shape), "Chozo layout shape domain");
                AssertEqual(owned, RoomObjectChozoStatueVisualFiles.TryGet(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var dto), "Chozo layout DTO domain");
                if (!owned)
                {
                    AssertTrue(shape is null, "Chozo layout missing shape");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Chozo layout missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectChozoStatueVisualFiles.VisualId(NativeObjectDraws.At<ChozoStatueDraw>(pointer)), "Chozo layout missing ID");
                }
            }
            foreach (string id in new[] { "LOWER-NORFAIR-CLEARED-HAND", "broken", "", "unknown" })
                AssertTrue(!RoomObjectChozoStatueVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Chozo layout ordinal ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            if (!ChozoStatueDraws.TryDescribe(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out RoomBlockShape? described))
                throw new InvalidOperationException($"Chozo statue frame {ids[index]} has no shape.");
            RoomBlockShape shape = described;
            RoomObjectChozoStatueVisualFiles.TryGet(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var dto);
            int frameCells = 0;
            RoomObjectDraw byId = RoomObjectChozoStatueVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Chozo layout original export order");
                AssertEqual(ids[index], RoomObjectChozoStatueVisualFiles.VisualId(NativeObjectDraws.At<ChozoStatueDraw>(pointer)), "Chozo layout published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Chozo layout reverse ID");
            }
            var level = CreateEmptyRoom(new(32, 16));
            var roomObjects = RoomObjectFixtures.Unbound();
            var guarded = NewChozoProgramAndDrawReadGuard(rom);
            if (field == 0)
            {
                AssertTrue(roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level,
                    new RoomObjectSpawnRequest<ChozoStatueRoomObject>(new Position16<RoomBlocks>(3, 4), ChozoStatueRoomObject.SlopeAccessOpening)), "Chozo layout fixture spawn");
                roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<ChozoStatueDraw>(pointer));
                AssertEqual(0, guarded.ForbiddenReadAttempts, "Chozo calculated draw avoids ROM reads");
            }
            int drawX = 3, drawY = 4;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterObjectWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Chozo layout native direction");
                    for (int cell = 0; cell < count; cell++)
                        AssertTrue(shape.Covers(dto.Runs.Span[run].CellAt(cell)), "Chozo layout shape covers every cell its list writes");
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < dto.Runs.Length ? dto.Runs.Span[run + 1].Start : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "Chozo layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Chozo layout native DTO geometry");
                        AssertEqual(count, part.Count, "Chozo layout native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodecConstants.Encode(frame, run), "Chozo layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2)),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).Terrain,
                            "Chozo decoded native block at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    RoomObjectFrameImage picture = pictures.Image(NativeObjectDraws.At<ChozoStatueDraw>(pointer));
                    Offset16<RoomBlocks> place = dto.Runs.Span[run].CellAt(cell);
                    AssertCellField(field, expected, shape.CollisionAt(place), picture, place, "Chozo layout original shape field");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                        AssertCellField(field, expected, frame.Runs.Span[run].Collisions.Span[cell], picture, frame.Runs.Span[run].CellAt(cell), "Chozo layout original DTO field");
                }
                cells += count;
                frameCells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "Chozo layout native run termination");
            }
            AssertEqual(run, dto.Runs.Length, "Chozo layout native DTO run count");
            // The list writes no cell twice, so the shape covers exactly the cells it writes.
            AssertEqual(frameCells, shape.Cells.Count, "Chozo layout shape covers exactly the cells its list writes");
        }
        AssertEqual(55, cells, "Chozo layout independent physical cell count");
    }
}
