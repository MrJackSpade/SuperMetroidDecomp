using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoLandingDustInstructionProgramDefinitions() =>
        VerifyTorizoLandingDustInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyTorizoLandingDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoLandingDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TorizoLandingDustInstructionMechanicsWord definition =
                TorizoLandingDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo landing-dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = new TorizoLandingDustInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLandingDust", flags)!
            .CreateDelegate<Action<RoomEnemySlot, bool>>(enemies);

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 300;
        torizo.YPosition = 120;
        spawn(torizo, true);
        spawn(torizo, false);
        RoomEnemyProjectileSlot right = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoRightFootDust);
        RoomEnemyProjectileSlot left = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoLeftFootDust);

        AssertEqual((ushort)324, right.XPosition, "right-foot dust producer X position");
        AssertEqual((ushort)276, left.XPosition, "left-foot dust producer X position");
        AssertEqual((ushort)168, right.YPosition, "right-foot dust producer Y position");
        AssertEqual((ushort)168, left.YPosition, "left-foot dust producer Y position");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.RightFoot,
            right.InstructionPointer, "right-foot dust program selection");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.LeftFoot,
            left.InstructionPointer, "left-foot dust program selection");

        Run(right, 4);
        Run(left, 4);
        AssertEqual((ushort)156, right.YPosition,
            "right-foot dust rises exactly twelve pixels across its four poses");
        AssertEqual((ushort)156, left.YPosition,
            "left-foot dust rises exactly twelve pixels across its four poses");
        Run(right, 1);
        Run(left, 1);
        AssertTrue(!right.IsActive && !left.IsActive,
            "both landing-dust programs delete after their fourth pose");

        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Torizo landing-dust spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo landing-dust mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf9f),
            "Torizo landing-dust spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf92),
            "Torizo landing-dust callback body is rejected as mechanics");

        _ = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo landing-dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo landing-dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo landing-dust instruction mechanics: sixteen compiled words, both " +
            "real foot producers, exact twelve-pixel rise/deletion, and eight live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        void Run(RoomEnemyProjectileSlot projectile, int steps)
        {
            for (int step = 0; step < steps; step++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeTorizoLandingDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoLandingDustInstructionProgramDefinitions.RightFoot
                    : TorizoLandingDustInstructionProgramDefinitions.LeftFoot);
        }
        return checksum;
    }

    private sealed class TorizoLandingDustInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TorizoLandingDustInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo landing-dust byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoLandingDustInstructionProgramDefinitions
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
