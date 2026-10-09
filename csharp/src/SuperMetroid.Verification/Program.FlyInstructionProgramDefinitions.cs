using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail cartridge image and runs the fly-family instruction-definition checks.
    /// </summary>
    private static void VerifyFlyInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled fly mechanics with cartridge data and exercises initialization,
    /// instruction-loop behavior, and the absence of runtime reads for compiled mechanics.
    /// </summary>
    /// <param name="rom">The retail address space used as the reference for compiled mechanics.</param>
    private static void VerifyFlyInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FlyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FlyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadFlyInstructionWord(rom, 0xa20000 | definition.Address),
                $"fly-family instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new FlyInstructionProgramReadGuard(rom);
        foreach (ushort definition in new ushort[]
                 {
                     RoomEnemySystem.MellowDefinition,
                     RoomEnemySystem.MellaDefinition,
                     RoomEnemySystem.MemuDefinition,
                 })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeFly", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(slot);
            AssertEqual(FlyInstructionProgramDefinitions.Flight, slot.CurrentInstruction,
                $"fly ${definition:X4} initializer program");

            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < 9; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(FlyInstructionProgramDefinitions.Flight + 4)),
                slot.CurrentInstruction,
                $"fly ${definition:X4} completes native animation loop");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "fly-family animation uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled fly-family mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb015),
            "fly-family spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb027),
            "adjacent unused movement data is rejected as mechanics");

        _ = ProbeFlyInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFlyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "fly-family allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed fly-family mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fly-family instruction mechanics: six compiled words, all three real " +
            "initializers, the complete loop, and four compiled spritemap selectors pass " +
            "without instruction or presentation ROM reads.");
    }

    /// <summary>
    /// Repeatedly resolves the flight instruction's mechanics word to measure warmed lookup
    /// allocations while consuming the returned values.
    /// </summary>
    /// <returns>A checksum of the mechanics values read during the probe.</returns>
    private static int ProbeFlyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += FlyInstructionProgramDefinitions.ReadMechanicsWord(
                FlyInstructionProgramDefinitions.Flight);
        return checksum;
    }

    /// <summary>
    /// Reads a little-endian instruction word from two adjacent cartridge bytes.
    /// </summary>
    /// <param name="bus">The address space containing the instruction data.</param>
    /// <param name="address">The absolute address of the word's low byte.</param>
    /// <returns>The word formed from the low byte at <paramref name="address"/> and the next byte.</returns>
    private static ushort ReadFlyInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps the cartridge address space to detect forbidden fly mechanics reads and record
    /// accesses to compiled presentation words while the real enemy code runs.
    /// </summary>
    /// <param name="source">The underlying address space to which permitted reads and writes are forwarded.</param>
    private sealed class FlyInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank A2 presentation-word addresses whose bytes were requested through this guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>The number of attempts to read bytes compiled into the fly mechanics definition.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Applies the guard's read checks before forwarding a cartridge-byte request.</summary>
        /// <param name="address">The absolute cartridge address to read.</param>
        /// <returns>The byte supplied by the underlying address space when the read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads of compiled fly mechanics, tracks reads of compiled presentation words,
        /// and forwards other permitted reads.
        /// </summary>
        /// <param name="address">The absolute address to read.</param>
        /// <returns>The byte supplied by the underlying address space.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled fly mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (FlyInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled fly-family mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FlyInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FlyInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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
        /// <param name="address">The absolute address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
