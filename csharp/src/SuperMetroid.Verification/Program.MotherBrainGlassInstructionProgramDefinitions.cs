using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the retail-ROM-backed checks for Mother Brain glass projectile instruction programs.</summary>
    private static void VerifyMotherBrainGlassInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainGlassInstructionProgramDefinitions), () => VerifyMotherBrainGlassInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native glass shard and sparkle timing while ensuring runtime visuals use compiled data.</summary>
    /// <param name="rom">Cartridge address space used by focused native instruction and presentation checks.</param>
    private static void VerifyMotherBrainGlassInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyGlassMechanicsAddresses), () => VerifyGlassMechanicsAddresses(rom));
        Suite(nameof(VerifyGlassPresentationAddresses), () => VerifyGlassPresentationAddresses(rom));
        Suite(nameof(VerifyGlassShardCadence), () => VerifyGlassShardCadence(rom));
        Suite(nameof(VerifyGlassSparkleCadence), () => VerifyGlassSparkleCadence(rom));
        Suite(nameof(VerifyGlassProjectileControls), () => VerifyGlassProjectileControls(rom));
        Suite(nameof(VerifyGlassProjectileLoopTargets), () => VerifyGlassProjectileLoopTargets(rom));

        var guard = new MotherBrainGlassInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnSparkle = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainGlassSparkle", flags)!;
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
            shardSystem.SpawnMotherBrainGlassProjectile(new(
                (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
                Parameter: 0,
                PlmBlockX: 9,
                PlmBlockY: 5));
            RoomEnemyProjectileSlot shard = shardSystem.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
            ushort program = MotherBrainGlassInstructionProgramDefinitions
                .ShardProgram(programIndex);
            AssertEqual(program, shard.InstructionPointer,
                $"real Mother Brain glass producer selects shard program {programIndex}");

            for (int frame = 0; frame < shardDurations.Length; frame++)
            {
                RunForcedTick(shardSystem, shard);
                AssertEqual(shardDurations[frame], shard.InstructionTimer,
                    $"glass shard program {programIndex} frame {frame} duration");
                AssertEqual(unchecked((ushort)(program + (frame + 1) * 4)),
                    shard.InstructionPointer,
                    $"glass shard program {programIndex} frame {frame} advance");
            }

            RunForcedTick(shardSystem, shard);
            AssertEqual(4, shard.InstructionTimer,
                $"glass shard program {programIndex} loop restarts first duration");
            AssertEqual(unchecked((ushort)(program + 4)), shard.InstructionPointer,
                $"glass shard program {programIndex} loops to its own first frame");
        }

        Queue<ushort> sparkleRandom = new([0, 8, 8, 16, 16]);
        RoomEnemySystem sparkleSystem = NewSystem(sparkleRandom);
        sparkleSystem.SpawnMotherBrainGlassProjectile(new(
            (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
            Parameter: 0,
            PlmBlockX: 9,
            PlmBlockY: 5));
        RoomEnemyProjectileSlot sparkleSource = sparkleSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
        spawnSparkle.Invoke(sparkleSystem, [sparkleSource]);
        RoomEnemyProjectileSlot sparkle = sparkleSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassSparkle);
        AssertEqual(MotherBrainGlassInstructionProgramDefinitions.Sparkle,
            sparkle.InstructionPointer,
            "real Mother Brain glass sparkle producer selects its named program");
        ushort[] sparkleDurations = [6, 8, 6, 8];
        for (int frame = 0; frame < sparkleDurations.Length; frame++)
        {
            RunForcedTick(sparkleSystem, sparkle);
            AssertEqual(sparkleDurations[frame], sparkle.InstructionTimer,
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
        shotSystem.SpawnMotherBrainGlassProjectile(new(
            (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
            Parameter: 0,
            PlmBlockX: 9,
            PlmBlockY: 5));
        RoomEnemyProjectileSlot shot = shotSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainGlassShard);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain glass shot reaction reaches shared compiled deletion");

        AssertEqual(68,
            MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount,
            "Mother Brain glass catalog retains all presentation operands");
        AssertEqual(0,
            guard.ObservedPresentationWords.Count,
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
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
                .SetValue(system, (Func<ushort>)(() =>
                    random.Count == 0 ? (ushort)0x4040 : random.Dequeue()));
            return system;
        }

        void RunForcedTick(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    /// <summary>Exercises repeated shard and sparkle mechanics lookups for the warmed allocation measurement.</summary>
    /// <returns>A checksum of the looked-up mechanics words to keep the probe observable.</returns>
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

    /// <summary>Rejects reads of compiled glass mechanics and records reads of presentation operands.</summary>
    /// <param name="source">Underlying address space for reads that the guard permits and for forwarded writes.</param>
    private sealed class MotherBrainGlassInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address to read.</param>
        /// <returns>The underlying byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Presentation word addresses observed while glass projectile instructions run.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to reread mechanics bytes that are supplied by compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Rejects compiled mechanics reads, records presentation operands, and forwards other byte reads.</summary>
        /// <param name="address">CPU-visible byte address to read.</param>
        /// <returns>The byte returned by the underlying address space when the read is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled glass mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (MotherBrainGlassInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mother Brain glass mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = MotherBrainGlassInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">CPU-visible byte address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
