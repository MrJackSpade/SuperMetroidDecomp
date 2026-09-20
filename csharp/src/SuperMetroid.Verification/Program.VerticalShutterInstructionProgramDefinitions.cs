using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyVerticalShutterInstructionProgramDefinitions()
    {
        VerifyVerticalShutterInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyVerticalShutterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < VerticalShutterInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            VerticalShutterInstructionMechanicsWord definition =
                VerticalShutterInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadVerticalShutterInstructionWord(rom, 0xa20000 | definition.Address),
                $"vertical-shutter instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new VerticalShutterInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = enemySystemType.GetMethod("InitializeVerticalShutter", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;
        object?[] processArguments =
            [null, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        foreach (ushort definitionPointer in new ushort[]
                 {
                     RoomEnemySystem.ShootableVerticalShutterDefinition,
                     RoomEnemySystem.DestroyableVerticalShutterDefinition,
                 })
        {
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = 0;
            slot.Parameter1 = 0x0002;
            initialize(slot);
            AssertEqual(VerticalShutterInstructionProgramDefinitions.Plain,
                slot.CurrentInstruction,
                $"vertical shutter ${definitionPointer:X4} installs plain program");
            processArguments[0] = slot;
            process.Invoke(enemies, processArguments);
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(
                    VerticalShutterInstructionProgramDefinitions.Plain + 4)),
                slot.CurrentInstruction,
                $"vertical shutter ${definitionPointer:X4} reaches terminal sleep");
        }

        RoomEnemySlot kamer = enemies.Slots[0];
        kamer.EnemyDefinitionPointer = RoomEnemySystem.KamerVerticalPlatformDefinition;
        kamer.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        kamer.CurrentInstruction = 0;
        kamer.Parameter1 = 0x0002;
        initialize(kamer);
        AssertEqual(VerticalShutterInstructionProgramDefinitions.KamerPlatform,
            kamer.CurrentInstruction,
            "Kamer vertical platform installs compiled loop");
        processArguments[0] = kamer;
        for (int frame = 0; frame < 5; frame++)
        {
            kamer.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(
                VerticalShutterInstructionProgramDefinitions.KamerPlatform + 4)),
            kamer.CurrentInstruction,
            "Kamer vertical platform loops to its first timed frame");

        AssertEqual(
            VerticalShutterInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "vertical-shutter spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled vertical-shutter mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xede9),
            "vertical-shutter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xedfb),
            "adjacent vertical-shutter initializer code is rejected as mechanics");

        _ = ProbeVerticalShutterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeVerticalShutterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "vertical-shutter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed vertical-shutter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Vertical-shutter instruction mechanics: eight compiled words, all three " +
            "real initializers, the Kamer loop, and five live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static int ProbeVerticalShutterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? VerticalShutterInstructionProgramDefinitions.Plain
                    : VerticalShutterInstructionProgramDefinitions.KamerPlatform);
        }
        return checksum;
    }

    private static ushort ReadVerticalShutterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class VerticalShutterInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (VerticalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled vertical-shutter mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < VerticalShutterInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(index);
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
