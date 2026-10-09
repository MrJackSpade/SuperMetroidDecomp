using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers and runs Falling Spark instruction checks against the retail cartridge.</summary>
    private static void VerifyFallingSparkInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled mechanics words with bank-$86 data, executes the falling and
    /// floor-impact projectile programs, and verifies installed presentation selectors
    /// are used without reading their cartridge operands at runtime.
    /// </summary>
    /// <param name="rom">Retail address space used to verify source mechanics words.</param>
    private static void VerifyFallingSparkInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FallingSparkInstructionProgramDefinitionsTooling.MechanicsWord(index);
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

        var observedOperands = new HashSet<ushort>();
        object?[] arguments = [projectile, null, (ushort)0, (ushort)0];
        for (int frame = 0; frame < 4; frame++)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            AssertEqual((ushort)(FallingSparkInstructionProgramDefinitions.Falling + 2 + (frame % 3) * 4),
                projectile.PresentationOperandAddress, "falling spark frame order and loop restart");
            AssertEqual((ushort)3, projectile.InstructionTimer, "falling spark frame duration");
            observedOperands.Add(projectile.PresentationOperandAddress);
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
            AssertEqual((ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + 2 + frame * 4),
                projectile.PresentationOperandAddress, "falling spark impact blink frame order");
            AssertEqual((ushort)1, projectile.InstructionTimer, "falling spark impact frame duration");
            observedOperands.Add(projectile.PresentationOperandAddress);
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
            observedOperands.Count,
            "all Falling Spark installed visual operands are selected");
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(
                observedOperands.Contains(address),
                $"production execution selects Falling Spark presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Falling Spark presentation does not read cartridge bytes");
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
            "falling/floor-impact execution, and fourteen installed visual operands pass " +
            "with mechanics bytes forbidden.");
    }

    /// <summary>Warms repeated compiled mechanics lookups and returns a checksum that consumes their results.</summary>
    /// <returns>The accumulated values of alternating falling and floor-impact mechanics words.</returns>
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

    /// <summary>Reads one little-endian instruction word from the enemy-projectile code bank.</summary>
    /// <param name="source">Address space containing the projectile instruction bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadFallingSparkInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Tracks reads of Falling Spark presentation operands and throws if execution tries
    /// to fetch mechanics bytes that are supplied by compiled definitions.
    /// </summary>
    /// <param name="source">Address space used to resolve permitted reads and forward writes.</param>
    private sealed class FallingSparkInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes import-source reads through the same tracking and mechanics rejection behavior.</summary>
        /// <param name="address">Cartridge bus address requested by the importer.</param>
        /// <returns>The wrapped address space's byte when the read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Falling Spark selector-word addresses observed in bank $86.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of compiled-mechanics byte reads rejected by the guard.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Rejects compiled-mechanics reads, records selector reads, and forwards other bytes.</summary>
        /// <param name="address">Address requested by production instruction processing.</param>
        /// <returns>The wrapped address space's byte when the read is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Falling Spark mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (FallingSparkInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
