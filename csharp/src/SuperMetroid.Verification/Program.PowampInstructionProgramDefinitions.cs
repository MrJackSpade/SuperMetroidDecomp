using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPowampInstructionProgramDefinitions()
    {
        VerifyPowampInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPowampInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PowampInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PowampInstructionMechanicsWord definition =
                PowampInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPowampInstructionWord(rom, definition.Address),
                $"Powamp mechanics word $A8:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PowampInstructionReadGuard(rom);
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
        ExecutePowampInstructionCalls(enemies, process, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodySlow + 4)),
            body.CurrentInstruction,
            "Powamp slow body loop completes its native goto and first repeated frame");

        body.CurrentInstruction = PowampInstructionProgramDefinitions.BodyFast;
        ExecutePowampInstructionCalls(enemies, process, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodyFast + 4)),
            body.CurrentInstruction,
            "Powamp fast body loop completes its native goto and first repeated frame");

        object?[] balloonArguments =
            [balloon, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonInflate0;
        ExecutePowampInstructionCalls(enemies, process, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonInflate2 + 4)),
            balloon.CurrentInstruction,
            "Powamp inflate program reaches terminal sleep");

        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonStartSinking;
        ExecutePowampInstructionCalls(enemies, process, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonDeflated + 4)),
            balloon.CurrentInstruction,
            "Powamp deflate program falls through to shared deflated sleep");

        AssertEqual(PowampInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Powamp spritemap operands remain cartridge reads");
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
            "inflate/deflate fallthrough programs, and 12 live spritemap reads pass " +
            "with mechanics bytes forbidden.");
    }

    private static void ExecutePowampInstructionCalls(
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
        }
    }

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

    private static ushort ReadPowampInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class PowampInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PowampInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
