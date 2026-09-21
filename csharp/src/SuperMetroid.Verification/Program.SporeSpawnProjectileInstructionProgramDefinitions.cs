using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySporeSpawnProjectileInstructionProgramDefinitions() =>
        VerifySporeSpawnProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifySporeSpawnProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SporeSpawnProjectileInstructionMechanicsWord definition =
                SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Spore Spawn projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new SporeSpawnProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 1));
        var samus = new SamusState();
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
            enemies,
            samus);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawnSpawner = typeof(RoomEnemySystem).GetMethod(
            "SpawnSporeSpawnSpawner", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var spawnStalk = typeof(RoomEnemySystem).GetMethod(
            "SpawnSporeSpawnStalk", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);

        RoomEnemySlot body = enemies.Slots[0];
        body.XPosition = 128;
        body.YPosition = 256;
        spawnSpawner(body, 0);
        RoomEnemyProjectileSlot spawner = Single(RoomEnemyProjectileKind.SporeSpawnSpawner);
        Run(spawner, 2);
        AssertEqual((ushort)0xdc04, spawner.InstructionPointer,
            "closed ceiling emitter reaches terminal sleep");
        AssertEqual((ushort)0, spawner.InstructionTimer,
            "closed ceiling emitter remains dormant at sleep");
        spawner.InstructionPointer = SporeSpawnProjectileInstructionProgramDefinitions.SpawnerRelease;
        Run(spawner, 6);
        AssertEqual((ushort)0xdc1c, spawner.InstructionPointer,
            "ceiling emitter completes its release and reaches sleep");

        RoomEnemyProjectileSlot spore = Single(RoomEnemyProjectileKind.SporeSpawnSpore);
        Run(spore, 4);
        AssertEqual((ushort)0xdc22, spore.InstructionPointer,
            "airborne spore completes and restarts its three-pose loop");

        spawnStalk(body, 0);
        RoomEnemyProjectileSlot stalk = Single(RoomEnemyProjectileKind.SporeSpawnStalk);
        Run(stalk, 2);
        AssertEqual((ushort)0xdc32, stalk.InstructionPointer,
            "stalk reaches terminal sleep after its display frame");

        spore.InstructionPointer = SporeSpawnProjectileInstructionProgramDefinitions.SporeShot;
        spore.InstructionTimer = 1;
        Run(spore, 8);
        AssertTrue(!spore.IsActive,
            "shot spore completes its seven-frame explosion/drop path and deletes");
        AssertEqual(1, enemies.SporeSpawnDropRequests.Count,
            "shot spore publishes exactly one drop request");
        AssertEqual((ushort)0, spore.Damage,
            "shot spore property callback clears damage before deletion");

        AssertEqual(SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Spore Spawn projectile spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Spore Spawn projectile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdc02),
            "Spore Spawn spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdc5a),
            "Spore Spawn callback body is rejected as mechanics");

        _ = ProbeSporeSpawnProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSporeSpawnProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Spore Spawn allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Spore Spawn projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Spore Spawn projectile instruction mechanics: twenty-eight compiled words, " +
            "all three real producers, complete release/loop/shot paths, and seventeen " +
            "live spritemap reads pass with mechanics bytes forbidden.");

        RoomEnemyProjectileSlot Single(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.First(projectile => projectile.Kind == kind);

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeSporeSpawnProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SporeSpawnProjectileInstructionProgramDefinitions.SpawnerClosed
                    : SporeSpawnProjectileInstructionProgramDefinitions.SporeShot);
        }
        return checksum;
    }

    private sealed class SporeSpawnProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SporeSpawnProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Spore Spawn mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = SporeSpawnProjectileInstructionProgramDefinitions
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
