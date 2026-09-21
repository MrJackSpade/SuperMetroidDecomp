using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFallingSparkInstructionProgramDefinitions()
    {
        VerifyFallingSparkInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFallingSparkInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FallingSparkInstructionMechanicsWord definition =
                FallingSparkInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadFallingSparkInstructionWord(rom, definition.Address),
                $"Falling Spark mechanics word $86:{definition.Address:X4}");
        }

        var guard = new FallingSparkInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnFallingSpark", instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        var beginFloorImpact = typeof(RoomEnemySystem).GetMethod(
            "BeginFallingSparkFloorImpact",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot>>();

        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x0120;
        source.YPosition = 0x0060;
        spawn(source);
        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.Falling,
            projectile.InstructionPointer,
            "Falling Spark definition selects the named falling program");

        object?[] arguments = [projectile, null, (ushort)0, (ushort)0];
        for (int frame = 0; frame < 4; frame++)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual(
            unchecked((ushort)(FallingSparkInstructionProgramDefinitions.Falling + 4)),
            projectile.InstructionPointer,
            "falling program loops to its first timed frame");

        beginFloorImpact(projectile);
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.HitFloor,
            projectile.InstructionPointer,
            "floor collision selects the named impact program");
        for (int frame = 0; frame < 11; frame++)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete,
            projectile.InstructionPointer,
            "floor impact reaches the native terminal delete");
        projectile.InstructionTimer = 1;
        process.Invoke(enemies, arguments);
        AssertTrue(!projectile.IsActive,
            "floor impact deletes Falling Spark after all eleven blinking frames");

        AssertEqual(
            FallingSparkInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Falling Spark spritemap operands remain cartridge reads");
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(
                guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Falling Spark presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Falling Spark mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(0xf355),
            "Falling Spark spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(0xf391),
            "adjacent Falling Spark initializer code is rejected as mechanics");

        _ = ProbeFallingSparkInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFallingSparkInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Falling Spark allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Falling Spark mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Falling Spark instruction mechanics: seventeen compiled words, complete " +
            "falling/floor-impact execution, and fourteen live spritemap reads pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeFallingSparkInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FallingSparkInstructionProgramDefinitions.Falling
                    : FallingSparkInstructionProgramDefinitions.HitFloor);
        }
        return checksum;
    }

    private static ushort ReadFallingSparkInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class FallingSparkInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FallingSparkInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Falling Spark mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FallingSparkInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = FallingSparkInstructionProgramDefinitions
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
