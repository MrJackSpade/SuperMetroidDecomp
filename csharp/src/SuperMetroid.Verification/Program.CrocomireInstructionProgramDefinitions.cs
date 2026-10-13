using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static ImportedSpritemapsVerification;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled Crocomire body and skeleton programs against bank $A4 and a retail room fixture.</summary>
internal static class CrocomireInstructionProgramDefinitionsVerification
{
    /// <summary>Native Crocomire room used to exercise the installed enemy instruction runner.</summary>
    private const ushort CrocomireInstructionAuditRoom = 0xa98d;

    /// <summary>Loads the pinned cartridge and runs the Crocomire instruction comparison.</summary>
    internal static void VerifyCrocomireInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireInstructionProgramDefinitions), () => VerifyCrocomireInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native mechanics words and all executed body/skeleton visual selectors with runtime reads guarded.</summary>
    internal static void VerifyCrocomireInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CrocomireInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa40000 | definition.Address),
                $"Crocomire mechanics word $A4:{definition.Address:X4}");
        }

        var guard = NewCrocomireInstructionReadGuard(rom);
        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(
            rom,
            CrocomireInstructionAuditRoom);
        var installation = RepositoryInstallation.Installation;
        CartridgeRoomAssets assets = LoadFixtureRoomAssets(rom, room,
            installation.LoadRoomCharacters(), installation.LoadRoomPalettes(),
            installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0440, 0x0078)),
        };
        PrepareRetailSamusFixture(samus);
        samus.PoseMetadata.RefreshCollisionRadii(rom);
        samus.Presentation.InitializeAnimation(rom);

        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.Load(new RoomEnemyLoadRequest(guard, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, vram, cgram, new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber, Bosses = new FixtureAreaBossFlags { IsMiniBossDefeated = () => false, SetMiniBossDefeated = () => { } }, RoomScroll = new FixtureRoomScroll(assets.Scrolls.SetStorage) }) { Level = assets.LevelData, Samus = samus, Camera = new(0x0400, 0) });

        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        object?[] arguments =
            [body, samus, assets.LevelData, new Position16<RoomPixels>((ushort)0x0400, (ushort)0), (ushort)0];

        // Each presentation operand follows a compiled duration. Entering at that duration
        // runs the real interpreter without executing unrelated callbacks between frames.
        // Compare the selected sprite against the independent cartridge operand.
        var executedOperands = new HashSet<ushort>();
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort presentation =
                CrocomireInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            body.CurrentInstruction = unchecked((ushort)(presentation - 2));
            body.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            VerifyExecutedEnemySelector(rom, body, executedOperands);
            AssertEqual(presentation, unchecked((ushort)(body.CurrentInstruction - 2)),
                $"Crocomire presentation handoff $A4:{presentation:X4}");
        }

        AssertEqual(236, executedOperands.Count,
            "all executed Crocomire body/skeleton selectors match cartridge operands");
        AssertEqual(0, guard.ObservedWords.Count,
            "compiled Crocomire visual selectors require no runtime ROM reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Crocomire mechanics byte");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Crocomire spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitions.FirstExcludedUnreferencedProgram),
            "unreferenced Crocomire body program is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitions.FirstTongueProgram),
            "independently owned Crocomire tongue program is rejected as body mechanics");

        _ = ProbeCrocomireInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCrocomireInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Crocomire allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Crocomire mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Crocomire instruction mechanics: 434 compiled words across fight, reaction, " +
            "melting, bridge and skeleton programs; 236 executed selectors match the cartridge " +
            "with runtime reads forbidden.");
    }

    /// <summary>Repeats warmed body-program lookups for the caller's allocation measurement.</summary>
    private static int ProbeCrocomireInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CrocomireInstructionProgramDefinitions.Initial
                    : CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver);
        }
        return checksum;
    }

    /// <summary>Forbids compiled mechanics rereads and observes native spritemap operand access.</summary>
    private static CartridgeReadGuard NewCrocomireInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(CrocomireInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Crocomire mechanics byte ${address:X6}.")
            .ObserveWords(0xa40000, CrocomireInstructionProgramDefinitionsTooling.PresentationWordCount, CrocomireInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Dispatches the Crocomire instruction mechanics verification command.</summary>
    [VerificationCommand("--crocomire-instruction-mechanics")]
    internal static int CrocomireInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireInstructionProgramDefinitions), () => VerifyCrocomireInstructionProgramDefinitions());
        return 0;
    }
}
