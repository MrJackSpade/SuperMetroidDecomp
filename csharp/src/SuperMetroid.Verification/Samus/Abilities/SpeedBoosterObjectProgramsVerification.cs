using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using static SuperMetroid.Verification.Harness.RetailAreaNumbers;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterCollisionBlocksVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks Speed Booster block PLM selection, compiled programs, sound, and physical reveal behavior.</summary>
internal static class SpeedBoosterObjectProgramsVerification
{
    /// <summary>Checks the bomb-reveal draw identity, one-run geometry, physical word, and continuation bytes.</summary>
    private static void VerifySpeedBlockRevealDrawMapping(CartridgeImportAddressSpace rom)
    {
        const ushort pointer = 0xa4f3;
        var exported = RoomObjectSpeedBoosterVisualFiles.All.ToArray();
        AssertEqual(1,exported.Length,"speed reveal export count");
        AssertEqual(pointer,RoomObjectDrawAddressCodec.Of(exported[0].Draw),"speed reveal export identity");
        // Every address but the reveal's yields the one unowned draw, so the visual-identity rejection is one case,
        // checked once; each address asserts that it yields that draw.
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            RoomObjectDraw atAddress = NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(address);
            bool found = SpeedBoosterBlockObjectDrawDefinitions.TryGet(atAddress,out var draw);
            AssertEqual(address == pointer,found,"speed reveal full pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList),draw,"speed reveal missing draw cleared");
                AssertEqual(NativeObjectDraws.Unowned,atAddress,"speed reveal unknown address yields the unowned draw");
            }
        }
        AssertThrows<InvalidDataException>(() => RoomObjectSpeedBoosterVisualFiles.VisualId(NativeObjectDraws.Unowned),"speed reveal rejects unknown visual identity");
        AssertEqual("bomb-reveal",RoomObjectSpeedBoosterVisualFiles.VisualId(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(pointer)),"speed reveal stable semantic ID");
        RoomObjectShotBlockDrawDefinitions.DrawList byId = RoomObjectSpeedBoosterVisualFiles.All.Single(draw => RoomObjectSpeedBoosterVisualFiles.Family.Ids[draw.Draw] == "bomb-reveal");
        foreach (string id in new[] {"", "Bomb-reveal", "bomb-reveal ", "unknown"})
            AssertTrue(!RoomObjectSpeedBoosterVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal),"speed reveal ordinal ID rejection");
        RoomTerrainBlock nativeTerrain = RoomLevelWordCodec.Decode(ReadRomWord(rom,0x840000 | (pointer + 2)));
        // What the reveal shows is its installed picture, which the Speed Booster picture check compares with the cartridge.
        AssertEqual(nativeTerrain.Collision,SpeedBoosterBlockObjectDrawDefinitions.BombRevealCollision,"speed reveal calculated physical collision");
        foreach (var draw in new[] {exported[0],byId,RoomObjectSpeedBoosterVisualFiles.BombReveal})
        {
            AssertEqual(pointer,RoomObjectDrawAddressCodec.Of(draw.Draw),"speed reveal DTO identity");
            AssertEqual(1,draw.Runs.Length,"speed reveal single run");
            var run = draw.Runs.Span[0];
            AssertRunHeader(ReadRomWord(rom,0x840000 | pointer),run,"speed reveal native direction/count");
            AssertEqual(1,run.Count,"speed reveal single word");
            AssertEqual(nativeTerrain.Collision,run.Collisions.Span[0],"speed reveal exported native collision");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 4))),RoomObjectRunContinuationCodecConstants.X(draw, 0),"speed reveal native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 5))),RoomObjectRunContinuationCodecConstants.Y(draw, 0),"speed reveal native terminal Y");
        }
    }

    /// <summary>
    /// Compares the complete BTS/area selector domain with original bank-94 entry records: each selected block's header
    /// names the Speed Booster setup, and its initial list plays as the block's typed crumble. The header itself is not
    /// compared, as the game keeps no record of it (#1691).
    /// </summary>
    private static void VerifySpeedBlockInstructionSelection(CartridgeImportAddressSpace rom)
    {
        // Original bank-94 speed-entry locations, independent of named C# results.
        SpeedBoosterObjectProgramsEntry[] entries =
            [new SpeedBoosterObjectProgramsEntry(0x0e,-1,0x949155),new SpeedBoosterObjectProgramsEntry(0x0f,-1,0x949157),new SpeedBoosterObjectProgramsEntry(0x82,1,0x9491fd),
             new SpeedBoosterObjectProgramsEntry(0x83,1,0x9491ff),new SpeedBoosterObjectProgramsEntry(0x84,1,0x949201),new SpeedBoosterObjectProgramsEntry(0x85,1,0x949203)];
        for (int area = 0; area <= byte.MaxValue; area++)
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            AreaId retail = RetailArea(area);
            if (retail == UnknownArea)
                continue;
            int tableAddress = 0;
            foreach (var entry in entries)
                if (entry.Bts == bts && (entry.Area < 0 || entry.Area == area))
                    tableAddress = entry.Address;
            bool found = SpeedBoosterBlocks.TryResolve(RoomBlockBehaviorCodec.Decode(retail, bts), out BlockReactionChange? crumble);
            AssertEqual(tableAddress != 0,found,"speed selector full byte BTS/area contract");
            if (!found)
            {
                AssertTrue(crumble is null,"speed selector missing crumble cleared");
                continue;
            }
            ushort header = ReadRomWord(rom,tableAddress);
            AssertEqual((ushort)0xcdea,ReadRomWord(rom,0x840000 | header),"native selected PLM is Speed Booster setup");
            ushort list = ReadRomWord(rom,0x840000 | (header + 2));
            AssertEqual(RoomObjectNativeListAddresses.SpeedBoosterBlockEntries.Single(entry => entry.Entry == list).Program,
                crumble, "speed native selected list plays its typed crumble");
        }
    }

    /// <summary>Runs static mappings and live boosted-contact and bomb-reveal PLM paths without source reads.</summary>
    internal static void VerifyCompiledSpeedBoosterObjectPrograms()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySpeedBlockInstructionSelection), () => VerifySpeedBlockInstructionSelection(rom));
        Suite(nameof(RoomObjectProgramsVerification.VerifySpeedBoosterBlockPrograms), () => RoomObjectProgramsVerification.VerifySpeedBoosterBlockPrograms(rom));

        Suite(nameof(VerifySpeedBlockRevealDrawMapping), () => VerifySpeedBlockRevealDrawMapping(rom));

        var cases = new SpeedBoosterObjectProgramsCase[]
        {
            new SpeedBoosterObjectProgramsCase(RoomBlockBehaviorCodec.Decode(0x0e), AreaId.Crateria, true),
            new SpeedBoosterObjectProgramsCase(RoomBlockBehaviorCodec.Decode(0x0f), AreaId.Crateria, false),
            new SpeedBoosterObjectProgramsCase(RoomBlockBehaviorCodec.Decode(AreaId.Brinstar, 0x82), AreaId.Brinstar, true),
            new SpeedBoosterObjectProgramsCase(RoomBlockBehaviorCodec.Decode(AreaId.Brinstar, 0x83), AreaId.Brinstar, false),
            new SpeedBoosterObjectProgramsCase(RoomBlockBehaviorCodec.Decode(AreaId.Brinstar, 0x84), AreaId.Brinstar, true),
        };
        foreach ((RoomBlockBehavior bts, AreaId area, bool respawns) in cases)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomObjectSystem roomObjects,
                int blockIndex) = CreateSpeedBoosterBlockFixture(bts, area);
            var guarded = NewSpeedBoosterObjectReadGuard(bus);
            SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
            BlockMoveResult contact = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = SignedSixteenSixteen.FromWholePixels(4), ScanLeftToRight = true });
            AssertTrue(!contact.Collided,
                $"boosted contact with BTS ${RoomBlockBehaviorCodec.Encode(bts):X2} admits the real PLM");
            for (int frame = 0; frame < 100 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                roomObjects.Step(level);
            AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                $"BTS ${RoomBlockBehaviorCodec.Encode(bts):X2} compiled list reaches native deletion");
            AssertEqual(respawns ? RoomCollisionType.SpecialBlock :
                    RoomCollisionType.Air,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"BTS ${RoomBlockBehaviorCodec.Encode(bts):X2} retains native restoration behavior");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"BTS ${RoomBlockBehaviorCodec.Encode(bts):X2} never rereads its compiled program or draw bytes");
        }

        (TestAddressSpace bombBus, RoomLevelData bombLevel,
            RoomObjectSystem bombObjects, int bombBlock) =
            CreateSpeedBoosterBlockFixture(RoomBlockBehaviorCodec.Decode(AreaId.Brinstar, 0x82),
                AreaId.Brinstar);
        var bombGuard = NewSpeedBoosterObjectReadGuard(bombBus);
        AssertTrue(bombObjects.BlockReactions.TrySpawnBombedSpecialBlock(bombLevel, bombLevel.BlockAt(bombBlock),
                RoomBlockBehaviorCodec.Decode(AreaId.Brinstar, 0x82), SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
            "normal bomb allocates the Speed Booster reveal PLM");
        bombObjects.Step(bombLevel);
        AssertEqual(RoomTerrainBlock.Of(0x0b6, BlockMirroring.None, RoomCollisionType.SpecialBlock),
            bombLevel.GetCollisionBlockByIndex(bombBlock).Terrain,
            "bombed speed block draws the native physical reveal word");
        bombObjects.Step(bombLevel);
        AssertEqual(0, bombObjects.SlotTable.ActiveCount,
            "bombed speed-block reveal deletes after its single frame");
        AssertEqual(0, bombGuard.ForbiddenReadAttempts,
            "bombed Speed Booster reveal reads no compiled source bytes");

        Console.WriteLine(
            "Speed-block PLMs: the typed programs and one physical reveal match ROM; all five boosted contacts and bomb reveal execute with source reads forbidden.");
    }

    /// <summary>Rejects runtime reads from compiled Speed Booster programs and their physical draw lists.</summary>
    internal static CartridgeReadGuard NewSpeedBoosterObjectReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    ((pointer >= 0xc928 && pointer < 0xc92e) ||
                     (pointer >= 0xc951 && pointer < 0xc9ba) ||
                     (pointer >= 0xc9cf && pointer < 0xc9f9) ||
                     (pointer >= 0xa4f3 && pointer < 0xa4f9) ||
                     (pointer >= 0xa345 && pointer < 0xa35d));
            },
            address => $"Speed-block PLM reread compiled source ${address:X6}.");
}
