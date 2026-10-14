using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks save-station electricity's compiled animation loop and projectile lifecycle against bank 86.</summary>
internal static class SaveStationElectricityInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge to compare electricity instruction mechanics and frames.</summary>
    internal static void VerifySaveStationElectricityInstructionProgramDefinitions() =>
        Suite(nameof(VerifySaveStationElectricityInstructionProgramDefinitions), () => VerifySaveStationElectricityInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Exercises all 160 displayed frames, the terminal and shot deletion paths, and compiled sprite selectors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for save station electricity instruction program definitions")]
    internal static void VerifySaveStationElectricityInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"save-station electricity mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewSaveStationElectricityInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        enemies.Projectiles.SpawnSaveStationElectricity(plmBlockIndex: 42, roomWidthInBlocks: 16);
        RoomEnemyProjectileSlot electricity = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(SaveStationElectricityInstructionProgramDefinitions.Initial,
            electricity.InstructionPointer,
            "real save-station producer selects the named initial program");
        AssertEqual((ushort)0x00b0, electricity.Position.X.Whole,
            "save-station producer derives X from one block right of its PLM");
        AssertEqual((ushort)0x0000, electricity.Position.Y.Whole,
            "save-station producer derives Y from two blocks above its PLM");

        RunForcedTick();
        AssertEqual((ushort)0x0014, electricity.GeneralTimer,
            "save-station electricity initializes twenty animation cycles");
        AssertEqual((ushort)0xe68b, electricity.InstructionPointer,
            "save-station electricity displays the first loop frame");

        for (int tick = 1; tick < 160; tick++)
        {
            AssertTrue(electricity.IsActive,
                $"save-station electricity remains active before displayed frame {tick + 1}");
            RunForcedTick();
        }

        AssertTrue(electricity.IsActive,
            "save-station electricity remains active after all 160 displayed frames");
        AssertEqual((ushort)1, electricity.GeneralTimer,
            "save-station electricity retains one count before its terminal branch");
        AssertEqual((ushort)0xe6a7, electricity.InstructionPointer,
            "save-station electricity reaches the terminal decrement branch");
        RunForcedTick();
        AssertTrue(!electricity.IsActive,
            "save-station electricity exits its twentieth cycle through private deletion");

        RoomEnemySystem shotSystem = NewSystem();
        shotSystem.Projectiles.SpawnSaveStationElectricity(plmBlockIndex: 42, roomWidthInBlocks: 16);
        RoomEnemyProjectileSlot shot = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = NativeCountdown.Start(1);
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "save-station electricity shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "SaveStationElectricity execution performs no live spritemap operand reads");
        AssertEqual(SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "SaveStationElectricity executes every native visual operand");
        for (int index = 0; index < SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"SaveStationElectricity executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "SaveStationElectricity has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "SaveStationElectricity compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared save-station electricity mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(0xe689),
            "save-station electricity spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(0xe6ad),
            "save-station electricity initializer body is rejected as mechanics");

        _ = ProbeSaveStationElectricityInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSaveStationElectricityInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "save-station electricity allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed save-station electricity mechanics lookups allocate no storage");

        Console.WriteLine(
            "Save-station electricity instruction mechanics: thirteen compiled words, the " +
            "real producer, 160 displayed frames, both delete paths, and eight installed " +
            "sprite frames match native OAM.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            system.BindFixtureBus(guard);
            return system;
        }

        void RunForcedTick()
        {
            electricity.InstructionTimer = NativeCountdown.Start(1);
            Process(enemies, electricity);
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    /// <summary>Alternates the initial and loop words repeatedly to expose allocation in compiled lookups.</summary>
    private static int ProbeSaveStationElectricityInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SaveStationElectricityInstructionProgramDefinitions.Initial
                    : SaveStationElectricityInstructionProgramDefinitions.Loop);
        }
        return checksum;
    }

    /// <summary>Rejects private and shared projectile mechanics reads while recording live sprite operand reads.</summary>
    internal static CartridgeReadGuard NewSaveStationElectricityInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    SaveStationElectricityInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled save-station electricity mechanics byte " +
                    $"${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the electricity instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--save-station-electricity-instruction-mechanics")]
    internal static int SaveStationElectricityInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySaveStationElectricityInstructionProgramDefinitions), () => VerifySaveStationElectricityInstructionProgramDefinitions());
        return 0;
    }
}
