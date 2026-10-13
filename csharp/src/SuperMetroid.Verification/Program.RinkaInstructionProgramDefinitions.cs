using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static CompiledEnemyVisualSelectorAssertionsVerification;

/// <summary>Checks ordinary and special Rinka instruction loops, fire callbacks, and visual selectors.</summary>
internal static class RinkaInstructionProgramDefinitionsVerification
{
    /// <summary>
    /// Compares both Rinka programs with the pinned ROM, then executes them through the
    /// production interpreter while every compiled mechanics byte is unavailable.
    /// </summary>
    internal static void VerifyRinkaInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Rinka instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        for (int index = 0;
             index < RinkaInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RinkaInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | definition.Address),
                $"Rinka mechanics word $A2:{definition.Address:X4}");
        }

        var guarded = NewRinkaInstructionReadGuard(rom);
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

        AssertEqual(0, guarded.ObservedWords.Count,
            "Rinka execution uses compiled spritemap selectors");
        for (int index = 0;
             index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Rinka,
                0xa2, address, $"Rinka $A2:{address:X4}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids every compiled Rinka mechanics byte");

        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(0xb9e4),
            "interleaved Rinka spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(
                (ushort)RinkaInstruction.UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2),
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
            $"{RinkaInstructionProgramDefinitionsTooling.MechanicsWordCount} words, " +
            $"{RinkaInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap words, and both production programs pass with mechanics reads " +
            "forbidden.");
    }

    /// <summary>Runs one native-length Rinka loop and observes visibility, off-screen behavior, and aim-delay selection.</summary>
    private static (RoomEnemySystem System, RoomEnemySlot Rinka) RunRinkaProgram(
        CartridgeReadGuard bus,
        ushort initialPointer,
        bool special)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        RoomEnemySlot rinka = enemies.SlotTable.Slots[0];
        rinka.EnemyDefinitionPointer = EnemyDefinitionId.Rinka;
        rinka.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        rinka.Parameter1 = special ? (ushort)1 : (ushort)0;
        rinka.CurrentInstruction = initialPointer;
        rinka.InstructionTimer = NativeCountdown.Start(1);

        var state = new RinkaEnemyState(rinka)
        {
            Function = RinkaEnemyFunction.WatchForLeavingViewport,
        };
        var states = (RinkaEnemyState?[])enemies.RinkaBehavior._rinkaStates;
        states[0] = state;
        enemies.BindFixtureBus(bus);

        object?[] arguments = [rinka, null, null, Position16<RoomPixels>.Zero, (ushort)0];

        // One complete list is 127 timer frames plus zero-time callback/goto dispatch. The
        // margin proves that the native terminal goto loops rather than merely reaching it.
        for (int frame = 0; frame < 180; frame++)
            EnemyInstructionRunner.Process(enemies, arguments);

        return (enemies, rinka);
    }

    /// <summary>Blocks compiled mechanics and records any visual operand reads during production execution.</summary>
    private static CartridgeReadGuard NewRinkaInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(RinkaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Rinka mechanics byte ${address:X6}.")
            .ObserveWords(0xa20000, RinkaInstructionProgramDefinitions.PresentationWordCount,
                RinkaInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the focused Rinka instruction check against the retail ROM when available.</summary>
    [VerificationCommand("--rinka-instruction-mechanics")]
    internal static int RinkaInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRinkaInstructionProgramDefinitions), () => VerifyRinkaInstructionProgramDefinitions());
        return 0;
    }
}
