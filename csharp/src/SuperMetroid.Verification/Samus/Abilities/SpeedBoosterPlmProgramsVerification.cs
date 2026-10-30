using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterCollisionBlocksVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks Speed Booster block PLM selection, compiled programs, sound, and physical reveal behavior.</summary>
internal static class SpeedBoosterPlmProgramsVerification
{
    /// <summary>Checks the bomb-reveal draw identity, one-run geometry, physical word, and continuation bytes.</summary>
    private static void VerifySpeedBlockRevealDrawMapping(SuperMetroidAddressSpace rom)
    {
        const ushort pointer = 0xa4f3;
        var exported = SpeedBoosterBlockPlmDrawDefinitions.All.ToArray();
        AssertEqual(1,exported.Length,"speed reveal export count");
        AssertEqual(pointer,PlmDrawAddressCodec.Of(exported[0].Draw),"speed reveal export identity");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = SpeedBoosterBlockPlmDrawDefinitions.TryGet(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(address),out var draw);
            AssertEqual(address == pointer,found,"speed reveal full pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList),draw,"speed reveal missing draw cleared");
                AssertThrows<InvalidDataException>(() => SpeedBoosterBlockPlmDraws.VisualId(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>((ushort)address)),"speed reveal rejects unknown visual identity");
            }
        }
        AssertEqual("bomb-reveal",SpeedBoosterBlockPlmDraws.VisualId(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer)),"speed reveal stable semantic ID");
        AssertTrue(SpeedBoosterBlockPlmDraws.TryGetByVisualId("bomb-reveal",out var byId),"speed reveal exact ID admitted");
        foreach (string id in new[] {"", "Bomb-reveal", "bomb-reveal ", "unknown"})
        {
            AssertTrue(!SpeedBoosterBlockPlmDraws.TryGetByVisualId(id,out var missing),"speed reveal ordinal ID rejection");
            AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList),missing,"speed reveal missing ID clears output");
        }
        RoomTerrainBlock nativeTerrain = RoomLevelWordCodec.Decode(ReadRomWord(rom,0x840000 | (pointer + 2)));
        AssertEqual(nativeTerrain,SpeedBoosterBlockPlmDrawDefinitions.BombRevealTerrain,"speed reveal calculated physical terrain");
        foreach (var draw in new[] {exported[0],byId,SpeedBoosterBlockPlmDrawDefinitions.BombReveal})
        {
            AssertEqual(pointer,PlmDrawAddressCodec.Of(draw.Draw),"speed reveal DTO identity");
            AssertEqual(1,draw.Runs.Length,"speed reveal single run");
            var run = draw.Runs.Span[0];
            AssertRunHeader(ReadRomWord(rom,0x840000 | pointer),run,"speed reveal native direction/count");
            AssertEqual(1,run.Count,"speed reveal single word");
            AssertEqual(nativeTerrain,run.Blocks.Span[0],"speed reveal exported native terrain");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 4))),run.Next.X,"speed reveal native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 5))),run.Next.Y,"speed reveal native terminal Y");
        }
    }

    /// <summary>Checks BTS and area selection of the native Speed Booster PLM headers.</summary>
    private static void VerifySpeedBlockHeaderSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedBlockSelectionField), () => VerifySpeedBlockSelectionField(rom, instruction: false));

    /// <summary>Checks BTS and area selection of each header's initial instruction pointer.</summary>
    private static void VerifySpeedBlockInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedBlockSelectionField), () => VerifySpeedBlockSelectionField(rom, instruction: true));

    /// <summary>Compares the complete BTS/area selector domain with original bank-94 entry records.</summary>
    private static void VerifySpeedBlockSelectionField(SuperMetroidAddressSpace rom, bool instruction)
    {
        // Original bank-94 speed-entry locations, independent of named C# results.
        SpeedBoosterPlmProgramsEntry[] entries =
            [new SpeedBoosterPlmProgramsEntry(0x0e,-1,0x949155),new SpeedBoosterPlmProgramsEntry(0x0f,-1,0x949157),new SpeedBoosterPlmProgramsEntry(0x82,1,0x9491fd),
             new SpeedBoosterPlmProgramsEntry(0x83,1,0x9491ff),new SpeedBoosterPlmProgramsEntry(0x84,1,0x949201),new SpeedBoosterPlmProgramsEntry(0x85,1,0x949203)];
        for (int area = 0; area <= byte.MaxValue; area++)
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            int tableAddress = 0;
            foreach (var entry in entries)
                if (entry.Bts == bts && (entry.Area < 0 || entry.Area == area))
                    tableAddress = entry.Address;
            bool found = SpeedBoosterBlocks.TryResolve(new RoomBlockBehavior((byte)bts),
                (AreaId)(byte)area,out var definition);
            AssertEqual(tableAddress != 0,found,"speed selector full byte BTS/area contract");
            if (!found)
            {
                AssertEqual(default(SpeedBoosterBlockPlmDefinition),definition,"speed selector missing definition cleared");
                continue;
            }
            ushort header = ReadRomWord(rom,tableAddress);
            AssertEqual((ushort)0xcdea,ReadRomWord(rom,0x840000 | header),"native selected PLM is Speed Booster setup");
            if (instruction)
            {
                ushort list = ReadRomWord(rom,0x840000 | (header + 2));
                AssertEqual(RoomPlmNativeListAddresses.SpeedBoosterBlockEntries.Single(entry => entry.Entry == list).Program,
                    definition.Program, "speed native selected list starts its typed program");
            }
            else
                AssertEqual(header,PlmNativeWords.Word(definition.Header),"speed native selected header");
        }
    }

    /// <summary>Runs static mappings and live boosted-contact and bomb-reveal PLM paths without source reads.</summary>
    internal static void VerifyCompiledSpeedBoosterPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySpeedBlockHeaderSelection), () => VerifySpeedBlockHeaderSelection(rom));
        Suite(nameof(VerifySpeedBlockInstructionSelection), () => VerifySpeedBlockInstructionSelection(rom));
        Suite(nameof(RoomPlmProgramsVerification.VerifySpeedBoosterBlockPrograms), () => RoomPlmProgramsVerification.VerifySpeedBoosterBlockPrograms(rom));

        Suite(nameof(VerifySpeedBlockRevealDrawMapping), () => VerifySpeedBlockRevealDrawMapping(rom));

        var cases = new SpeedBoosterPlmProgramsCase[]
        {
            new SpeedBoosterPlmProgramsCase(new RoomBlockBehavior(0x0e), AreaId.Crateria, true),
            new SpeedBoosterPlmProgramsCase(new RoomBlockBehavior(0x0f), AreaId.Crateria, false),
            new SpeedBoosterPlmProgramsCase(new RoomBlockBehavior(0x82), AreaId.Brinstar, true),
            new SpeedBoosterPlmProgramsCase(new RoomBlockBehavior(0x83), AreaId.Brinstar, false),
            new SpeedBoosterPlmProgramsCase(new RoomBlockBehavior(0x84), AreaId.Brinstar, true),
        };
        foreach ((RoomBlockBehavior bts, AreaId area, bool respawns) in cases)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms,
                int blockIndex) = CreateSpeedBoosterBlockFixture(bts, area);
            var guarded = NewSpeedBoosterPlmReadGuard(bus);
            SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
            BlockMoveResult contact = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });
            AssertTrue(!contact.Collided,
                $"boosted contact with BTS ${bts.Value:X2} admits the real PLM");
            for (int frame = 0; frame < 100 && plms.SlotTable.ActiveCount != 0; frame++)
                plms.Step(level);
            AssertEqual(0, plms.SlotTable.ActiveCount,
                $"BTS ${bts.Value:X2} compiled list reaches native deletion");
            AssertEqual(respawns ? RoomCollisionType.SpecialBlock :
                    RoomCollisionType.Air,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"BTS ${bts.Value:X2} retains native restoration behavior");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"BTS ${bts.Value:X2} never rereads its compiled program or draw bytes");
        }

        (TestAddressSpace bombBus, RoomLevelData bombLevel,
            RoomPlmSystem bombPlms, int bombBlock) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x82),
                AreaId.Brinstar);
        var bombGuard = NewSpeedBoosterPlmReadGuard(bombBus);
        AssertTrue(bombPlms.BlockReactions.TrySpawnBombedSpecialBlock(bombLevel, bombBlock,
                new RoomBlockBehavior(0x82), AreaId.Brinstar, SamusProjectileType.Of(SamusProjectileFamily.Bomb)),
            "normal bomb allocates the Speed Booster reveal PLM");
        bombPlms.Step(bombLevel);
        AssertEqual(RoomTerrainBlock.Of(0x0b6, BlockMirroring.None, RoomCollisionType.SpecialBlock),
            bombLevel.GetCollisionBlockByIndex(bombBlock).Terrain,
            "bombed speed block draws the native physical reveal word");
        bombPlms.Step(bombLevel);
        AssertEqual(0, bombPlms.SlotTable.ActiveCount,
            "bombed speed-block reveal deletes after its single frame");
        AssertEqual(0, bombGuard.ForbiddenReadAttempts,
            "bombed Speed Booster reveal reads no compiled source bytes");

        Console.WriteLine(
            "Speed-block PLMs: the typed programs and one physical reveal match ROM; all five boosted contacts and bomb reveal execute with source reads forbidden.");
    }

    /// <summary>Rejects runtime reads from compiled Speed Booster programs and their physical draw lists.</summary>
    internal static CartridgeReadGuard NewSpeedBoosterPlmReadGuard(ISnesAddressSpace source) =>
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
