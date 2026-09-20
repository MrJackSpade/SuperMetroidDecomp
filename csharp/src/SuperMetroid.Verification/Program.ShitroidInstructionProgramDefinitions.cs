using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShitroidInstructionProgramDefinitions()
    {
        VerifyShitroidInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyShitroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShitroidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ShitroidInstructionMechanicsWord definition =
                ShitroidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadShitroidInstructionWord(rom, definition.Address),
                $"Shitroid mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort randomNumber = 0;
        var guard = new ShitroidInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        type.GetField("_readRandomNumber", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => randomNumber));
        var initialize = type.GetMethod("InitializeShitroid", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot shitroid = enemies.Slots[0];
        shitroid.EnemyDefinitionPointer = RoomEnemySystem.ShitroidDefinition;
        shitroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(shitroid, 0);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "real Shitroid initializer installs compiled normal program");

        object?[] processArguments =
            [shitroid, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteShitroidProgram(
            enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.FinishDraining,
            callCount: 2);
        AssertEqual(ShitroidInstructionProgramDefinitions.Normal,
            shitroid.CurrentInstruction,
            "finish-draining program falls through to normal program");

        ExecuteShitroidProgram(
            enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Normal,
            callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Normal + 4)),
            shitroid.CurrentInstruction,
            "normal callback loops and installs its first repeated frame");

        ExecuteShitroidProgram(
            enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.LatchedOn,
            callCount: 5);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.LatchedOn + 4)),
            shitroid.CurrentInstruction,
            "latched callback loops and installs its first repeated frame");

        randomNumber = 0;
        ExecuteShitroidProgram(
            enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Remorse,
            callCount: 9);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "clear-high-bit remorse branch follows its compiled loop target");

        randomNumber = 0x8000;
        ExecuteShitroidProgram(
            enemies,
            process,
            processArguments,
            shitroid,
            ShitroidInstructionProgramDefinitions.Remorse,
            callCount: 21);
        AssertEqual(unchecked((ushort)(ShitroidInstructionProgramDefinitions.Remorse + 4)),
            shitroid.CurrentInstruction,
            "set-high-bit remorse path plays every trailing frame and loops");
        AssertEqual((ushort?)0x0052, enemies.LastShitroidSoundEffectLibrary2,
            "set-high-bit remorse branch publishes native Shitroid cry");

        AssertEqual(ShitroidInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Shitroid spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Shitroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Shitroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                ShitroidInstructionProgramDefinitions.FirstAdjacentCallbackCode),
            "adjacent Shitroid callback code is rejected as instruction mechanics");

        _ = ProbeShitroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShitroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shitroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Shitroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Shitroid instruction mechanics: thirty-five compiled words, the real " +
            "initializer, finish-draining fallthrough, normal/latched loops, both remorse " +
            "branches, native cry, and thirty live spritemap reads pass with mechanics " +
            "bytes forbidden.");
    }

    private static void ExecuteShitroidProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot shitroid,
        ushort program,
        int callCount)
    {
        shitroid.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            shitroid.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
    }

    private static int ProbeShitroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShitroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ShitroidInstructionProgramDefinitions.Normal
                    : ShitroidInstructionProgramDefinitions.RemorseLoopOpcode);
        }
        return checksum;
    }

    private static ushort ReadShitroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ShitroidInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ShitroidInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shitroid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShitroidInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
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
