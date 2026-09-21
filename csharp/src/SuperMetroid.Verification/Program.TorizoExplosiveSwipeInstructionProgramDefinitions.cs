using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoExplosiveSwipeInstructionProgramDefinitions() =>
        VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoExplosiveSwipeInstructionMechanicsWord definition =
                TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo explosive-swipe mechanics word $86:{definition.Address:X4}");
        }

        var guard = new TorizoExplosiveSwipeInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoExplosiveSwipe", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 1000;
        torizo.YPosition = 500;
        torizo.Parameter1 = 0;
        spawn(torizo, 0);
        RoomEnemyProjectileSlot swipe = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoExplosiveSwipe);
        AssertEqual(TorizoExplosiveSwipeInstructionProgramDefinitions.Initial,
            swipe.InstructionPointer, "real Torizo swipe selects compiled program");
        AssertEqual((ushort)970, swipe.XPosition, "real Torizo swipe initial X");
        AssertEqual((ushort)448, swipe.YPosition, "real Torizo swipe initial Y");

        for (int frame = 0; frame < 25; frame++)
        {
            process.Invoke(enemies, [swipe, null, (ushort)0, (ushort)0]);
            AssertTrue(swipe.IsActive,
                $"Torizo explosive swipe remains active through frame {frame + 1}");
        }
        process.Invoke(enemies, [swipe, null, (ushort)0, (ushort)0]);
        AssertTrue(!swipe.IsActive,
            "Torizo explosive swipe deletes on the tick after five five-frame poses");

        AssertEqual(TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Torizo explosive-swipe spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo explosive-swipe mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(0xa4af),
            "Torizo explosive-swipe spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(0xa4a8),
            "adjacent Torizo program data is rejected as explosive-swipe mechanics");

        _ = ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo explosive-swipe allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo explosive-swipe mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo explosive-swipe instruction mechanics: seven compiled words, the " +
            "real producer's exact 25-frame lifetime, and five live spritemap reads pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoExplosiveSwipeInstructionProgramDefinitions.Initial
                    : (ushort)0xa4c1);
        }
        return checksum;
    }

    private sealed class TorizoExplosiveSwipeInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TorizoExplosiveSwipeInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo explosive-swipe byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoExplosiveSwipeInstructionProgramDefinitions
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
