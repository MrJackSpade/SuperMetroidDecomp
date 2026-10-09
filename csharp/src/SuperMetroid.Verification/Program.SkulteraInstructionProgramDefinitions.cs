using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the retail-ROM-backed verification of compiled Skultera instruction programs.</summary>
    private static void VerifySkulteraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native mechanics words, swimming and turning flows, visual selectors, and allocation-free lookup.</summary>
    /// <param name="rom">Retail cartridge address space used to compare compiled words with native data.</param>
    private static void VerifySkulteraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SkulteraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSkulteraInstructionWord(rom, 0xa30000 | definition.Address),
                $"Skultera instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new SkulteraInstructionProgramReadGuard(rom);
        RoomEnemySystem leftSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: true, out RoomEnemySlot left);
        RoomEnemySystem rightSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: false, out RoomEnemySlot right);

        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingLeft,
            left.CurrentInstruction, "Skultera initializer selects left-swimming program");
        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingRight,
            right.CurrentInstruction, "Skultera initializer selects right-swimming program");

        RunSkulteraProgram(leftSystem, left, frames: 45);
        RunSkulteraProgram(rightSystem, right, frames: 45);
        AssertEqual((ushort)2, left.Layer,
            "left-swimming Skultera program selects native layer two");
        AssertEqual((ushort)6, right.Layer,
            "right-swimming Skultera program selects native layer six");

        var beginTurn = typeof(RoomEnemySystem).GetMethod("BeginSkulteraTurn", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SkulteraEnemyState, bool>>();
        SkulteraEnemyState leftState = leftSystem.SkulteraStates[0]!;
        SkulteraEnemyState rightState = rightSystem.SkulteraStates[0]!;
        beginTurn(left, leftState, true);
        beginTurn(right, rightState, false);
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningRight,
            left.CurrentInstruction, "left swimmer enters turning-right program");
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningLeft,
            right.CurrentInstruction, "right swimmer enters turning-left program");

        RunSkulteraProgram(leftSystem, left, frames: 80);
        RunSkulteraProgram(rightSystem, right, frames: 80);
        AssertTrue(leftState.TurnFinished,
            "turning-right program publishes Skultera completion flag");
        AssertTrue(rightState.TurnFinished,
            "turning-left program publishes Skultera completion flag");
        AssertEqual((ushort)0x905e, left.CurrentInstruction,
            "turning-right program sleeps at its native terminal command");
        AssertEqual((ushort)0x9094, right.CurrentInstruction,
            "turning-left program sleeps at its native terminal command");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
            RoomEnemySystem enemies = CreateSkulteraProgramSystem(guard,
                startsLeft: true, out RoomEnemySlot slot);
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = 1;
            process.Invoke(enemies,
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual(ReadSkulteraInstructionWord(rom, 0xa30000 | address),
                slot.SpritemapPointer,
                $"production execution selects Skultera frame $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Skultera mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x902e),
            "interleaved Skultera spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x9096),
            "adjacent Skultera callback implementation is rejected as mechanics");

        _ = ProbeSkulteraInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkulteraInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Skultera allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Skultera mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Skultera instruction mechanics: thirty-two compiled words, both swimming " +
            "loops, both turning programs, layer/completion callbacks, sleeps, and " +
            "twenty-two compiled visual selectors pass with source bytes forbidden.");

        static RoomEnemySystem CreateSkulteraProgramSystem(
            SkulteraInstructionProgramReadGuard guard,
            bool startsLeft,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSkultera", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.SkulteraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.Parameter1 = startsLeft ? (ushort)0x0100 : (ushort)0x0000;
            slot.Parameter2 = 0x0120;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0200;
            initialize(slot);
            slot.InstructionTimer = 1;
            return enemies;
        }

        static void RunSkulteraProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Exercises repeated Skultera mechanics lookups for the warmed allocation measurement.</summary>
    /// <returns>A checksum of the swimming-program mechanics word to keep the probe observable.</returns>
    private static int ProbeSkulteraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkulteraInstructionProgramDefinitions.ReadMechanicsWord(
                SkulteraInstructionProgramDefinitions.SwimmingLeft);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from the supplied cartridge address space.</summary>
    /// <param name="bus">Cartridge address space containing the instruction bytes.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The two bytes combined as a 16-bit instruction word.</returns>
    private static ushort ReadSkulteraInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects runtime reads of compiled Skultera mechanics and visual selectors.</summary>
    /// <param name="source">Underlying cartridge address space for reads outside the protected tables.</param>
    private sealed class SkulteraInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of forbidden mechanics or visual-selector read attempts.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-byte requests through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address to read.</param>
        /// <returns>The underlying byte when it is outside the protected tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects protected mechanics and visual bytes, forwarding other byte reads.</summary>
        /// <param name="address">CPU-visible byte address to read.</param>
        /// <returns>The byte returned by the underlying address space when permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled mechanics or visual-selector table.</exception>
        public byte ReadByte(int address)
        {
            if (SkulteraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Skultera mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SkulteraInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Skultera visual selector " +
                            $"${address:X6}.");
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">CPU-visible byte address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
