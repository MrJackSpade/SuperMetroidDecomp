using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static LookupGlassProjectileProgramVerification;
using static SuiteTimingVerification;

internal static class MotherBrainGlassInstructionProgramDefinitionsVerification
{
    internal static void VerifyMotherBrainGlassInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainGlassInstructionProgramDefinitions), () => VerifyMotherBrainGlassInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    internal static void VerifyMotherBrainGlassInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyGlassMechanicsAddresses), () => VerifyGlassMechanicsAddresses(rom));
        Suite(nameof(VerifyGlassPresentationAddresses), () => VerifyGlassPresentationAddresses(rom));
        Suite(nameof(VerifyGlassShardCadence), () => VerifyGlassShardCadence(rom));
        Suite(nameof(VerifyGlassSparkleCadence), () => VerifyGlassSparkleCadence(rom));
        Suite(nameof(VerifyGlassProjectileControls), () => VerifyGlassProjectileControls(rom));
        Suite(nameof(VerifyGlassProjectileLoopTargets), () => VerifyGlassProjectileLoopTargets(rom));

        var guard = NewMotherBrainGlassInstructionReadGuard(rom);
        ushort[] selectorIndexes = [0, 1, 3, 6, 8, 9, 11, 14];
        ushort[] shardDurations = [4, 3, 2, 3, 4, 3, 2, 3];

        for (int programIndex = 0;
             programIndex < MotherBrainGlassInstructionProgramDefinitions.ShardProgramCount;
             programIndex++)
        {
            ushort selectorIndex = selectorIndexes[programIndex];
            Queue<ushort> random = new([
                unchecked((ushort)(selectorIndex << 4)),
                8,
                8,
            ]);
            RoomEnemySystem shardSystem = NewSystem(random);
            shardSystem.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(new(
                (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
                Parameter: 0,
                PlmBlock: new(9, 5)));
            RoomEnemyProjectileSlot shard = shardSystem.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
            ushort program = MotherBrainGlassInstructionProgramDefinitions
                .ShardProgram(programIndex);
            AssertEqual(program, shard.InstructionPointer,
                $"real Mother Brain glass producer selects shard program {programIndex}");

            for (int frame = 0; frame < shardDurations.Length; frame++)
            {
                RunForcedTick(shardSystem, shard);
                AssertEqual(shardDurations[frame], shard.InstructionTimer.RawValue,
                    $"glass shard program {programIndex} frame {frame} duration");
                AssertEqual(unchecked((ushort)(program + (frame + 1) * 4)),
                    shard.InstructionPointer,
                    $"glass shard program {programIndex} frame {frame} advance");
            }

            RunForcedTick(shardSystem, shard);
            AssertEqual(4, shard.InstructionTimer.RawValue,
                $"glass shard program {programIndex} loop restarts first duration");
            AssertEqual(unchecked((ushort)(program + 4)), shard.InstructionPointer,
                $"glass shard program {programIndex} loops to its own first frame");
        }

        Queue<ushort> sparkleRandom = new([0, 8, 8, 16, 16]);
        RoomEnemySystem sparkleSystem = NewSystem(sparkleRandom);
        sparkleSystem.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(new(
            (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
            Parameter: 0,
            PlmBlock: new(9, 5)));
        RoomEnemyProjectileSlot sparkleSource = sparkleSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
        sparkleSystem.MotherBrainBehavior.SpawnMotherBrainGlassSparkle(sparkleSource);
        RoomEnemyProjectileSlot sparkle = sparkleSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassSparkle);
        AssertEqual(MotherBrainGlassInstructionProgramDefinitions.Sparkle,
            sparkle.InstructionPointer,
            "real Mother Brain glass sparkle producer selects its named program");
        ushort[] sparkleDurations = [6, 8, 6, 8];
        for (int frame = 0; frame < sparkleDurations.Length; frame++)
        {
            RunForcedTick(sparkleSystem, sparkle);
            AssertEqual(sparkleDurations[frame], sparkle.InstructionTimer.RawValue,
                $"Mother Brain glass sparkle frame {frame} duration");
            AssertEqual(unchecked((ushort)(
                    MotherBrainGlassInstructionProgramDefinitions.Sparkle + (frame + 1) * 4)),
                sparkle.InstructionPointer,
                $"Mother Brain glass sparkle frame {frame} advance");
        }
        RunForcedTick(sparkleSystem, sparkle);
        AssertTrue(!sparkle.IsActive,
            "Mother Brain glass sparkle reaches its private compiled deletion");

        Queue<ushort> shotRandom = new([0, 8, 8]);
        RoomEnemySystem shotSystem = NewSystem(shotRandom);
        shotSystem.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(new(
            (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
            Parameter: 0,
            PlmBlock: new(9, 5)));
        RoomEnemyProjectileSlot shot = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain glass shot reaction reaches shared compiled deletion");

        AssertEqual(68,
            MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount,
            "Mother Brain glass catalog retains all presentation operands");
        AssertEqual(0,
            guard.ObservedWords.Count,
            "Mother Brain glass artwork operands no longer read the cartridge");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared Mother Brain glass mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord(0xcc95),
            "Mother Brain shard spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord(0xcdc5),
            "adjacent Mother Brain shard definition is rejected as mechanics");

        _ = ProbeMotherBrainGlassInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainGlassInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mother Brain glass allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mother Brain glass mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain glass instruction mechanics: 85 compiled words, all eight real " +
            "shard loops, real sparkle lifetime, shared deletion, and 68 calculated spritemap operand addresses with zero cartridge " +
            "reads pass.");

        RoomEnemySystem NewSystem(Queue<ushort> random)
        {
            var system = new RoomEnemySystem();
            system.BindFixtureBus(guard);
            system.VerificationServices().NextRandom = (Func<ushort>)(() =>
                    random.Count == 0 ? (ushort)0x4040 : random.Dequeue());
            return system;
        }

        void RunForcedTick(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
        }
    }

    private static int ProbeMotherBrainGlassInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MotherBrainGlassInstructionProgramDefinitions.ShardGroup0
                    : MotherBrainGlassInstructionProgramDefinitions.Sparkle);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewMotherBrainGlassInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MotherBrainGlassInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Mother Brain glass mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount, MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress);
}
