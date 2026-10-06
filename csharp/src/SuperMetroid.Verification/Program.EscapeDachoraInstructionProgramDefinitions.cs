using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeDachoraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyEscapeDachoraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EscapeDachoraInstructionMechanicsWord definition =
                EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadEscapeDachoraInstructionWord(rom, 0xb30000 | definition.Address),
                $"escape Dachora mechanics word $B3:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = new EscapeDachoraInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateEscapeDachoraProgramSystem(guard, flags);
        RoomEnemySlot dachora = enemies.Slots[0];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

        ushort initialX = dachora.XPosition;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningAroundLowTide;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, process, enemies, dachora, 70);
        AssertTrue(dachora.XPosition != initialX,
            "escape Dachora low-tide pacing runs movement callbacks");

        initialX = dachora.XPosition;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningAroundHighTide;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, process, enemies, dachora, 70);
        AssertTrue(dachora.XPosition != initialX,
            "escape Dachora high-tide pacing runs movement callbacks");

        initialX = dachora.XPosition;
        dachora.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningForEscape;
        RunForcedEscapeDachoraInstructions(rom, executedOperands, process, enemies, dachora, 30);
        AssertTrue(unchecked((short)(dachora.XPosition - initialX)) > 0,
            "escape Dachora departure moves right through its accelerating callbacks");
        AssertTrue(dachora.CurrentInstruction is
                >= EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed and
                <= EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed +
                    0x0024,
            "escape Dachora departure reaches its maximum-speed loop");

        AssertEqual(EscapeDachoraInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production covers escape-Dachora presentation word $B3:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"escape-Dachora spritemap $B3:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled escape-Dachora mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeDachoraInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent escape-Dachora callback code is rejected as mechanics");

        _ = ProbeEscapeDachoraInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEscapeDachoraInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "escape-Dachora allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed escape-Dachora mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Escape Dachora instruction mechanics: 119 compiled words, low/high-tide " +
            "pacing, accelerating departure and 43 executed selectors match cartridge data with runtime " +
            "ROM reads forbidden.");
    }

    private static RoomEnemySystem CreateEscapeDachoraProgramSystem(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_hasEvent", flags)!
            .SetValue(enemies, (Func<EventNumber, bool>)(_ => false));
        RoomEnemySlot dachora = enemies.Slots[0];
        dachora.EnemyDefinitionPointer = RoomEnemySystem.EscapeDachoraDefinition;
        dachora.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
        dachora.XPosition = 0x0100;
        dachora.YPosition = 0x00c8;
        typeof(RoomEnemySystem).GetMethod("InitializeEscapeDachora", flags)!
            .Invoke(enemies, [dachora]);
        return enemies;
    }

    private static void RunForcedEscapeDachoraInstructions(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot dachora,
        int steps)
    {
        object?[] arguments =
            [dachora, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int step = 0; step < steps; step++)
        {
            dachora.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            VerifyExecutedEnemySelector(rom, dachora, executedOperands);
        }
    }

    private static ushort ReadEscapeDachoraInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static int ProbeEscapeDachoraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeDachoraInstructionProgramDefinitions.RunningForEscapeMaximumSpeed);
        }
        return checksum;
    }

    private sealed class EscapeDachoraInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (EscapeDachoraInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled escape-Dachora mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = EscapeDachoraInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
