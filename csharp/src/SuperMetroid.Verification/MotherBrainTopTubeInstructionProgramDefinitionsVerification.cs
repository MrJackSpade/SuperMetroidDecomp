using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Checks Mother Brain ceiling-tube projectile programs, sprites, and shared deletion.</summary>
internal static class MotherBrainTopTubeInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for ceiling-tube instruction checks.</summary>
    internal static void VerifyMotherBrainTopTubeInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Compares native words and executes four producers through display and terminal sleep.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain top Tube instruction program definitions")]
    internal static void VerifyMotherBrainTopTubeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Mother Brain ceiling-tube mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewMotherBrainTopTubeInstructionReadGuard(rom);

        foreach ((RoomEnemyProjectileKind Kind, ushort Program, ushort X, ushort Y) item in
                 new[]
                 {
                     (RoomEnemyProjectileKind.MotherBrainTopRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
                         (ushort)152, (ushort)47),
                     (RoomEnemyProjectileKind.MotherBrainTopLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
                         (ushort)104, (ushort)47),
                     (RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
                         (ushort)120, (ushort)59),
                     (RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
                         (ushort)136, (ushort)59),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            enemies.MotherBrainBehavior.SpawnMotherBrainTopTube(item.Kind, new Position16<RoomPixels>(item.X, item.Y));
            RoomEnemyProjectileSlot tube = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == item.Kind);
            AssertEqual(item.Program, tube.InstructionPointer,
                $"real {item.Kind} producer selects its named pose");
            AssertEqual(item.X, tube.Position.X.Whole, $"real {item.Kind} producer X position");
            AssertEqual(item.Y, tube.Position.Y.Whole, $"real {item.Kind} producer Y position");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} displays its pose before terminal sleep");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} terminal sleep retains its own opcode");
        }

        RoomEnemySystem shotSystem = NewSystem();
        shotSystem.MotherBrainBehavior.SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, new Position16<RoomPixels>(152, 47));
        RoomEnemyProjectileSlot shot = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainTopRightTube);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain ceiling-tube shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared ceiling-tube mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(0xcc45),
            "ceiling-tube spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(0xcc5b),
            "ceiling-tube definition data is rejected as mechanics");

        _ = ProbeMotherBrainTopTubeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainTopTubeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "ceiling-tube allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed ceiling-tube mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain ceiling-tube instruction mechanics: eight compiled words, all " +
            "four real producers, terminal sleeps, shared deletion, and four native " +
            "sprite compositions pass with zero live operand reads.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    /// <summary>Alternates two compiled tube words to expose allocation in warmed lookups.</summary>
    private static int ProbeMotherBrainTopTubeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MotherBrainTopTubeInstructionProgramDefinitions.TopRight
                    : MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight);
        }
        return checksum;
    }

    /// <summary>Blocks private and shared control bytes and observes visual operand reads.</summary>
    private static CartridgeReadGuard NewMotherBrainTopTubeInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MotherBrainTopTubeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Mother Brain ceiling-tube mechanics byte " +
                    $"${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount, MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Mother Brain ceiling-tube instruction mechanics command.</summary>
    [VerificationCommand("--mother-brain-top-tube-instruction-mechanics")]
    internal static int MotherBrainTopTubeInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions());
        return 0;
    }
}
