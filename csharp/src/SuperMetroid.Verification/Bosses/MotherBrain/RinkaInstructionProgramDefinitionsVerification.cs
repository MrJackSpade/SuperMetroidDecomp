using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        (RoomEnemySystem ordinarySystem, RoomEnemySlot ordinary) = RunRinkaProgram(RinkaProgram.Ordinary, special: false);
        AssertEqual(RinkaEnemyFunction.AimDelay, (ordinarySystem.RinkaStates[0] ?? throw new InvalidOperationException("Verification requires ordinarySystem.RinkaStates[0].")).Function,
            "ordinary Rinka fire callback selects aim-delay AI");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.Invisible),
            "ordinary Rinka fire callback makes the actor visible");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "ordinary Rinka program does not enable off-screen processing");

        (RoomEnemySystem specialSystem, RoomEnemySlot special) = RunRinkaProgram(RinkaProgram.Special, special: true);
        AssertEqual(RinkaEnemyFunction.AimDelay, (specialSystem.RinkaStates[0] ?? throw new InvalidOperationException("Verification requires specialSystem.RinkaStates[0].")).Function,
            "special Rinka fire callback selects aim-delay AI");
        AssertTrue(!special.Properties.HasAny(EnemyProperties.Invisible),
            "special Rinka fire callback makes the actor visible");
        AssertTrue(special.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "special Rinka setup retains off-screen processing after firing");

        Suite(nameof(RinkaAnimationsVerification.VerifyRinkaAnimations), () => RinkaAnimationsVerification.VerifyRinkaAnimations(rom));
        Console.WriteLine("Rinka: both programs' hide, off-screen and fire cues and the program oracle pass.");
    }

    /// <summary>Runs one native-length Rinka loop and observes visibility, off-screen behavior, and aim-delay selection.</summary>
    private static RunRinkaProgramResult RunRinkaProgram(
        RinkaProgram program,
        bool special)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot rinka = enemies.SlotTable.Slots[0];
        rinka.Bind(EnemyDefinitionId.Rinka, rinka.Definition);
        rinka.Bind(rinka.DefinitionId, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        rinka.SetSpawnParameter1(special ? (ushort)1 : (ushort)0);
        rinka.InstructionTimer = WrappingCountdown.Start(1);

        var state = new RinkaEnemyState()
        {
            Function = RinkaEnemyFunction.WatchForLeavingViewport,
            Program = AnimationProgramCursor<RinkaProgram>.Start(program),
        };
        enemies.SlotTable.Slots[0].SetSpeciesState(state);

        object?[] arguments = [rinka, null, null, Position16<RoomPixels>.Zero, SnesButton.None];

        // One complete list is 127 timer frames plus zero-time callback/goto dispatch. The
        // margin proves that the native terminal goto loops rather than merely reaching it.
        for (int frame = 0; frame < 180; frame++)
            EnemyInstructionRunner.Process(arguments);

        return new RunRinkaProgramResult(enemies, rinka);
    }

    /// <summary>Runs the focused Rinka instruction check against the retail ROM when available.</summary>
    [VerificationCommand("--rinka-instruction-mechanics")]
    internal static int RinkaInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRinkaInstructionProgramDefinitions), () => VerifyRinkaInstructionProgramDefinitions());
        return 0;
    }
}
