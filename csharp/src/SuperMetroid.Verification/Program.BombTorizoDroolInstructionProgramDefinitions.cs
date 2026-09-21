using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBombTorizoDroolInstructionProgramDefinitions() =>
        VerifyBombTorizoDroolInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyBombTorizoDroolInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < BombTorizoDroolInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BombTorizoDroolInstructionMechanicsWord definition =
                BombTorizoDroolInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Bomb Torizo drool mechanics word $86:{definition.Address:X4}");
        }

        var guard = new BombTorizoDroolInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        ushort[] expectedPrograms =
        [
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay,
            BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay,
        ];

        for (int selection = 0; selection < expectedPrograms.Length; selection++)
        {
            var random = new Queue<ushort>(
                [unchecked((ushort)(selection << 2)), 0x0037]);
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 0x0400;
            torizo.YPosition = 0x0200;
            torizo.Parameter1 = 0x8000;
            typeof(RoomEnemySystem).GetMethod(
                "SpawnBombTorizoLowHealthDrool", flags)!.Invoke(enemies, [torizo]);

            RoomEnemyProjectileSlot drool = enemies.EnemyProjectiles[^1];
            AssertEqual(expectedPrograms[selection], drool.InstructionPointer,
                $"drool selector {selection} initial program");
            AssertEqual(0, random.Count,
                $"drool selector {selection} consumes selection and trajectory RNG");
            AssertEqual((ushort)0x0408, drool.XPosition,
                $"drool selector {selection} X origin");
            AssertEqual((ushort)0x01fb, drool.YPosition,
                $"drool selector {selection} Y origin");

            int delay = expectedPrograms[selection] switch
            {
                BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay => 4,
                BombTorizoDroolInstructionProgramDefinitions.TwoFrameDelay => 2,
                _ => 0,
            };
            for (int frame = 0; frame < delay; frame++)
            {
                process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
                AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                    $"drool selector {selection} stays high priority during delay {frame + 1}");
            }
            process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual(BombTorizoDroolInstructionProgramDefinitions.FallingPreInstruction,
                drool.PreInstruction,
                $"drool selector {selection} installs falling callback after delay");
            AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                $"drool selector {selection} first five-frame pose is high priority");

            for (int frame = 1; frame < 5; frame++)
                process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual(EnemyProjectileDrawPriority.High, drool.DrawPriority,
                $"drool selector {selection} keeps priority through five visible frames");
            process.Invoke(enemies, [drool, null, (ushort)0, (ushort)0]);
            AssertEqual(EnemyProjectileDrawPriority.Low, drool.DrawPriority,
                $"drool selector {selection} clears priority before 64-frame loop");
            AssertEqual((ushort)0x0040, drool.InstructionTimer,
                $"drool selector {selection} installs exact loop duration");
        }

        var initialRandom = new Queue<ushort>([0x001f, 0x0003]);
        RoomEnemySystem initialEnemies = CreateSystem(guard, initialRandom);
        RoomEnemySlot initialTorizo = initialEnemies.Slots[0];
        initialTorizo.XPosition = 0x0500;
        initialTorizo.YPosition = 0x0300;
        initialTorizo.Parameter1 = 0x4000;
        typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoInitialDrool", flags)!.Invoke(initialEnemies, [initialTorizo]);
        RoomEnemyProjectileSlot initial = initialEnemies.EnemyProjectiles[^1];
        AssertEqual(BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            initial.InstructionPointer, "initial gut-break drool bypasses delay selector");
        AssertEqual((ushort)0x02fe, initial.YPosition, "initial drool random Y origin");
        AssertEqual((ushort)79, initial.YVelocity, "initial drool random Y velocity");
        AssertEqual((ushort)0x0503, initial.XPosition, "initial drool turning X origin");
        AssertEqual(0, initialRandom.Count, "initial drool consumes exactly two RNG words");

        initial.InstructionPointer = BombTorizoDroolInstructionProgramDefinitions.WallImpact;
        initial.InstructionTimer = 1;
        process.Invoke(initialEnemies, [initial, null, (ushort)0, (ushort)0]);
        AssertTrue(!initial.IsActive, "drool wall impact clears callback and deletes immediately");

        var floorRandom = new Queue<ushort>([0, 0]);
        RoomEnemySystem floorEnemies = CreateSystem(guard, floorRandom);
        RoomEnemySlot floorTorizo = floorEnemies.Slots[0];
        floorTorizo.XPosition = 0x0500;
        floorTorizo.YPosition = 0x0300;
        typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoInitialDrool", flags)!.Invoke(floorEnemies, [floorTorizo]);
        RoomEnemyProjectileSlot floor = floorEnemies.EnemyProjectiles[^1];
        floor.PreInstruction = BombTorizoDroolInstructionProgramDefinitions.FallingPreInstruction;
        floor.InstructionPointer = BombTorizoDroolInstructionProgramDefinitions.FloorImpact;
        floor.InstructionTimer = 1;
        for (int frame = 0; frame < 24; frame++)
        {
            process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
            AssertTrue(floor.IsActive,
                $"drool floor impact remains active through frame {frame + 1}");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, floor.PreInstruction,
                $"drool floor impact callback is clear on frame {frame + 1}");
        }
        process.Invoke(floorEnemies, [floor, null, (ushort)0, (ushort)0]);
        AssertTrue(!floor.IsActive,
            "drool floor impact deletes on the tick after three eight-frame poses");

        AssertEqual(BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Bomb Torizo drool spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Bomb Torizo drool mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(0xa47c),
            "Bomb Torizo drool spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(0xa49e),
            "adjacent unused Torizo program is rejected as drool mechanics");

        _ = ProbeBombTorizoDroolInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBombTorizoDroolInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bomb Torizo drool allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bomb Torizo drool mechanics lookups allocate no storage");

        Console.WriteLine(
            "Bomb Torizo drool instruction mechanics: nineteen compiled words, all " +
            "eight native delay selections, exact two-word RNG sequencing, priority " +
            "handoff, and both impact lifetimes pass with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(
            ISnesAddressSpace bus,
            Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)random.Dequeue);
            return enemies;
        }
    }

    private static int ProbeBombTorizoDroolInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? BombTorizoDroolInstructionProgramDefinitions.NoDelay
                    : BombTorizoDroolInstructionProgramDefinitions.FloorImpact);
        }
        return checksum;
    }

    private sealed class BombTorizoDroolInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BombTorizoDroolInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Bomb Torizo drool byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = BombTorizoDroolInstructionProgramDefinitions
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
