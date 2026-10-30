using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Compares the three exported elevator platform PLM draw lists with cartridge data and room effects.</summary>
internal static class LookupElevatorPlatformLayoutsVerification
{
    /// <summary>Checks draw run positions, continuation offsets, and resulting room block placement.</summary>
    internal static void VerifyElevatorPlatformLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 0);
    /// <summary>Checks the collision kind of each exported block against the decoded native draw lists.</summary>
    internal static void VerifyElevatorPlatformLayoutCollision(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 1);
    /// <summary>Checks the appearance of each exported block against the decoded native draw lists.</summary>
    internal static void VerifyElevatorPlatformLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 2);

    /// <summary>Checks the requested word field across the calculated shape, DTO exports, and native PLM list.</summary>
    private static void VerifyElevatorPlatformLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xaa97,0xaaaf,0xaac7];
        string[] ids = ["first-frame","second-frame","third-frame"];
        var exported = ElevatorPlatformDraws.DrawLists.ToArray();
        AssertEqual(3, exported.Length, "ElevatorPlatform layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ElevatorPlatformDraws.TryDescribe(NativePlmDraws.At<ElevatorPlatformDraw>(pointer), out var shape), "ElevatorPlatform layout shape domain");
                AssertEqual(owned, ElevatorPlatformDraws.TryGetDraw(NativePlmDraws.At<ElevatorPlatformDraw>(pointer), out var dto), "ElevatorPlatform layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ElevatorPlatformDraws.Draw), shape, "ElevatorPlatform layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "ElevatorPlatform layout missing DTO");
                    AssertThrows<InvalidDataException>(() => ElevatorPlatformDraws.VisualId(NativePlmDraws.At<ElevatorPlatformDraw>(pointer)), "ElevatorPlatform layout missing ID");
                }
            }
            foreach (string id in new[] { "FIRST-FRAME", "broken", "", "unknown" })
            {
                AssertTrue(!ElevatorPlatformDraws.TryGetByVisualId(id, out var missing), "ElevatorPlatform layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "ElevatorPlatform layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ElevatorPlatformDraws.TryDescribe(NativePlmDraws.At<ElevatorPlatformDraw>(pointer), out var shape);
            ElevatorPlatformDraws.TryGetDraw(NativePlmDraws.At<ElevatorPlatformDraw>(pointer), out var dto);
            ElevatorPlatformDraws.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, PlmDrawAddressCodec.Of(exported[index].Draw), "ElevatorPlatform layout original export order");
                AssertEqual(ids[index], ElevatorPlatformDraws.VisualId(NativePlmDraws.At<ElevatorPlatformDraw>(pointer)), "ElevatorPlatform layout published ID");
                AssertEqual(pointer, PlmDrawAddressCodec.Of(byId.Draw), "ElevatorPlatform layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "ElevatorPlatform layout run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.NextX(invalid), "ElevatorPlatform layout X bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.NextY(invalid), "ElevatorPlatform layout offset bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(invalid, 0), "ElevatorPlatform layout word run bounds");
                }
            }
            var level = CreateEmptyRoom(new(32, 16));
            var plms = PlmFixtures.Unbound();
            var bus = new TestAddressSpace();
            if (field == 0)
            {
                SeedSoleElevatorPlatformPlm(bus, 0x9400, new(3, 4), 0xafb6);
                AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), SuperMetroid.TestSupport.VerificationImporters.RoomPlmPopulationImporter.Read(bus, 0x9400), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }), "Elevator platform fixture spawn");
                plms.DrawSolePlmFrameForVerification(level, NativePlmDraws.At<ElevatorPlatformDraw>(pointer));
            }
            int drawX = 3, drawY = 4;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterPlmWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "ElevatorPlatform layout native direction");
                    AssertEqual(count, shape.BlockCount(run), "ElevatorPlatform layout native calculated count");
                    AssertEqual(LittleEndianBytes.Word((byte)shape.NextX(run), (byte)shape.NextY(run)), offset, "ElevatorPlatform layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "ElevatorPlatform layout native DTO geometry");
                        AssertEqual(count, part.Blocks.Length, "ElevatorPlatform layout native DTO count");
                        AssertEqual(offset, LittleEndianBytes.Word((byte)part.Next.X, (byte)part.Next.Y), "ElevatorPlatform layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2)),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).Terrain,
                            "ElevatorPlatform decoded native block at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(selectedRun, invalid), "ElevatorPlatform layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2));
                    AssertTerrainField(field, expected, shape.BlockAt(run, cell), "ElevatorPlatform layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertTerrainField(field, expected, frame.Runs.Span[run].Blocks.Span[cell], "ElevatorPlatform layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "ElevatorPlatform layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "ElevatorPlatform layout native run count");
            AssertEqual(run, dto.Runs.Length, "ElevatorPlatform layout native DTO run count");
        }
        AssertEqual(18, cells, "ElevatorPlatform layout independent physical cell count");
    }
}
