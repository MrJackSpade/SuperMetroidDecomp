using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWreckedShipGhostInstructionProgramDefinitions()
    {
        VerifyWreckedShipGhostInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWreckedShipGhostInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            WreckedShipGhostInstructionMechanicsWord definition =
                WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWreckedShipGhostInstructionWord(rom, definition.Address),
                $"Wrecked Ship ghost mechanics word $A8:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new WreckedShipGhostInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var initialize = type.GetMethod("InitializeWreckedShipGhost", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot ghost = enemies.Slots[0];
        ghost.EnemyDefinitionPointer = RoomEnemySystem.WreckedShipGhostDefinition;
        ghost.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(ghost);
        AssertEqual(WreckedShipGhostInstructionProgramDefinitions.Floating,
            ghost.CurrentInstruction,
            "real Wrecked Ship ghost initializer installs compiled floating program");

        object?[] processArguments =
            [ghost, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < 4; call++)
        {
            ghost.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }

        AssertEqual(unchecked((ushort)(
                WreckedShipGhostInstructionProgramDefinitions.Floating + 4)),
            ghost.CurrentInstruction,
            "Wrecked Ship ghost loop completes its native goto and first repeated frame");
        AssertEqual(WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Wrecked Ship ghost spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Wrecked Ship ghost mechanics byte");
        AssertThrows<InvalidDataException>(
            () => WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Wrecked Ship ghost spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                WreckedShipGhostInstructionProgramDefinitions.FirstAdjacentConstant),
            "adjacent ghost constants are rejected as instruction mechanics");

        _ = ProbeWreckedShipGhostInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWreckedShipGhostInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Wrecked Ship ghost allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Wrecked Ship ghost mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Wrecked Ship ghost instruction mechanics: five compiled words, the real " +
            "initializer, the complete floating loop, and three live spritemap reads " +
            "pass with mechanics bytes forbidden.");
    }

    private static int ProbeWreckedShipGhostInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? WreckedShipGhostInstructionProgramDefinitions.Floating
                    : WreckedShipGhostInstructionProgramDefinitions.LoopOpcode);
        }
        return checksum;
    }

    private static ushort ReadWreckedShipGhostInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class WreckedShipGhostInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WreckedShipGhostInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled ghost mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(index);
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
