using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the retail-ROM-backed checks for walking Space Pirate instruction programs.</summary>
    private static void VerifyWalkingSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native mechanics, movement and attack sequences, and compiled frame selectors.</summary>
    /// <param name="rom">Retail cartridge address space used to compare native instruction and presentation words.</param>
    private static void VerifyWalkingSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWalkingPirateWord(rom, 0xb20000 | definition.Address),
                $"walking Pirate mechanics word $B2:{definition.Address:X4}");
        }

        var guard = new WalkingSpacePirateInstructionReadGuard(rom);
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft,
            frames: 81,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingRight,
            frames: 81,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingLeft,
            frames: 17,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingRight,
            frames: 17,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingLeft,
            frames: 125,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingRight,
            frames: 125,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));

        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            var (enemies, slot, _, samus) = CreateSystem();
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = 1;
            process.Invoke(enemies,
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual(ReadWalkingPirateWord(rom, 0xb20000 | address),
                slot.SpritemapPointer,
                $"production execution selects walking Pirate frame $B2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled walking Pirate mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfb52),
            "walking Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfc68),
            "adjacent walking Pirate callback code is rejected as mechanics");

        _ = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "walking Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed walking Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Walking Space Pirate instruction mechanics: 92 compiled words, all eight " +
            "production programs, both three-laser attacks, and fifty compiled frame " +
            "selectors pass with source bytes forbidden.");

        void VerifyAttack(bool movingRight)
        {
            ushort program = movingRight
                ? WalkingSpacePirateInstructionProgramDefinitions.FireLasersRight
                : WalkingSpacePirateInstructionProgramDefinitions.FireLasersLeft;
            ushort expectedCursor = movingRight ? (ushort)0xfc16 : (ushort)0xfb94;
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.XPosition = movingRight ? (ushort)0x0180 : (ushort)0x0080;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 113);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate {(movingRight ? "right" : "left")} attack loops");
            AssertEqual(3, state.SpawnedLaserCount,
                $"walking Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0067, enemies.LastSpacePirateSoundEffect!.Value,
                $"walking Pirate {(movingRight ? "right" : "left")} laser sound");
        }

        void VerifyProgram(
            ushort program,
            int frames,
            ushort expectedCursor,
            WalkingSpacePirateFunction expectedFunction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, frames);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate program $B2:{program:X4} cursor");
            AssertEqual(expectedFunction, state.Function,
                $"walking Pirate program $B2:{program:X4} function");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WalkingSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GreyWalkingSpacePirateDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            typeof(RoomEnemySystem).GetMethod("InitializeWalkingSpacePirate", flags)!
                .Invoke(enemies, [slot]);
            var samus = new SamusState
            {
                XPosition = 0x0080,
                YPosition = 0x0100,
            };
            return (enemies, slot, enemies.WalkingSpacePirateStates[0]!, samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Exercises repeated mechanics lookups for the warmed allocation measurement.</summary>
    /// <returns>A checksum of the walking-program mechanics word to keep the probe observable.</returns>
    private static int ProbeWalkingSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from the supplied cartridge address space.</summary>
    /// <param name="bus">Cartridge address space containing the instruction bytes.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The two bytes combined as a 16-bit instruction word.</returns>
    private static ushort ReadWalkingPirateWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects runtime reads of compiled walking Pirate mechanics and visual selectors.</summary>
    /// <param name="source">Underlying cartridge address space for reads outside the protected tables.</param>
    private sealed class WalkingSpacePirateInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempts to read protected mechanics or presentation bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-byte requests through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address to read.</param>
        /// <returns>The underlying byte when it is outside the protected tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects protected mechanics and visual bytes, forwarding other byte reads.</summary>
        /// <param name="address">CPU-visible byte address to read.</param>
        /// <returns>The byte returned by the underlying address space when permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics or visual-selector data.</exception>
        public byte ReadByte(int address)
        {
            if (WalkingSpacePirateInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled walking Pirate mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xb20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled walking Pirate frame selector " +
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
