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

/// <summary>Checks Noob Tube crack, shard, and released-air-bubble programs through their complete projectile lifetimes.</summary>
internal static class NoobTubeProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for Noob Tube projectile instruction verification.</summary>
    internal static void VerifyNoobTubeProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyNoobTubeProjectileInstructionProgramDefinitions), () => VerifyNoobTubeProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Compares 207 mechanics words and 90 visual operands, then exercises all burst parameters with runtime cartridge reads guarded.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Noob Tube projectile instruction program definitions")]
    internal static void VerifyNoobTubeProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(207,
            NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount,
            "n00b-tube catalog contains every mechanics word");
        for (int index = 0;
             index < NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"n00b-tube mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewNoobTubeProjectileInstructionReadGuard(rom);

        RoomEnemySystem crackSystem = NewSystem();
        RoomEnemyProjectileSlot crack = Spawn(
            crackSystem,
            RoomEnemyProjectileKind.NoobTubeCrack,
            parameter: 0);
        AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.Crack,
            crack.InstructionPointer,
            "real n00b-tube crack producer selects the named program");
        AssertEqual(48, RunThroughDeletion(crackSystem, crack, maximumTicks: 64),
            "n00b-tube crack completes 47 frames and its terminal deletion tick");

        for (ushort parameter = 0; parameter <= 18; parameter += 2)
        {
            RoomEnemySystem shardSystem = NewSystem();
            RoomEnemyProjectileSlot shard = Spawn(
                shardSystem,
                RoomEnemyProjectileKind.NoobTubeShard,
                parameter);
            AssertEqual(
                NoobTubeProjectileInstructionProgramDefinitions
                    .ShardInstructionLists[parameter / 2],
                shard.InstructionPointer,
                $"real n00b-tube shard producer selects parameter ${parameter:X2} program");
            AssertEqual(305, RunThroughDeletion(shardSystem, shard, maximumTicks: 320),
                $"n00b-tube shard ${parameter:X2} completes both counted flicker phases");
        }

        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            RoomEnemySystem bubbleSystem = NewSystem();
            RoomEnemyProjectileSlot bubble = Spawn(
                bubbleSystem,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
                parameter);
            AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble,
                bubble.InstructionPointer,
                $"real released-air-bubble producer selects parameter ${parameter:X2} program");
            AssertEqual(16, RunThroughDeletion(bubbleSystem, bubble, maximumTicks: 24),
                $"released-air bubble ${parameter:X2} completes fifteen frames and deletion");
        }

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = Spawn(
            shotSystem,
            RoomEnemyProjectileKind.NoobTubeCrack,
            parameter: 0);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = NativeCountdown.Start(1);
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "n00b-tube shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(90,
            NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            "n00b-tube catalog retains all ninety presentation operands");
        for (int index = 0;
             index < NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = NoobTubeProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production executes n00b-tube presentation $86:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86,address,out ushort selector),
                $"n00b-tube presentation $86:{address:X4} is compiled");
            AssertEqual(ReadVerificationWord(rom,0x860000 | address),selector,
                $"n00b-tube presentation $86:{address:X4} preserves its native operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared n00b-tube mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd3d9),
            "n00b-tube crack spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd69a),
            "released-air-bubble callback body is rejected as mechanics");

        _ = ProbeNoobTubeProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNoobTubeProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "n00b-tube allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed n00b-tube mechanics lookups allocate no storage");

        Console.WriteLine(
            "N00b-tube projectile instruction mechanics: 207 compiled words, all seventeen " +
            "real burst producers, complete crack/shard/bubble lifecycles, shared deletion, " +
            "and ninety executed native sprite compositions pass with zero live operand reads.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            system.BindFixtureBus(guard);
            system.VerificationServices().NextRandom = (Func<ushort>)(() => 0x4040);
            return system;
        }

        RoomEnemyProjectileSlot Spawn(
            RoomEnemySystem system,
            RoomEnemyProjectileKind kind,
            ushort parameter)
        {
            NoobTubeProjectileDefinition definition = kind switch
            {
                RoomEnemyProjectileKind.NoobTubeCrack => NoobTubeProjectileDefinition.Crack,
                RoomEnemyProjectileKind.NoobTubeShard => NoobTubeProjectileDefinition.Shard,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble =>
                    NoobTubeProjectileDefinition.ReleasedAirBubble,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            system.NoobTubeBehavior.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(definition, parameter, PlmBlockIndex: 42),
                roomWidthInBlocks: 16);
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(
            RoomEnemySystem system,
            RoomEnemyProjectileSlot projectile,
            int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                system.Projectiles.CurrentFrame8 = unchecked((byte)tick);
                projectile.InstructionTimer = NativeCountdown.Start(1);
                Process(system, projectile);
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException(
                $"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            ushort? expectedOperand = null;
            if (projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.NoobTubeShard)
            {
                // Native reflected flicker selects the first or second operand by
                // frame parity, then advances past both rather than one timed frame.
                ushort commandAddress = unchecked((ushort)(projectile.InstructionPointer - 6));
                ushort command = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x860000 | commandAddress);
                if (command == (ushort)EnemyProjectileInstruction.NoobTubeShardReflectFlicker)
                {
                    bool oddFrame = (system.Projectiles.CurrentFrame8 & 1) != 0;
                    expectedOperand = unchecked((ushort)(projectile.InstructionPointer - (oddFrame ? 4 : 2)));
                }
            }
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands, expectedOperand);
        }
    }

    /// <summary>Consumes warmed crack and bubble mechanics words for a lookup allocation check.</summary>
    private static int ProbeNoobTubeProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NoobTubeProjectileInstructionProgramDefinitions.Crack
                    : NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble);
        }
        return checksum;
    }

    /// <summary>Forbids private and shared projectile mechanics reads while observing visual operand reads.</summary>
    private static CartridgeReadGuard NewNoobTubeProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => NoobTubeProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled n00b-tube mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount, NoobTubeProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Noob Tube projectile instruction check against the local retail cartridge.</summary>
    [VerificationCommand("--noob-tube-projectile-instruction-mechanics")]
    internal static int NoobTubeProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNoobTubeProjectileInstructionProgramDefinitions), () => VerifyNoobTubeProjectileInstructionProgramDefinitions());
        return 0;
    }
}
