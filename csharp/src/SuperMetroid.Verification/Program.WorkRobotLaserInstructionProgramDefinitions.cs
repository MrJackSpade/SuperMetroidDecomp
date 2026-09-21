using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWorkRobotLaserInstructionProgramDefinitions()
    {
        VerifyWorkRobotLaserInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWorkRobotLaserInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            WorkRobotLaserInstructionMechanicsWord definition =
                WorkRobotLaserInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWorkRobotLaserInstructionWord(rom, definition.Address),
                $"Work Robot laser mechanics word $86:{definition.Address:X4}");
        }

        var guard = new WorkRobotLaserInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnWorkRobotLaser", instanceFlags)!;

        var robot = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            XRadius = 12,
            YRadius = 16,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new WorkRobotEnemyState(robot);
        var definitions = new (ushort Definition, ushort XVelocity)[]
        {
            (WorkRobotLaserDefinitions.UpLeft, unchecked((ushort)-0x0200)),
            (WorkRobotLaserDefinitions.Horizontal, unchecked((ushort)-0x0200)),
            (WorkRobotLaserDefinitions.DownLeft, unchecked((ushort)-0x0200)),
            (WorkRobotLaserDefinitions.UpRight, 0x0200),
            (WorkRobotLaserDefinitions.DownRight, 0x0200),
        };

        foreach ((ushort definition, ushort xVelocity) in definitions)
        {
            state.LaserXVelocity = xVelocity;
            spawn.Invoke(enemies, [robot, state, definition, (ushort)0, (ushort)0]);
        }

        RoomEnemyProjectileSlot[] lasers = enemies.EnemyProjectiles
            .Where(projectile => WorkRobotLaserInstructionProgramDefinitions.Owns(projectile.Kind))
            .ToArray();
        AssertEqual(5, lasers.Length,
            "all five real Work Robot laser definitions spawn through production code");
        foreach (RoomEnemyProjectileSlot laser in lasers)
        {
            AssertEqual(WorkRobotLaserInstructionProgramDefinitions.Initial,
                laser.InstructionPointer,
                "real Work Robot laser producer selects the named animation prefix");
            RunForcedTicks(laser, 8);
            AssertEqual(
                unchecked((ushort)(WorkRobotLaserInstructionProgramDefinitions.Loop + 4)),
                laser.InstructionPointer,
                "Work Robot laser completes its prefix and full loop before returning");
            AssertTrue(laser.IsActive,
                "Work Robot laser loop remains active until collision or a shot deletes it");
        }

        lasers[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(lasers[0], 1);
        AssertTrue(!lasers[0].IsActive,
            "Work Robot laser shot reaction reaches the compiled shared delete program");

        AssertEqual(WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Work Robot laser spritemap operands remain cartridge reads");
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = WorkRobotLaserInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Work Robot laser presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Work Robot laser and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(0xd2ee),
            "Work Robot laser spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(0xd30c),
            "adjacent Work Robot laser initializer is rejected as mechanics");

        _ = ProbeWorkRobotLaserInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWorkRobotLaserInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Work Robot laser allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Work Robot laser mechanics lookups allocate no storage");

        Console.WriteLine(
            "Work Robot laser instruction mechanics: nine compiled words, all five real " +
            "definitions, complete prefix/loop execution, shared shot deletion, and seven " +
            "live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeWorkRobotLaserInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? WorkRobotLaserInstructionProgramDefinitions.Initial
                    : WorkRobotLaserInstructionProgramDefinitions.LoopCommand);
        }
        return checksum;
    }

    private static ushort ReadWorkRobotLaserInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class WorkRobotLaserInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WorkRobotLaserInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Work Robot laser mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = WorkRobotLaserInstructionProgramDefinitions
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
