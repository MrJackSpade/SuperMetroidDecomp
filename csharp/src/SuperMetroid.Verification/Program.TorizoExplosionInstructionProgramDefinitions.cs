using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoExplosionInstructionProgramDefinitions() =>
        VerifyTorizoExplosionInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyTorizoExplosionInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoExplosionInstructionMechanicsWord definition =
                TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo explosion mechanics word $86:{definition.Address:X4}");
        }

        var guard = new TorizoExplosionInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        var lowRandom = new Queue<ushort>(
            [0xc000, 7, 9, 0x0000, 1, 2, 0x8000, 3, 4]);
        RoomEnemySystem lowEnemies = CreateSystem(guard, lowRandom);
        RoomEnemySlot lowTorizo = CreateTorizo(lowEnemies);
        lowTorizo.Parameter1 = 0x8000;
        typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLowHealthExplosion", flags)!.Invoke(
                lowEnemies,
                [lowTorizo, (ushort)0]);
        RoomEnemyProjectileSlot low = lowEnemies.EnemyProjectiles[^1];
        AssertEqual(TorizoExplosionInstructionProgramDefinitions.LowHealthInitial,
            low.InstructionPointer, "low-health explosion selects compiled program");
        AssertEqual((ushort)1012, low.Variable0, "low-health explosion retained X center");
        AssertEqual((ushort)492, low.Variable1, "low-health explosion retained Y center");

        for (int frame = 1; frame <= 36; frame++)
        {
            process.Invoke(lowEnemies, [low, null, (ushort)0, (ushort)0]);
            AssertTrue(low.IsActive,
                $"low-health explosion remains active through frame {frame}");
            if (frame == 1)
            {
                AssertEqual((ushort)1005, low.XPosition,
                    "low-health explosion first negative X jitter");
                AssertEqual((ushort)483, low.YPosition,
                    "low-health explosion first negative Y jitter");
                AssertEqual((ushort)3, low.GeneralTimer,
                    "low-health explosion installs three-cycle timer");
            }
            else if (frame == 13)
            {
                AssertEqual((ushort)1013, low.XPosition,
                    "low-health explosion second X jitter resets around center");
                AssertEqual((ushort)494, low.YPosition,
                    "low-health explosion second Y jitter resets around center");
                AssertEqual((ushort)2, low.GeneralTimer,
                    "low-health explosion second cycle timer");
            }
            else if (frame == 25)
            {
                AssertEqual((ushort)1009, low.XPosition,
                    "low-health explosion third X jitter resets around center");
                AssertEqual((ushort)496, low.YPosition,
                    "low-health explosion third Y jitter resets around center");
                AssertEqual((ushort)1, low.GeneralTimer,
                    "low-health explosion final cycle timer");
            }
        }
        process.Invoke(lowEnemies, [low, null, (ushort)0, (ushort)0]);
        AssertTrue(!low.IsActive,
            "low-health explosion deletes after three exact twelve-frame cycles");
        AssertEqual(0, lowRandom.Count,
            "low-health explosion consumes three random words per cycle");

        VerifyDeathPath(
            branchSample: 0,
            random: new Queue<ushort>(
                [0, 0xc000, 7, 25, 0x0000, 1, 20]),
            visibleFrames: 62,
            expectedFirst: (993, 491),
            expectedSecond: (1001, 504),
            secondCycleFrame: 32,
            "large explosions");
        VerifyDeathPath(
            branchSample: 0xc000,
            random: new Queue<ushort>(
                [0xc000, 0x8000, 8, 9, 0x4000, 2, 12]),
            visibleFrames: 64,
            expectedFirst: (992, 505),
            expectedSecond: (1002, 492),
            secondCycleFrame: 33,
            "smoke");

        AssertEqual(TorizoExplosionInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Torizo explosion spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo explosion mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa3e2),
            "Torizo explosion spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa3df),
            "Torizo explosion packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa456),
            "adjacent probability callback body is rejected as explosion mechanics");

        _ = ProbeTorizoExplosionInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoExplosionInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo explosion allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo explosion mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo explosion instruction mechanics: fifty-three compiled words, the " +
            "real low-health producer's three cycles, and both probabilistic death paths " +
            "pass with exact jitter, lifetimes, and mechanics bytes forbidden.");

        void VerifyDeathPath(
            ushort branchSample,
            Queue<ushort> random,
            int visibleFrames,
            (int X, int Y) expectedFirst,
            (int X, int Y) expectedSecond,
            int secondCycleFrame,
            string scenario)
        {
            AssertEqual(branchSample, random.Peek(),
                $"Torizo death {scenario} branch sample fixture");
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = CreateTorizo(enemies);
            typeof(RoomEnemySystem).GetMethod(
                "SpawnBombTorizoDeathExplosion", flags)!.Invoke(enemies, [torizo]);
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];
            AssertEqual(TorizoExplosionInstructionProgramDefinitions.DeathInitial,
                projectile.InstructionPointer,
                $"Torizo death {scenario} selects compiled program");

            for (int frame = 1; frame <= visibleFrames; frame++)
            {
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
                AssertTrue(projectile.IsActive,
                    $"Torizo death {scenario} remains active through frame {frame}");
                if (frame == 1)
                {
                    AssertEqual(unchecked((ushort)expectedFirst.X), projectile.XPosition,
                        $"Torizo death {scenario} first X jitter");
                    AssertEqual(unchecked((ushort)expectedFirst.Y), projectile.YPosition,
                        $"Torizo death {scenario} first Y jitter");
                    AssertEqual((ushort)2, projectile.GeneralTimer,
                        $"Torizo death {scenario} installs two-cycle timer");
                }
                else if (frame == secondCycleFrame)
                {
                    AssertEqual(unchecked((ushort)expectedSecond.X), projectile.XPosition,
                        $"Torizo death {scenario} second X jitter resets around center");
                    AssertEqual(unchecked((ushort)expectedSecond.Y), projectile.YPosition,
                        $"Torizo death {scenario} second Y jitter resets around center");
                    AssertEqual((ushort)1, projectile.GeneralTimer,
                        $"Torizo death {scenario} final cycle timer");
                }
            }
            process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            AssertTrue(!projectile.IsActive,
                $"Torizo death {scenario} deletes after its second complete cycle");
            AssertEqual(0, random.Count,
                $"Torizo death {scenario} consumes branch plus six jitter RNG words");
        }

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus, Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            return enemies;
        }

        static RoomEnemySlot CreateTorizo(RoomEnemySystem enemies)
        {
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            return torizo;
        }
    }

    private static int ProbeTorizoExplosionInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoExplosionInstructionProgramDefinitions.LowHealthInitial
                    : TorizoExplosionInstructionProgramDefinitions.DeathInitial);
        }
        return checksum;
    }

    private sealed class TorizoExplosionInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TorizoExplosionInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo explosion byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoExplosionInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoExplosionInstructionProgramDefinitions
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
