using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares both Rinka programs with the pinned ROM, then executes them through the
    /// production interpreter while every compiled mechanics byte is unavailable.
    /// </summary>
    private static void VerifyRinkaInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Rinka instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < RinkaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            RinkaInstructionMechanicsWord definition =
                RinkaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadRinkaProgramWord(rom, definition.Address),
                $"Rinka mechanics word $A2:{definition.Address:X4}");
        }

        var guarded = new RinkaInstructionReadGuard(rom);
        (RoomEnemySystem ordinarySystem, RoomEnemySlot ordinary) = RunRinkaProgram(
            guarded,
            RinkaInstructionProgramDefinitions.OrdinaryInitial,
            special: false);
        AssertEqual(RinkaEnemyFunction.AimDelay, ordinarySystem.RinkaStates[0]!.Function,
            "ordinary Rinka fire callback selects aim-delay AI");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.Invisible),
            "ordinary Rinka fire callback makes the actor visible");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "ordinary Rinka program does not enable off-screen processing");

        (RoomEnemySystem specialSystem, RoomEnemySlot special) = RunRinkaProgram(
            guarded,
            RinkaInstructionProgramDefinitions.SpecialInitial,
            special: true);
        AssertEqual(RinkaEnemyFunction.AimDelay, specialSystem.RinkaStates[0]!.Function,
            "special Rinka fire callback selects aim-delay AI");
        AssertTrue(!special.Properties.HasAny(EnemyProperties.Invisible),
            "special Rinka fire callback makes the actor visible");
        AssertTrue(special.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "special Rinka setup retains off-screen processing after firing");

        AssertEqual(
            RinkaInstructionProgramDefinitions.PresentationWordCount,
            guarded.ObservedPresentationWords.Count,
            "all live Rinka spritemap words remain cartridge reads");
        for (int index = 0;
             index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guarded.ObservedPresentationWords.Contains(address),
                $"production execution reads presentation word $A2:{address:X4}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids every compiled Rinka mechanics byte");

        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(0xb9e4),
            "interleaved Rinka spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(
                RinkaInstructionCodes.UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2),
            "unused Rinka conditional callback cannot acquire an invented operand");
        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside the translated Rinka programs fails loudly");

        _ = RinkaInstructionProgramDefinitions.ReadMechanicsWord(
            RinkaInstructionProgramDefinitions.OrdinaryInitial);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RinkaInstructionProgramDefinitions.ReadMechanicsWord(
                RinkaInstructionProgramDefinitions.OrdinaryInitial);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Rinka allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Rinka mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Rinka instruction mechanics: " +
            $"{RinkaInstructionProgramDefinitions.MechanicsWordCount} words, " +
            $"{RinkaInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap words, and both production programs pass with mechanics reads " +
            "forbidden.");
    }

    private static (RoomEnemySystem System, RoomEnemySlot Rinka) RunRinkaProgram(
        RinkaInstructionReadGuard bus,
        ushort initialPointer,
        bool special)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        RoomEnemySlot rinka = enemies.Slots[0];
        rinka.EnemyDefinitionPointer = RoomEnemySystem.RinkaDefinition;
        rinka.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        rinka.Parameter1 = special ? (ushort)1 : (ushort)0;
        rinka.CurrentInstruction = initialPointer;
        rinka.InstructionTimer = 1;

        var state = new RinkaEnemyState(rinka)
        {
            Function = RinkaEnemyFunction.WatchForLeavingViewport,
        };
        var states = (RinkaEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_rinkaStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments = [rinka, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        // One complete list is 127 timer frames plus zero-time callback/goto dispatch. The
        // margin proves that the native terminal goto loops rather than merely reaching it.
        for (int frame = 0; frame < 180; frame++)
            process.Invoke(enemies, arguments);

        return (enemies, rinka);
    }

    private static ushort ReadRinkaProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class RinkaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (RinkaInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Rinka mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RinkaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
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
