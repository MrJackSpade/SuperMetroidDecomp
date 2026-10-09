using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the retail-ROM-backed comparison for Powamp's compiled instruction programs.</summary>
    private static void VerifyPowampInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words, initializes the balloon and body, and executes their instruction loops.</summary>
    /// <param name="rom">Retail address space used to compare compiled words and executed visual operands.</param>
    private static void VerifyPowampInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PowampInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PowampInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPowampInstructionWord(rom, definition.Address),
                $"Powamp mechanics word $A8:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PowampInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializePowamp", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot balloon = enemies.Slots[0];
        balloon.EnemyDefinitionPointer = RoomEnemySystem.PowampDefinition;
        balloon.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        balloon.Parameter1 = 1;
        initialize(balloon);
        AssertEqual(PowampInstructionProgramDefinitions.BalloonDeflated,
            balloon.CurrentInstruction,
            "real Powamp balloon initializer installs compiled deflated program");

        RoomEnemySlot body = enemies.Slots[1];
        body.EnemyDefinitionPointer = RoomEnemySystem.PowampDefinition;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(body);
        AssertEqual(PowampInstructionProgramDefinitions.BodySlow,
            body.CurrentInstruction,
            "real Powamp body initializer installs compiled slow program");

        object?[] bodyArguments =
            [body, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, process, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodySlow + 4)),
            body.CurrentInstruction,
            "Powamp slow body loop completes its native goto and first repeated frame");

        body.CurrentInstruction = PowampInstructionProgramDefinitions.BodyFast;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, process, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodyFast + 4)),
            body.CurrentInstruction,
            "Powamp fast body loop completes its native goto and first repeated frame");

        object?[] balloonArguments =
            [balloon, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonInflate0;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, process, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonInflate2 + 4)),
            balloon.CurrentInstruction,
            "Powamp inflate program reaches terminal sleep");

        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonStartSinking;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, process, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonDeflated + 4)),
            balloon.CurrentInstruction,
            "Powamp deflate program falls through to shared deflated sleep");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(PowampInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        for (int index = 0; index < PowampInstructionProgramDefinitions.PresentationWordCount; index++)
            AssertTrue(executedOperands.Contains(PowampInstructionProgramDefinitions.PresentationWordAddress(index)),
                "every Powamp visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Powamp mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PowampInstructionProgramDefinitions.ReadMechanicsWord(
                PowampInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Powamp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PowampInstructionProgramDefinitions.ReadMechanicsWord(
                PowampInstructionProgramDefinitions.FirstAdjacentConstant),
            "adjacent Powamp constants are rejected as instruction mechanics");

        _ = ProbePowampInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePowampInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Powamp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Powamp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Powamp instruction mechanics: 18 compiled words, both body loops, the " +
            "inflate/deflate fallthrough programs, and 12 native sprite selections pass " +
            "with mechanics bytes forbidden.");
    }

    /// <summary>Runs eligible instruction calls and validates each visual selector reached by the selected slot.</summary>
    /// <param name="rom">Retail address space used to validate executed selector values.</param>
    /// <param name="executedOperands">Set that accumulates the addresses of executed presentation words.</param>
    /// <param name="enemies">System instance on which the reflected processor runs.</param>
    /// <param name="process">Reflected instruction-processing method.</param>
    /// <param name="arguments">Argument array passed to each processor invocation.</param>
    /// <param name="slot">Powamp slot whose timer is reset before each call.</param>
    /// <param name="calls">Number of eligible processor calls to make.</param>
    private static void ExecutePowampInstructionCalls(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            VerifyExecutedEnemySelector(rom, slot, executedOperands);
        }
    }

    /// <summary>Repeats lookups of representative body and balloon mechanics words for the warmed allocation check.</summary>
    /// <returns>A checksum that consumes the resolved words.</returns>
    private static int ProbePowampInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PowampInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PowampInstructionProgramDefinitions.BodyFast
                    : PowampInstructionProgramDefinitions.BalloonDeflatedSleep);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from bank $A8 of the retail ROM.</summary>
    /// <param name="source">Retail address space containing the reference bytes.</param>
    /// <param name="address">Bank-local offset of the low byte.</param>
    /// <returns>The adjacent cartridge bytes combined as a 16-bit value.</returns>
    private static ushort ReadPowampInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects runtime reads from compiled Powamp mechanics and records attempted presentation reads.</summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class PowampInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Distinct presentation-word offsets whose bytes production attempted to read.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected for targeting a compiled mechanics byte.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the same checks as ordinary address-space reads.</summary>
        /// <param name="address">SNES cartridge address requested by the caller.</param>
        /// <returns>The underlying byte when the address is not a compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation reads, and delegates other addresses.</summary>
        /// <param name="address">SNES address requested by the caller.</param>
        /// <returns>The byte supplied by the wrapped address space when permitted.</returns>
        /// <exception cref="InvalidOperationException">The address targets a compiled Powamp mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (PowampInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Powamp mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PowampInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        PowampInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>Forwards a write unchanged because this verification wrapper guards reads only.</summary>
        /// <param name="address">SNES address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
