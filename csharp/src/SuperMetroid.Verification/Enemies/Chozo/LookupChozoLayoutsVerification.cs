using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks three Chozo statue PLM draw layouts against native geometry and decoded terrain blocks.</summary>
internal static class LookupChozoLayoutsVerification
{
    /// <summary>Checks draw-domain ownership, continuation offsets, and physical block placement.</summary>
    internal static void VerifyChozoLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 0);
    /// <summary>Compares the collision kind of all Chozo layout cells with the decoded native draw blocks.</summary>
    internal static void VerifyChozoLayoutCollision(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 1);
    /// <summary>Compares the appearance of all Chozo layout cells with the decoded native draw blocks.</summary>
    internal static void VerifyChozoLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 2);

    /// <summary>Checks the selected geometry or cell field across pointer and visual-ID views.</summary>
    private static void VerifyChozoLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa2b5,0x9cc5,0x9d0f];
        string[] ids = ["lower-norfair-cleared-hand","wrecked-ship-clear-slope-access","wrecked-ship-block-slope-access"];
        var exported = RoomObjectChozoStatueVisualFiles.All.ToArray();
        AssertEqual(3, exported.Length, "Chozo layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ChozoStatueDraws.TryDescribe(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var shape), "Chozo layout shape domain");
                AssertEqual(owned, RoomObjectChozoStatueVisualFiles.TryGet(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var dto), "Chozo layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ChozoStatueDraws.Draw), shape, "Chozo layout missing shape");
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
            ChozoStatueDraws.TryDescribe(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var shape);
            RoomObjectChozoStatueVisualFiles.TryGet(NativeObjectDraws.At<ChozoStatueDraw>(pointer), out var dto);
            RoomObjectDraw byId = RoomObjectChozoStatueVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Chozo layout original export order");
                AssertEqual(ids[index], RoomObjectChozoStatueVisualFiles.VisualId(NativeObjectDraws.At<ChozoStatueDraw>(pointer)), "Chozo layout published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Chozo layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Chozo layout run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Chozo layout start bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(invalid, 0), "Chozo layout block run bounds");
                }
            }
            var level = CreateEmptyRoom(new(32, 16));
            var roomObjects = RoomObjectFixtures.Unbound();
            var guarded = NewChozoProgramAndDrawReadGuard(rom);
            if (field == 0)
            {
                AssertTrue(roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level,
                    new ChozoStatueObjectRequest(RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo, new Position16<RoomBlocks>(3, 4), IsHardcoded: false)), "Chozo layout fixture spawn");
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
                    AssertEqual(count, shape.BlockCount(run), "Chozo layout native calculated count");
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < shape.RunCount ? shape.Start(run + 1) : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "Chozo layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Chozo layout native DTO geometry");
                        AssertEqual(count, part.Blocks.Length, "Chozo layout native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodec.Encode(frame, run), "Chozo layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2)),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).Terrain,
                            "Chozo decoded native block at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(selectedRun, invalid), "Chozo layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    AssertTerrainField(field, expected, shape.BlockAt(run, cell), "Chozo layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertTerrainField(field, expected, frame.Runs.Span[run].Blocks.Span[cell], "Chozo layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "Chozo layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Chozo layout native run count");
            AssertEqual(run, dto.Runs.Length, "Chozo layout native DTO run count");
        }
        AssertEqual(55, cells, "Chozo layout independent physical cell count");
    }
}
