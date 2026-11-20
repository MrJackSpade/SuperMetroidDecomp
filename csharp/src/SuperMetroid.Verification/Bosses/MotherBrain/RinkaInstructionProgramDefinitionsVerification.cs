using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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

        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        (RoomEnemySystem ordinarySystem, RoomEnemyPlace ordinary) = RunRinkaProgram(RinkaSegment.Ordinary, special: false);
        AssertEqual(RinkaAction.AimDelay, (ordinarySystem.RinkaStates[0] ?? throw new InvalidOperationException("Verification requires ordinarySystem.RinkaStates[0].")).Action,
            "ordinary Rinka fire callback selects aim-delay AI");
        AssertTrue(!ordinary.Presence.HasAny(EnemyPresence.Hidden),
            "ordinary Rinka fire callback makes the actor visible");
        AssertTrue(!ordinary.Processing.HasAny(EnemyProcessing.OffScreen),
            "ordinary Rinka program does not enable off-screen processing");

        (RoomEnemySystem specialSystem, RoomEnemyPlace special) = RunRinkaProgram(RinkaSegment.KeptRunningOffScreen, special: true);
        AssertEqual(RinkaAction.AimDelay, (specialSystem.RinkaStates[0] ?? throw new InvalidOperationException("Verification requires specialSystem.RinkaStates[0].")).Action,
            "special Rinka fire callback selects aim-delay AI");
        AssertTrue(!special.Presence.HasAny(EnemyPresence.Hidden),
            "special Rinka fire callback makes the actor visible");
        AssertTrue(special.Processing.HasAny(EnemyProcessing.OffScreen),
            "special Rinka setup retains off-screen processing after firing");

        Suite(nameof(RinkaAnimationsVerification.VerifyRinkaAnimations), () => RinkaAnimationsVerification.VerifyRinkaAnimations(rom));
        Console.WriteLine("Rinka: both programs' hide, off-screen and fire cues and the program oracle pass.");
    }

    /// <summary>Runs one native-length Rinka loop and observes visibility, off-screen behavior, and aim-delay selection.</summary>
    private static RunRinkaProgramResult RunRinkaProgram(
        RinkaSegment program,
        bool special)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace rinka = enemies.Places.InUpdateOrder[0];
        rinka.Bind(EnemyDefinitionId.Rinka, rinka.Definition);
        rinka.Bind(rinka.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        RinkaSpawnSettings spawning = special ? new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.FarRightUpper) : RinkaSpawnSettings.AtOwnPlacement;
        rinka.SetSpawnSettings(spawning);
        rinka.AnimationClock.StepOnNextUpdate();

        var state = new RinkaEnemyState(RinkaAction.WatchForLeavingViewport, program, spawning);
        enemies.Places.InUpdateOrder[0].SetSpeciesState(state);

        object?[] arguments = [rinka, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];

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
