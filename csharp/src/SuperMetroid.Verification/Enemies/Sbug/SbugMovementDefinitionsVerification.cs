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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSbugMovementReadGuard(new TestAddressSpace()));
        Action<RoomEnemySlot> initialize = SbugBehavior.InitializeSbug;
        Action<RoomEnemySlot, SamusState, RoomLevelData?> runMain = enemies.SbugBehavior.RunSbugMain;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);

        for (ushort directionIndex = 0; directionIndex < 16; directionIndex++)
        {
            int address = instructionTable + (directionIndex >> 1) * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, NativeEnemyProgramOracle.EntryOf(SbugNativeListAddresses.Entries, SbugMovementDefinitions.FacingProgram(directionIndex)),
                $"compiled Sbug facing selector {directionIndex}");
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
            AssertEqual(unchecked((ushort)(selector * 2)), (state ?? throw new InvalidOperationException("Verification requires state.")).ForwardInstructionIndex,
                $"Sbug production facing index {selector}");
            AssertEqual(AnimationProgramCursor<SbugProgram>.Start(SbugMovementDefinitions.FacingProgram((ushort)(selector * 2))), state.Program,
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
            () => SbugMovementDefinitions.FacingProgram(16),
            "Sbug facing selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingProgram(ushort.MaxValue),
            "Sbug restored facing selector does not read adjacent code");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)7),
            "Sbug activation selector beyond authored table");

        Console.WriteLine(
            "Sbug movement definitions: eight facing loops, seven activation callbacks and every loop's run pass " +
            "with the selector tables forbidden.");
    }

    /// <summary>Rejects reads of the two migrated Sbug movement-selector tables.</summary>
    private static CartridgeReadGuard NewSbugMovementReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3a111 and < 0xa3a12f,
            address => $"Sbug movement attempted migrated selector read ${address:X6}.");

    /// <summary>Runs the retail-backed Sbug movement checks and program oracle.</summary>
    [VerificationCommand("--sbug-instruction-mechanics")]
    internal static int SbugInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(rom));
        Suite(nameof(SbugAnimationsVerification.VerifySbugAnimations), () => SbugAnimationsVerification.VerifySbugAnimations(rom));
        return 0;
    }
}
