using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainTopTubeInstructionProgramDefinitions() =>
        VerifyMotherBrainTopTubeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyMotherBrainTopTubeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < MotherBrainTopTubeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MotherBrainTopTubeInstructionMechanicsWord definition =
                MotherBrainTopTubeInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Mother Brain ceiling-tube mechanics word $86:{definition.Address:X4}");
        }

        var guard = new MotherBrainTopTubeInstructionReadGuard(rom);
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTopTube", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

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
            spawn.Invoke(enemies, [item.Kind, item.X, item.Y]);
            RoomEnemyProjectileSlot tube = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == item.Kind);
            AssertEqual(item.Program, tube.InstructionPointer,
                $"real {item.Kind} producer selects its named pose");
            AssertEqual(item.X, tube.XPosition, $"real {item.Kind} producer X position");
            AssertEqual(item.Y, tube.YPosition, $"real {item.Kind} producer Y position");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} displays its pose before terminal sleep");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} terminal sleep retains its own opcode");
        }

        RoomEnemySystem shotSystem = NewSystem();
        spawn.Invoke(shotSystem,
            [RoomEnemyProjectileKind.MotherBrainTopRightTube, (ushort)152, (ushort)47]);
        RoomEnemyProjectileSlot shot = shotSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainTopRightTube);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain ceiling-tube shot reaction reaches shared compiled deletion");

        AssertEqual(MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Mother Brain ceiling-tube spritemap operands remain cartridge reads");
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
            "four real producers, terminal sleeps, shared deletion, and four live " +
            "spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

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

    private sealed class MotherBrainTopTubeInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MotherBrainTopTubeInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mother Brain ceiling-tube mechanics byte " +
                    $"${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MotherBrainTopTubeInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = MotherBrainTopTubeInstructionProgramDefinitions
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
