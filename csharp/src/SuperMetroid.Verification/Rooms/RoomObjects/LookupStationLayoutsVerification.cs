using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks station PLM draw geometry, collision, and visual words against retail layouts.</summary>
internal static class LookupStationLayoutsVerification
{
    /// <summary>Checks the twenty layout pointers, published IDs, native run shapes, and live physical placement.</summary>
    internal static void VerifyStationLayoutGeometry(CartridgeImportAddressSpace rom) => VerifyStationLayoutDrawField(rom, 0);
    /// <summary>Compares the collision nibble of all sixty-four station draw cells.</summary>
    internal static void VerifyStationLayoutCollision(CartridgeImportAddressSpace rom) => VerifyStationLayoutDrawField(rom, 1);
    /// <summary>Compares the visual metatile and flip bits of all station draw cells.</summary>
    internal static void VerifyStationLayoutVisuals(CartridgeImportAddressSpace rom) => VerifyStationLayoutDrawField(rom, 2);

    /// <summary>Compares one selected field across native bank-$84 runs, descriptors, and exported artwork records.</summary>
    private static void VerifyStationLayoutDrawField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9f25,0x9f6d,0x9f91,0x9f31,0x9f79,0x9f9d,0x9f3d,0x9f85,0x9fa9,0x9a3f,0x9a9f,0x9a6f,0x9f49,0x9f55,0x9f5b,0x9f67,0x9fb5,0x9fbb,0x9fc1,0x9fc7];
        string[] ids = ["map-frame-0","energy-frame-0","missile-frame-0","map-frame-1","energy-frame-1","missile-frame-1","map-frame-2","energy-frame-2","missile-frame-2","save-idle","save-active-a","save-active-b","map-right-retracted","map-right-extended","map-left-retracted","map-left-extended","resource-right-retracted","resource-right-extended","resource-left-retracted","resource-left-extended"];
        var exported = RoomObjectStationVisualFiles.All.ToArray();
        AssertEqual(20, exported.Length, "Station layout export count");
        InstalledRoomObjectStationVisuals? pictures = field == 2 ? InstalledRoomObjectStationVisuals.Stock() : null;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, StationObjectDraws.TryDescribe(NativeObjectDraws.At<StationDraw>(pointer), out var shape), "Station layout shape domain");
                AssertEqual(owned, RoomObjectStationVisualFiles.TryGet(NativeObjectDraws.At<StationDraw>(pointer), out var dto), "Station layout DTO domain");
                if (!owned)
                {
                    AssertEqual(null, shape, "Station layout missing shape");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Station layout missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectStationVisualFiles.VisualId(NativeObjectDraws.At<StationDraw>(pointer)), "Station layout missing ID");
                }
            }
            foreach (string id in new[] { "MAP-FRAME-0", "broken", "", "unknown" })
                AssertTrue(!RoomObjectStationVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Station layout ordinal ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            var shape = StationObjectDraws.TryDescribe(NativeObjectDraws.At<StationDraw>(pointer), out StationObjectDraws.Draw? described)
                ? described : throw new InvalidOperationException($"${pointer:X4} is not a station draw list.");
            RoomObjectStationVisualFiles.TryGet(NativeObjectDraws.At<StationDraw>(pointer), out var dto);
            RoomObjectDraw byId = RoomObjectStationVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Station layout original export order");
                AssertEqual(ids[index], RoomObjectStationVisualFiles.VisualId(NativeObjectDraws.At<StationDraw>(pointer)), "Station layout published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Station layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Station layout run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Station layout start bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Station layout word run bounds");
                }
            }
            var level = CreateRoom(new(32, 16), new RoomTerrainBlock[512], new RoomBlockBehavior[512]);
            var roomObjects = RoomObjectFixtures.Unbound();
            var bus = new TestAddressSpace();
            if (field == 0)
            {
                SeedSoleElevatorPlatformObject(bus, 0x9400, new(8, 8), 0xafb6);
                AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Crateria), SuperMetroid.TestSupport.VerificationImporters.RoomObjectPopulationImporter.Read(bus, 0x9400), new RoomObjectServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }), "Elevator platform fixture spawn");
                roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<StationDraw>(pointer));
            }
            int drawX = 8, drawY = 8;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterObjectWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Station layout native direction");
                    AssertEqual(count, shape.BlockCount(run), "Station layout native calculated count");
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < shape.RunCount ? shape.Start(run + 1) : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "Station layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Station layout native DTO geometry");
                        AssertEqual(count, part.Count, "Station layout native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodecConstants.Encode(frame, run), "Station layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2)),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).Terrain,
                            "Station native physical word at original absolute run position");
                    drawX = 8 + unchecked((sbyte)offset);
                    drawY = 8 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, invalid), "Station layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    if (field == 1)
                    {
                        AssertEqual(expected.Collision, shape.CollisionAt(run, cell), "Station layout original calculated collision");
                        foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                            AssertEqual(expected.Collision, frame.Runs.Span[run].Collisions.Span[cell], "Station layout original DTO collision");
                    }
                    else
                        // Core holds no appearance: what the cell shows is the station's installed picture.
                        AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<StationDraw>(pointer)).At(dto.Runs.Span[run].CellAt(cell)),
                            "Station layout installed picture shows the cartridge's block");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 7, "Station layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Station layout native run count");
            AssertEqual(run, dto.Runs.Length, "Station layout native DTO run count");
        }
        AssertEqual(64, cells, "Station layout independent physical cell count");
    }
}
