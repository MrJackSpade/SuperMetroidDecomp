using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs Work Robot laser mechanics and animation checks against the retail ROM in the verification directory.</summary>
    private static void VerifyWorkRobotLaserInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with cartridge data and exercises all five laser definitions through their animation and delete paths.</summary>
    /// <param name="rom">Retail address space used for expected instruction words and presentation operands.</param>
    private static void VerifyWorkRobotLaserInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WorkRobotLaserInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWorkRobotLaserInstructionWord(rom, definition.Address),
                $"Work Robot laser mechanics word $86:{definition.Address:X4}");
        }

        var observedInstalledOperands = new HashSet<ushort>();
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
            observedInstalledOperands.Count,
            "all Work Robot laser presentation operands select installed artwork");
        AssertEqual(0, guard.ObservedPresentationWords.Count, "Work Robot laser presentation requires no runtime cartridge reads");
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = WorkRobotLaserInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(observedInstalledOperands.Contains(address),
                $"production execution selects installed Work Robot laser presentation $86:{address:X4}");
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
            "installed presentation operands pass without runtime cartridge reads.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                ushort nativeCursor = projectile.InstructionPointer;
                // The final native Goto operand returns to the fourth frame, after the prefix.
                if (ReadWorkRobotLaserInstructionWord(rom, nativeCursor) == EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY)
                    nativeCursor = ReadWorkRobotLaserInstructionWord(rom, (ushort)(nativeCursor + 2));
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
                if (projectile.IsActive)
                {
                    ushort expectedOperand = (ushort)(nativeCursor + 2);
                    AssertEqual(expectedOperand, projectile.PresentationOperandAddress,
                        "Work Robot laser selects native presentation operand after instruction control flow");
                    AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(expectedOperand),
                        "Work Robot laser selected operand belongs to installed artwork");
                    AssertEqual(ReadWorkRobotLaserInstructionWord(rom, nativeCursor), projectile.InstructionTimer,
                        "Work Robot laser native duration survives calculated mechanics");
                    observedInstalledOperands.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    /// <summary>Repeats alternating initial and loop-command lookups for the warmed allocation check.</summary>
    /// <returns>A checksum that keeps both mechanics lookup paths observable.</returns>
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

    /// <summary>Reads a little-endian instruction word from the enemy-projectile code bank.</summary>
    /// <param name="source">Address space supplying the instruction bytes.</param>
    /// <param name="address">Bank-relative address of the low byte; the high byte follows with 16-bit address wrapping.</param>
    /// <returns>The combined 16-bit word.</returns>
    private static ushort ReadWorkRobotLaserInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects runtime reads of compiled laser mechanics and records reads of installed presentation operands.</summary>
    /// <param name="source">Underlying address space for permitted reads and forwarded writes.</param>
    private sealed class WorkRobotLaserInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation word addresses observed through the guard in the enemy-projectile bank.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempts to read mechanics bytes supplied by compiled laser or shared-delete definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the checked byte-read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The underlying byte when the address is not compiled mechanics.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation-operand reads, and delegates other addresses.</summary>
        /// <param name="address">Address requested from the wrapped SNES address space.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (WorkRobotLaserInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
