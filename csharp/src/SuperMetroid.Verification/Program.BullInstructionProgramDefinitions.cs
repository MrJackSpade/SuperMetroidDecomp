using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and checks the compiled Bull instruction definitions against it.</summary>
    private static void VerifyBullInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with bank A8 and executes Bull's normal and immune-shot instruction paths.</summary>
    /// <param name="rom">Retail address space containing Bull's native instruction data.</param>
    private static void VerifyBullInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Static;
        for (int index = 0;
             index < BullInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                BullInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBullInstructionWord(rom, 0xa80000 | definition.Address),
                $"Bull instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new BullInstructionProgramReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBull", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        MethodInfo resolveShot = typeof(RoomEnemySystem).GetMethod(
            "ResolveBullImmuneShot", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.BullDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(slot);
        AssertEqual(BullInstructionProgramDefinitions.Normal, slot.CurrentInstruction,
            "Bull initializer program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int frame = 0; frame < 41; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull normal program completes its native animation loop");
        AssertEqual((ushort)10, slot.InstructionTimer,
            "Bull normal loop restores its ten-frame duration");

        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        resolveShot.Invoke(null, [slot, state, (ushort)2]);
        AssertEqual(BullInstructionProgramDefinitions.Shot, slot.CurrentInstruction,
            "Bull immune-shot program");
        for (int frame = 0; frame < 61; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull shot program repeats five times and returns to normal");
        AssertEqual((ushort)0, slot.Timer, "Bull shot loop exhausts its native repeat timer");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all Bull spritemap words select compiled presentation frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Bull mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd843),
            "Bull spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd871),
            "adjacent Bull shot-angle data is rejected as instruction mechanics");

        _ = ProbeBullInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBullInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bull allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bull mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Bull instruction mechanics: sixteen compiled words, the complete normal " +
            "loop, the five-cycle immune-shot response, and eight compiled visual selectors " +
            "pass with mechanics bytes forbidden.");
    }

    /// <summary>Repeatedly reads the normal-program entry word for the warmed-allocation check.</summary>
    /// <returns>A checksum that keeps each mechanics lookup observable.</returns>
    private static int ProbeBullInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += BullInstructionProgramDefinitions.ReadMechanicsWord(
                BullInstructionProgramDefinitions.Normal);
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from a bus address and its following byte.</summary>
    /// <param name="bus">Address space containing the instruction bytes.</param>
    /// <param name="address">Bus address of the low byte.</param>
    /// <returns>The word formed from the addressed byte and its successor.</returns>
    private static ushort ReadBullInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Detects production reads of compiled Bull mechanics and can reject reads of presentation selectors.</summary>
    /// <param name="source">Wrapped address space used for reads and writes permitted by the guard.</param>
    /// <param name="forbidPresentation">Whether presentation-selector reads should throw instead of being recorded.</param>
    private sealed class BullInstructionProgramReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets presentation-word offsets whose bytes were requested through this wrapper.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets attempts to read bytes owned by compiled Bull mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's read policy.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped source when permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled-mechanics reads, records or rejects presentation reads, and forwards other bytes.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The byte returned by the wrapped source when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (BullInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Bull mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BullInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BullInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Bull read cartridge visual selector $A8:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte written at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
