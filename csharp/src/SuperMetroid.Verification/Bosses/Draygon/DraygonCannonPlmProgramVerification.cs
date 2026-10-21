using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
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
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon cannon PLM controls, visual layouts, and compiled damage programs.</summary>
internal static class DraygonCannonPlmProgramVerification
{
    /// <summary>Runs both retail cannon sequences through shield redraws and Super Missile damage under a program read guard.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon cannon PLM program")]
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
        var blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        blockDefinitions[0x54 * 8] = 0x54;
        var level = CreateRoom(new(width, 32),
            new ushort[width * 32], new byte[width * 32],
            blockDefinitions: blockDefinitions);
        var disabled = new List<DraygonCannonControlWord>();
        var plms = PlmFixtures.Unbound();
        var entries = DraygonCannonPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmVisualEntry(
                DraygonCannonPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.VisualWords())).ToArray();
        entries.Single(entry => entry.Id == "right-shield-a").Blocks[0] = 0x0053;
        entries.Single(entry => entry.Id == "left-shield-a").Blocks[0] = 0x0054;
        PlmFixtures.VisualsOf(plms).RoomPlmDraygonCannonVisuals = new RoomPlmDraygonCannonVisualCatalog(entries);
        AssertEqual(4, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new Bank80SystemState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(guarded, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { DraygonCannonDisabled = disabled.Add } }),
            "four retail cannon headers load with cartridge program lists");

        static void Step(RoomPlmSystem plms, ISnesAddressSpace bus,
            RoomLevelData level) =>
            // Keep both upper and lower shield origins inside the visible
            // tilemap ring so their immediate redraws are observable.
            plms.Step(level);

        Step(plms, guarded, level);
        int rightShieldBlock = 18 * width + 2;
        int leftShieldBlock = 15 * width + 29;
        AssertEqual((ushort)0xc514,
            level.GetCollisionBlockByIndex(rightShieldBlock).PackedWord.Raw,
            "edited right shield retains native physical collision and tile reference");
        AssertEqual((ushort)0x0053,
            level.DisplayedBlockTiles(rightShieldBlock).TopLeft.Raw,
            "edited right shield survives later room streaming");
        AssertEqual((ushort)0xc114,
            level.GetCollisionBlockByIndex(leftShieldBlock).PackedWord.Raw,
            "edited left shield retains native physical collision and tile reference");
        AssertEqual((ushort)0x0054,
            level.DisplayedBlockTiles(leftShieldBlock).TopLeft.Raw,
            "edited offscreen left shield enters later background streaming");
        AssertTrue(disabled.Contains(DraygonCannonControlWord.UpperLeft),
            "pre-destroyed cannon executes its retail damage-list entry");
        int rightBlock = rightShieldBlock;
        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0200),
            "Super Missile reaches live right cannon");
        var seenRightDamageWords = new HashSet<ushort>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level);
            seenRightDamageWords.Add(
                level.GetCollisionBlockByIndex(rightBlock).PackedWord.Raw);
        }
        foreach (ushort word in new ushort[] { 0xa580, 0xa581, 0xa582, 0xa583 })
            AssertTrue(seenRightDamageWords.Contains(word),
                $"right cannon damage loop draws compiled frame ${word:X4}");
        AssertTrue(disabled.Contains(DraygonCannonControlWord.LowerLeft),
            "right cannon follows native three-hit threshold to damaged list");
        int leftBlock = leftShieldBlock;
        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(leftBlock, 0x0200),
            "Super Missile reaches live left cannon");
        var seenLeftDamageWords = new HashSet<ushort>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level);
            seenLeftDamageWords.Add(
                level.GetCollisionBlockByIndex(leftBlock).PackedWord.Raw);
        }
        foreach (ushort word in new ushort[] { 0xa180, 0xa181, 0xa182, 0xa183 })
            AssertTrue(seenLeftDamageWords.Contains(word),
                $"left cannon damage loop draws compiled frame ${word:X4}");
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
                    DraygonCannonPlmDrawDefinitions.All.Any(draw =>
                    {
                        int start = 0x840000 | draw.Pointer;
                        int size = draw.Runs.Span.Sum(run =>
                            4 + run.LevelWords.Length * 2);
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
