using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonCannonVisualsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.LookupCannonLayoutsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Checks Draygon cannon PLM controls, visual layouts, and compiled damage programs.</summary>
internal static class DraygonCannonPlmProgramVerification
{
    /// <summary>Runs both retail cannon sequences through shield redraws and Super Missile damage under a program read guard.</summary>
    internal static void VerifyDraygonCannonPlmProgram()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon oracle revision");
        BossRoomPlmProgramsVerification.VerifyDraygonCannonPrograms(rom);
        VerifyCannonLayoutGeometry(rom);
        VerifyCannonLayoutCollision(rom);
        VerifyCannonLayoutVisuals(rom);
        VerifyDraygonCannonVisualInstallation(rom);

        var source = new TestAddressSpace();
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        source.WriteBytes(0x848000, bank84);
        source.WriteBytes(0x8f9000,
        [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
        var guarded = NewDraygonCannonProgramReadGuard(source);
        const int width = 32;
        var level = CreateEmptyRoom(new(width, 32));
        var disabled = new List<DraygonCannonControlWord>();
        var plms = PlmFixtures.Unbound();
        var entries = DraygonCannonDraws.All.Select(draw =>
            new RoomPlmVisualEntry(
                DraygonCannonDraws.VisualId(draw.Draw),
                draw.Appearances())).ToArray();
        entries.Single(entry => entry.Id == "right-shield-a").Blocks[0] = RoomBlockAppearance.Of(0x53);
        entries.Single(entry => entry.Id == "left-shield-a").Blocks[0] = RoomBlockAppearance.Of(0x54);
        PlmFixtures.VisualsOf(plms).RoomPlmDraygonCannonVisuals = new RoomPlmDraygonCannonVisualCatalog(entries);
        AssertEqual(4, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(guarded, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { DraygonCannonDisabled = disabled.Add } }),
            "four retail cannon headers load with cartridge program lists");

        static void Step(RoomPlmSystem plms, ISnesAddressSpace bus,
            RoomLevelData level) =>
            // Keep both upper and lower shield origins inside the visible
            // tilemap ring so their immediate redraws are observable.
            plms.Step(level);

        Step(plms, guarded, level);
        int rightShieldBlock = 18 * width + 2;
        int leftShieldBlock = 15 * width + 29;
        AssertEqual(RoomTerrainBlock.Of(0x114, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
            level.GetCollisionBlockByIndex(rightShieldBlock).Terrain,
            "edited right shield retains native physical collision and tile reference");
        AssertEqual(RoomBlockAppearance.Of(0x53),
            level.DisplayedAppearance(rightShieldBlock),
            "edited right shield survives later room streaming");
        AssertEqual(RoomTerrainBlock.Of(0x114, BlockMirroring.None, RoomCollisionType.ShootableBlock),
            level.GetCollisionBlockByIndex(leftShieldBlock).Terrain,
            "edited left shield retains native physical collision and tile reference");
        AssertEqual(RoomBlockAppearance.Of(0x54),
            level.DisplayedAppearance(leftShieldBlock),
            "edited offscreen left shield enters later background streaming");
        AssertTrue(disabled.Contains(DraygonCannonControlWord.UpperLeft),
            "pre-destroyed cannon executes its retail damage-list entry");
        int rightBlock = rightShieldBlock;
        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0200),
            "Super Missile reaches live right cannon");
        var seenRightDamageBlocks = new HashSet<RoomTerrainBlock>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level);
            seenRightDamageBlocks.Add(
                level.GetCollisionBlockByIndex(rightBlock).Terrain);
        }
        foreach (int block in new[] { 0x180, 0x181, 0x182, 0x183 })
            AssertTrue(seenRightDamageBlocks.Contains(RoomTerrainBlock.Of(block, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock)),
                $"right cannon damage loop draws compiled mirrored spike block ${block:X3}");
        AssertTrue(disabled.Contains(DraygonCannonControlWord.LowerLeft),
            "right cannon follows native three-hit threshold to damaged list");
        int leftBlock = leftShieldBlock;
        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(leftBlock, 0x0200),
            "Super Missile reaches live left cannon");
        var seenLeftDamageBlocks = new HashSet<RoomTerrainBlock>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level);
            seenLeftDamageBlocks.Add(
                level.GetCollisionBlockByIndex(leftBlock).Terrain);
        }
        foreach (int block in new[] { 0x180, 0x181, 0x182, 0x183 })
            AssertTrue(seenLeftDamageBlocks.Contains(RoomTerrainBlock.Of(block, BlockMirroring.None, RoomCollisionType.SpikeBlock)),
                $"left cannon damage loop draws compiled spike block ${block:X3}");
        AssertTrue(disabled.Contains(DraygonCannonControlWord.UpperRight),
            "left cannon follows native threshold to damaged list");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail right/left cannon sequences do not reread compiled program or draw bytes");
        Console.WriteLine("  Draygon cannon PLM: both retail programs match ROM and execute through damage without program reads.");
    }

    /// <summary>Rejects live reads of both cannon instruction lists and their compiled draw records.</summary>
    private static CartridgeReadGuard NewDraygonCannonProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    (address >= 0x84dcde && address <= 0x84dd26) ||
                    (address >= 0x84ddb9 && address <= 0x84de01) ||
                    DraygonCannonDraws.All.Any(draw =>
                    {
                        int start = 0x840000 | PlmDrawAddressCodec.Of(draw.Draw);
                        int size = draw.Runs.Span.Sum(run =>
                            4 + run.Blocks.Length * 2);
                        return address >= start && address < start + size;
                    }),
                address => $"Draygon cannon reread compiled program byte ${address:X6}.");

    /// <summary>Runs the cannon PLM verification from the command dispatcher.</summary>
    [VerificationCommand("--draygon-cannon-plm-program")]
    internal static int DraygonCannonPlmProgramCommand()
    {
        Suite(nameof(VerifyDraygonCannonPlmProgram), () => VerifyDraygonCannonPlmProgram());
        return 0;
    }
}
