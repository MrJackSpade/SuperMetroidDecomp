using SuperMetroid.Core.World;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
namespace SuperMetroid.Verification.Enemies.Elevator;

using SuperMetroid.Verification.Harness;

/// <summary>Compares the three exported elevator platform PLM draw lists with cartridge data and room effects.</summary>
internal static class LookupElevatorPlatformLayoutsVerification
{
    /// <summary>Checks draw run positions, continuation offsets, and resulting room block placement.</summary>
    internal static void VerifyElevatorPlatformLayoutGeometry(CartridgeImportAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 0);
    /// <summary>Checks the collision kind of each exported block against the decoded native draw lists.</summary>
    internal static void VerifyElevatorPlatformLayoutCollision(CartridgeImportAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 1);
    /// <summary>Checks the appearance of each exported block against the decoded native draw lists.</summary>
    internal static void VerifyElevatorPlatformLayoutVisuals(CartridgeImportAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 2);

    /// <summary>Checks the requested word field across the calculated shape, DTO exports, and native PLM list.</summary>
    private static void VerifyElevatorPlatformLayoutDrawField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0xaa97,0xaaaf,0xaac7];
        string[] ids = ["dim","glowing","bright"];
        var exported = RoomObjectElevatorPlatformVisualFiles.DrawLists.ToArray();
        AssertEqual(3, exported.Length, "ElevatorPlatform layout export count");
        InstalledRoomObjectPictures pictures = RoomObjectPicturesStock.ElevatorPlatform;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, RoomObjectDrawIdentity.TryShape(ElevatorPlatformDraws.Family, NativeObjectDraws.At<ElevatorPlatformDraw>(pointer), out var shape), "ElevatorPlatform layout shape domain");
                AssertEqual(owned, RoomObjectElevatorPlatformVisualFiles.TryGetDraw(NativeObjectDraws.At<ElevatorPlatformDraw>(pointer), out var dto), "ElevatorPlatform layout DTO domain");
                if (!owned)
                {
                    AssertTrue(shape is null, "ElevatorPlatform layout missing shape");
                    AssertEqual(default(RoomObjectDrawLists.DrawList), dto, "ElevatorPlatform layout missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectElevatorPlatformVisualFiles.VisualId(NativeObjectDraws.At<ElevatorPlatformDraw>(pointer)), "ElevatorPlatform layout missing ID");
                }
            }
            foreach (string id in new[] { "FIRST-FRAME", "broken", "", "unknown" })
                AssertTrue(!RoomObjectElevatorPlatformVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "ElevatorPlatform layout ordinal ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            RoomBlockShape shape = RoomObjectDrawIdentity.TryShape(ElevatorPlatformDraws.Family, NativeObjectDraws.At<ElevatorPlatformDraw>(pointer), out RoomBlockShape? described)
                ? described : throw new InvalidOperationException($"Elevator platform draw {pointer:X4} has no block shape.");
            RoomObjectElevatorPlatformVisualFiles.TryGetDraw(NativeObjectDraws.At<ElevatorPlatformDraw>(pointer), out var dto);
            RoomObjectDraw byId = RoomObjectElevatorPlatformVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "ElevatorPlatform layout original export order");
                AssertEqual(ids[index], RoomObjectElevatorPlatformVisualFiles.VisualId(NativeObjectDraws.At<ElevatorPlatformDraw>(pointer)), "ElevatorPlatform layout published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "ElevatorPlatform layout reverse ID");
                // The block shape covers exactly the cells the cartridge writes (#1862): six, none outside its runs.
                AssertEqual(6, shape.Cells.Count, "ElevatorPlatform block shape cell count");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(new(1, 0)), "ElevatorPlatform block shape leaves the gap between its edges");
            }
            var level = CreateEmptyRoom(new(32, 16));
            var roomObjects = RoomObjectFixtures.Unbound();
            var bus = new TestAddressSpace();
            if (field == 0)
            {
                SeedSoleElevatorPlatformObject(bus, 0x9400, new(3, 4), 0xafb6);
                RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), SuperMetroid.TestSupport.VerificationImporters.RoomObjectPopulationImporter.Read(bus, 0x9400), RoomObjectServiceFixtures.Uninstalled with { Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
                roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<ElevatorPlatformDraw>(pointer));
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
                    AssertEqual((ushort)count, header, "ElevatorPlatform layout native direction");
                    AssertEqual(count, dto.Runs.Span[run].Count, "ElevatorPlatform layout native exported count");
                    // The cartridge ends each strip with the next strip's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < dto.Runs.Length ? dto.Runs.Span[run + 1].Start : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "ElevatorPlatform layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "ElevatorPlatform layout native DTO geometry");
                        AssertEqual(count, part.Count, "ElevatorPlatform layout native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodecConstants.Encode(frame, run), "ElevatorPlatform layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2)),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).Terrain,
                            "ElevatorPlatform decoded native block at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    RoomObjectFrameImage picture = pictures.Image(NativeObjectDraws.At<ElevatorPlatformDraw>(pointer));
                    Offset16<RoomBlocks> place = dto.Runs.Span[run].CellAt(cell);
                    AssertCellField(field, expected, shape.CollisionAt(place), picture, place, "ElevatorPlatform layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                        AssertCellField(field, expected, frame.Runs.Span[run].Collisions.Span[cell], picture, frame.Runs.Span[run].CellAt(cell), "ElevatorPlatform layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "ElevatorPlatform layout native run termination");
            }
            AssertEqual(run, dto.Runs.Length, "ElevatorPlatform layout native DTO run count");
        }
        AssertEqual(18, cells, "ElevatorPlatform layout independent physical cell count");
    }
}
