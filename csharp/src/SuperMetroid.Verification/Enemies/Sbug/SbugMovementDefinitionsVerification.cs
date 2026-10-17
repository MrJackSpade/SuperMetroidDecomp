using SuperMetroid.Core.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Sbug;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-word and guarded-execution checks for compiled Sbug movement programs.</summary>
internal static class SbugMovementDefinitionsVerification
{
    /// <summary>Checks compiled Sbug facing, activation, and instruction data against retail words, then runs every facing loop with migrated reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Sbug movement definitions")]
    internal static void VerifySbugMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa3a111;
        const int activationTable = 0xa3a121;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = SbugBehavior.InitializeSbug;
        Action<RoomEnemySlot, SamusState, RoomLevelData?> runMain = enemies.SbugBehavior.RunSbugMain;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);

        for (int facing = 0; facing < 8; facing++)
        {
            int address = instructionTable + facing * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, NativeEnemyProgramOracle.EntryOf(SbugNativeListAddresses.Entries, SbugMovementDefinitions.FacingProgram(facing)),
                $"compiled Sbug facing selector {facing}");
        }

        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)behavior),
                $"compiled Sbug activation selector {behavior}");
        }

        for (ushort selector = 0; selector < 8; selector++)
        {
            byte angle = unchecked((byte)(0x30 + selector * 0x20));
            slot.Parameter1 = (ushort)(angle << 8 | 1);
            slot.Parameter2 = 1;
            initialize(slot);

            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + selector * 2);
            SbugEnemyState state = (enemies.SbugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SbugStates[0]."));
            AssertEqual(selector, (state ?? throw new InvalidOperationException("Verification requires state.")).ForwardFacing,
                $"Sbug production facing {selector}");
            AssertEqual(AnimationProgramCursor<SbugProgram>.Start(SbugMovementDefinitions.FacingProgram(selector)), state.Program,
                $"Sbug production facing animation {selector}");
            AssertEqual(native, NativeEnemyProgramOracle.EntryOf(SbugNativeListAddresses.Entries, state.Program.Program),
                $"Sbug production facing list {selector}");
            // Each loop shows its four frames five updates apiece, then its goto shows the first again.
            ushort[] loop = SbugAnimationsConstants.Spritemaps(state.Program.Program).ToArray();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [5, 5, 5, 5, 5]),
                EnemyInstructionRunner.ShownFrames(slot, 21), $"Sbug facing loop {selector}");
        }

        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
        };
        slot.Position = slot.Position.WithWhole(samus.Position.Whole);
        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            SbugEnemyState state = (enemies.SbugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SbugStates[0]."));
            (state ?? throw new InvalidOperationException("Verification requires state.")).Function = SbugEnemyFunction.WaitForSamus;
            slot.Parameter2 = (ushort)(behavior << 8 | 1);
            runMain(slot, samus, null);

            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, state.Function, $"Sbug production activation {behavior}");
        }

        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingProgram(8),
            "Sbug facing selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingProgram(-1),
            "Sbug facing selector below the authored table");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)7),
            "Sbug activation selector beyond authored table");

        Console.WriteLine(
            "Sbug movement definitions: eight facing loops, seven activation callbacks and every loop's run pass " +
            "with the selector tables forbidden.");
    }

    /// <summary>
    /// The production random-direction chooser adds an unsigned byte turn to the custom heading's whole variable word:
    /// from heading 240, random 96 turns by 32, so the word becomes 272 with the whole turn carried into its high byte,
    /// and the heading, its low byte, is 16, facing down-right. A later reversal, with Samus at least six blocks away on
    /// both axes, advances that facing by two and folds it into the first four facings: down-right becomes up-right.
    /// </summary>
    internal static void VerifySbugCustomHeadingCarriesAndFolds()
    {
        var enemies = new RoomEnemySystem();
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 96);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);
        slot.Parameter1 = 1;
        slot.Parameter2 = 1;
        slot.Position = slot.Position.WithWhole(new(512, 512));
        SbugBehavior.InitializeSbug(slot);
        SbugEnemyState state = SbugBehavior.RequireSbugState(slot);
        state.FaceCustomHeading(SnesAngle.FromTableIndex(240));
        state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(Position16<RoomPixels>.Zero) };

        enemies.SbugBehavior.RunSbugMain(slot, samus, null);
        AssertEqual(272, slot.VariableC, "the turn carries a whole turn into the heading word's high byte");
        AssertEqual(SnesAngle.FromTableIndex(16), state.CustomHeading, "the heading is the word's low byte");
        AssertEqual(7, state.CustomFacing, "heading 16 faces down-right");
        AssertEqual(SbugEnemyFunction.MoveStraightAndReverseWhenFar, state.Function, "the chooser starts a reversal-aware leg");

        state.StraightMovementTimer = NativeCountdown.Start(512);
        enemies.SbugBehavior.RunSbugMain(slot, samus, null);
        AssertEqual(1, state.CustomFacing, "the reversal folds facing 7 into facing 1");
        AssertEqual(AnimationProgramCursor<SbugProgram>.Start(SbugMovementDefinitions.FacingProgram(1)), state.Program,
            "the folded facing installs its loop");
        Console.WriteLine("  Sbug custom heading: turns carry into the heading word and a reversal folds the facing.");
    }


    /// <summary>Runs the retail-backed Sbug movement checks and program oracle.</summary>
    [VerificationCommand("--sbug-instruction-mechanics")]
    internal static int SbugInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(rom));
        Suite(nameof(VerifySbugCustomHeadingCarriesAndFolds), () => VerifySbugCustomHeadingCarriesAndFolds());
        Suite(nameof(SbugAnimationsVerification.VerifySbugAnimations), () => SbugAnimationsVerification.VerifySbugAnimations(rom));
        return 0;
    }
}
