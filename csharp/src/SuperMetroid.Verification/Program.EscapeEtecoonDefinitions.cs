using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeEtecoonDefinitions(SuperMetroidAddressSpace rom)
    {
        const int xTable = 0xb3e718;
        const int yTable = 0xb3e71e;
        const int preInstructionTable = 0xb3e724;
        const int instructionTable = 0xb3e72a;
        const int speedTable = 0xb3e730;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (int role = 0; role < 3; role++)
        {
            ushort parameter = unchecked((ushort)(role * 2));
            EscapeEtecoonInitialization initialization =
                EscapeEtecoonDefinitions.Initialization(parameter);
            AssertEqual(ReadWord(rom, xTable + parameter), initialization.XPosition,
                $"escape Etecoon role {role} X");
            AssertEqual(ReadWord(rom, yTable + parameter), initialization.YPosition,
                $"escape Etecoon role {role} Y");
            AssertEqual((EscapeEtecoonPreInstruction)ReadWord(
                    rom,
                    preInstructionTable + parameter),
                initialization.PreInstruction,
                $"escape Etecoon role {role} pre-instruction");
            AssertEqual(ReadWord(rom, instructionTable + parameter),
                initialization.InstructionList,
                $"escape Etecoon role {role} instruction");
            AssertEqual(ReadWord(rom, speedTable + parameter),
                initialization.HorizontalSpeed,
                $"escape Etecoon role {role} speed");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(6),
            "escape Etecoon selector after authored roles");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(0xffff),
            "escape Etecoon wrapping restored selector");

        var guarded = new EscapeEtecoonDefinitionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeEscapeEtecoon",
            instanceFlags)!;
        for (ushort parameter = 0; parameter < 6; parameter++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
            typeof(RoomEnemySystem).GetField("_hasEvent", instanceFlags)!
                .SetValue(enemies, (Func<EventNumber, bool>)(_ => false));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = parameter;
            slot.XPosition = 0xaaaa;
            slot.YPosition = 0xbbbb;
            initialize.Invoke(enemies, [slot]);

            int role = parameter >> 1;
            EscapeEtecoonInitialization expected =
                EscapeEtecoonDefinitions.Initialization(parameter);
            EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
                throw new InvalidDataException("Escape Etecoon initializer omitted typed state.");
            AssertEqual(expected.XPosition, slot.XPosition,
                $"production escape Etecoon selector {parameter} X");
            AssertEqual(expected.YPosition, slot.YPosition,
                $"production escape Etecoon selector {parameter} Y");
            AssertEqual(expected.PreInstruction, state.PreInstruction,
                $"production escape Etecoon selector {parameter} pre-instruction");
            AssertEqual(expected.InstructionList, slot.CurrentInstruction,
                $"production escape Etecoon selector {parameter} instruction");
            AssertEqual(expected.HorizontalSpeed, state.HorizontalSpeed,
                $"production escape Etecoon selector {parameter} speed");
            AssertEqual((EscapeEtecoonRole)(role * 2), (EscapeEtecoonRole)(parameter & 0xfffe),
                $"escape Etecoon selector {parameter} role mask");
        }

        Console.WriteLine(
            "Escape Etecoon definitions: fifteen native values and all six masked retail selectors pass with the authored tables forbidden during production initialization; malformed restored selectors fail explicitly.");
    }

    private static void VerifyEscapeEtecoonInstructionProgramDefinitions()
    {
        VerifyEscapeEtecoonInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyEscapeEtecoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EscapeEtecoonInstructionMechanicsWord definition =
                EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadEscapeEtecoonInstructionWord(rom, 0xb30000 | definition.Address),
                $"escape Etecoon mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new EscapeEtecoonDefinitionReadGuard(rom);
        RoomEnemySystem enemies = CreateEscapeEtecoonProgramSystem(guard, flags);
        RoomEnemySlot etecoon = enemies.Slots[0];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        ushort[] programs =
        [
            EscapeEtecoonInstructionProgramDefinitions.RunningLeftLowTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningLeftHighTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningRightLowTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningRightHighTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningForEscape,
            EscapeEtecoonInstructionProgramDefinitions.Stationary,
        ];
        foreach (ushort program in programs)
        {
            etecoon.CurrentInstruction = program;
            RunForcedEscapeEtecoonInstructions(process, enemies, etecoon, 7);
        }

        EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
            throw new InvalidDataException("Escape Etecoon test lost typed state.");
        ushort xBeforeGratitude = etecoon.XPosition;
        etecoon.CurrentInstruction =
            EscapeEtecoonInstructionProgramDefinitions.ExpressGratitudeThenEscape;
        RunForcedEscapeEtecoonInstructions(process, enemies, etecoon, 40);
        AssertTrue(unchecked((short)(etecoon.XPosition - xBeforeGratitude)) < 0,
            "escape Etecoon gratitude program applies its repeated left displacement");
        AssertEqual(EscapeEtecoonPreInstruction.EscapeRight, state.PreInstruction,
            "escape Etecoon gratitude program hands off to rightward escape");

        AssertEqual(EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live escape-Etecoon spritemap words remain cartridge reads");
        for (int index = 0;
             index < EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads escape-Etecoon presentation word $B3:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"escape-Etecoon spritemap $B3:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled escape-Etecoon mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeEtecoonInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent escape-Etecoon callback code is rejected as mechanics");

        _ = ProbeEscapeEtecoonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEscapeEtecoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "escape-Etecoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed escape-Etecoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Escape Etecoon instruction mechanics: 63 compiled words, all seven live " +
            "programs and 30 spritemap reads pass with mechanics bytes forbidden.");
    }

    private static RoomEnemySystem CreateEscapeEtecoonProgramSystem(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_hasEvent", flags)!
            .SetValue(enemies, (Func<EventNumber, bool>)(_ => false));
        RoomEnemySlot etecoon = enemies.Slots[0];
        etecoon.EnemyDefinitionPointer = RoomEnemySystem.EscapeEtecoonDefinition;
        etecoon.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
        etecoon.Parameter1 = 0;
        typeof(RoomEnemySystem).GetMethod("InitializeEscapeEtecoon", flags)!
            .Invoke(enemies, [etecoon]);
        return enemies;
    }

    private static void RunForcedEscapeEtecoonInstructions(
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot etecoon,
        int steps)
    {
        object?[] arguments =
            [etecoon, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int step = 0; step < steps; step++)
        {
            etecoon.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static ushort ReadEscapeEtecoonInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static int ProbeEscapeEtecoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeEtecoonInstructionProgramDefinitions.Stationary);
        }
        return checksum;
    }

    private sealed class EscapeEtecoonDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EscapeEtecoonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled escape-Etecoon mechanics byte ${address:X6}.");
            }
            if (address is >= 0xb3e718 and < 0xb3e736)
            {
                throw new InvalidOperationException(
                    $"Escape Etecoon attempted migrated definition read ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = EscapeEtecoonInstructionProgramDefinitions
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
