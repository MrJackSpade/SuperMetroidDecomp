using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.Nuclear;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Nuclear Waffle sweep geometry and body-loop mechanics against bank-$A6 ROM and production enemy state.</summary>
internal static class NuclearWaffleDefinitionsVerification
{
    /// <summary>Compares both sweep directions and the full body loop with retail data while guarding migrated geometry and program reads.</summary>
    internal static void VerifyNuclearWaffleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int endpointTable = 0xa695f6;
        const int spacingTable = 0xa695fe;
        const int thresholdTable = 0xa69606;

        for (byte direction = 0; direction < 2; direction++)
        {
            int offset = direction * 4;
            NuclearWaffleSweepDefinition definition = NuclearWaffleDefinitions.Sweep(direction);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((endpointTable + offset)), definition.StartAngle,
                $"Nuclear Waffle start angle {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((endpointTable + offset + 2)), definition.EndAngle,
                $"Nuclear Waffle end angle {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((spacingTable + offset))),
                definition.SegmentSpacing,
                $"Nuclear Waffle segment spacing {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((spacingTable + offset + 2))),
                definition.InterleavedSegmentOffset,
                $"Nuclear Waffle interleaved offset {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((thresholdTable + offset)),
                definition.SecondTurnThreshold,
                $"Nuclear Waffle second turn threshold {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((thresholdTable + offset + 2)),
                definition.FirstTurnThreshold,
                $"Nuclear Waffle first turn threshold {direction}");

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = enemies.NuclearWaffleBehavior.InitializeNuclearWaffle;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Puromi, slot.Definition);
            slot.SetSpawnParameter1(0x2001);
            slot.SetSpawnParameter2((ushort)(0x2000 | direction));
            slot.Position = slot.Position.WithWhole(new(0x0180, 0x0200));
            initialize(slot);

            NuclearWaffleEnemyState state = (enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NuclearWaffleStates[0]."));
            AssertEqual(definition.StartAngle, state.SweepStartAngle,
                $"Nuclear Waffle production start angle {direction}");
            AssertEqual(definition.StartAngle, state.CurrentAngle,
                $"Nuclear Waffle production current angle {direction}");
            AssertEqual(definition.EndAngle, state.SweepEndAngle,
                $"Nuclear Waffle production end angle {direction}");
            AssertEqual(definition.SegmentSpacing, state.SegmentSpacing,
                $"Nuclear Waffle production spacing {direction}");
            AssertEqual(definition.InterleavedSegmentOffset, state.InterleavedSegmentOffset,
                $"Nuclear Waffle production interleaved offset {direction}");
            AssertEqual(definition.SecondTurnThreshold, state.SecondTurnThreshold,
                $"Nuclear Waffle production second threshold {direction}");
            AssertEqual(definition.FirstTurnThreshold, state.FirstTurnThreshold,
                $"Nuclear Waffle production first threshold {direction}");
            AssertEqual(AnimationProgramCursor<NuclearWaffleProgram>.Start(NuclearWaffleProgram.BodyLoop), state.Program,
                $"Nuclear Waffle production initial animation {direction}");
            AssertEqual(4, state.ProjectileSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production projectile links {direction}");
            AssertEqual(3, state.SpriteSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production sprite links {direction}");
        }

        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(2),
            "Nuclear Waffle direction beyond authored table");
        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(byte.MaxValue),
            "Nuclear Waffle restored direction does not read adjacent enemy code");

        Console.WriteLine(
            "Nuclear Waffle definitions: twelve geometry words and both complete production " +
            "initializers pass with runtime ROM reads forbidden.");
    }

    /// <summary>
    /// Drives the production main AI through one forward sweep at one and a half angle units per update. The wait of
    /// zero expires on the first update and restarts the sweep at 240 with a zero fraction; each update then adds the
    /// 16.16 angular speed, carrying the half unit into the whole angle every second update. The head crosses the
    /// rising threshold (256) on sweep update 11, queuing the turn sound, and the falling threshold (384) on update 96
    /// silently. The sweep ends on update 163, the first whose last damaging link (84 units behind the head) reaches the
    /// end angle 400, with the head clamped there and the angle advanced once more to 486.
    /// </summary>
    internal static void VerifyNuclearWaffleSweep()
    {
        const int oneUnit = 65536;
        const int speedIndex = 24;
        const int radius = 32;
        const int startAngle = 240;
        const int endAngle = 400;
        const int finishingUpdate = 163;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puromi, slot.Definition);
        slot.SetSpawnParameter1(LittleEndianBytes.Word(speedIndex, radius));
        slot.SetSpawnParameter2(LittleEndianBytes.Word(1, 0));
        slot.Position = slot.Position.WithWhole(new(384, 512));
        enemies.NuclearWaffleBehavior.InitializeNuclearWaffle(slot);
        NuclearWaffleEnemyState state = enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Nuclear Waffle has no state.");
        AssertTrue(state.SweepsForward, "Nuclear Waffle direction one sweeps forward");
        AssertEqual(SignedSixteenSixteen.FromRaw(oneUnit + oneUnit / 2), state.AngularSpeed, "Nuclear Waffle speed record 24 is 1.5 units per update");

        enemies.NuclearWaffleBehavior.RunNuclearWaffleMainAi(slot, Frame());
        AssertEqual(NuclearWaffleEnemyFunction.Sweeping, state.Function, "Nuclear Waffle zero wait expires on the first update");
        AssertEqual(SignedSixteenSixteen.FromParts(startAngle, 0), state.SweepAngle, "Nuclear Waffle sweep restarts at its start angle");

        for (int update = 0; update <= finishingUpdate; update++)
        {
            if (update == 96)
                enemies.NuclearWaffleBehavior.BeginEnemyFrame(new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState() });
            enemies.NuclearWaffleBehavior.RunNuclearWaffleMainAi(slot, Frame());
            AssertEqual(SignedSixteenSixteen.FromRaw(startAngle * oneUnit + (update + 1) * (oneUnit + oneUnit / 2)), state.SweepAngle,
                $"Nuclear Waffle sweep angle after sweep update {update}");
            AssertEqual(update == finishingUpdate ? NuclearWaffleEnemyFunction.Waiting : NuclearWaffleEnemyFunction.Sweeping, state.Function,
                $"Nuclear Waffle sweep finishes only on update {finishingUpdate} (update {update})");
            if (update == 10)
            {
                AssertEqual(NuclearWaffleTurnState.BeforeTurns, state.HeadOrientationState, "Nuclear Waffle head before the rising threshold");
                AssertEqual<SoundEffectId?>(null, enemies.NuclearWaffleBehavior.LastNuclearWaffleSoundEffect, "Nuclear Waffle is silent before any turn");
            }
            if (update == 11)
            {
                AssertEqual(NuclearWaffleTurnState.PastRisingThreshold, state.HeadOrientationState, "Nuclear Waffle head crosses the rising threshold");
                AssertEqual<SoundEffectId?>(NuclearWaffleDefinitions.TurnSoundEffect, enemies.NuclearWaffleBehavior.LastNuclearWaffleSoundEffect,
                    "Nuclear Waffle rising turn queues the turn sound");
            }
            if (update == 96)
            {
                AssertEqual(NuclearWaffleTurnState.PastFallingThreshold, state.HeadOrientationState, "Nuclear Waffle head crosses the falling threshold");
                AssertEqual<SoundEffectId?>(null, enemies.NuclearWaffleBehavior.LastNuclearWaffleSoundEffect, "Nuclear Waffle falling turn is silent");
            }
        }

        AssertEqual(486, state.CurrentAngle, "Nuclear Waffle angle advances once more on the finishing update");
        AssertEqual(state.Origin + EnemyMath.CircleOffset(new ByteAngle(unchecked((byte)endAngle)), radius), slot.Position.Whole, "Nuclear Waffle head is clamped to the end angle");
        Console.WriteLine("  Nuclear Waffle sweep: the 16.16 angle carries, both turns latch with the rising sound only, and the last link ends the sweep.");

        static ClockedEnemyAiFrame Frame() => new(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState() },
            new FrameCounters(0));
    }

    /// <summary>Runs the Nuclear Waffle geometry and body-animation verification command.</summary>
    [VerificationCommand("--nuclear-waffle-instruction-mechanics")]
    internal static int NuclearWaffleInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(rom));
        Suite(nameof(VerifyNuclearWaffleSweep), () => VerifyNuclearWaffleSweep());
        Suite(nameof(NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations),
            () => NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations(rom));
        return 0;
    }
}
