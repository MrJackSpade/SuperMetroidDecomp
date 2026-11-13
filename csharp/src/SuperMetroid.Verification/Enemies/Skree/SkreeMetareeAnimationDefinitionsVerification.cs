using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>Checks the four native animation selectors and live instruction handoffs for both Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitionsVerification
{
    /// <summary>Compares both compiled phase tables with ROM and exercises all install and attack transitions with table reads forbidden.</summary>
    internal static void VerifySkreeMetareeAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3894e + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationChoices.MetareeAnimation(phase)),
                $"Metaree animation phase {phase}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3c69c + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationChoices.SkreeAnimation(phase)),
                $"Skree animation phase {phase}");
        }

        Suite(nameof(VerifyAllSkreeMetareeInstallHandoffs), () => VerifyAllSkreeMetareeInstallHandoffs());
        Suite(nameof(VerifyLiveMetareeAnimationHandoffs), () => VerifyLiveMetareeAnimationHandoffs());
        Suite(nameof(VerifyLiveSkreeAnimationHandoffs), () => VerifyLiveSkreeAnimationHandoffs());

        var invalid = (SkreeMetareeAnimationPhase)4;
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationChoices.MetareeAnimation(invalid),
            "Metaree animation phase past table");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationChoices.SkreeAnimation(invalid),
            "Skree animation phase past table");
        Console.WriteLine(
            "Skree/Metaree animation definitions: eight native selectors, all eight install handoffs, and both live attack transitions per family pass with pointer tables forbidden.");
    }

    /// <summary>Installs every requested phase from a different prior phase and checks each slot's pointer and timer reset.</summary>
    private static void VerifyAllSkreeMetareeInstallHandoffs()
    {

        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            var metareeSlot = NewSkreeMetareeAnimationSlot();
            var metareeState = new MetareeEnemyState
            {
                RequestedAnimation = phase,
                InstalledAnimation = DifferentSkreeMetareePhase(phase),
            };
            MetareeBehavior.StartRequestedMetareeAnimation(metareeSlot, metareeState);
            AssertSkreeMetareeHandoff(
                metareeSlot,
                metareeState.InstalledAnimation,
                phase,
                metareeState.Playback,
                SkreeMetareeAnimationChoices.MetareeAnimation(phase),
                $"Metaree {phase}");

            var skreeSlot = NewSkreeMetareeAnimationSlot();
            var skreeState = new SkreeEnemyState()
            {
                RequestedAnimation = phase,
                InstalledAnimation = DifferentSkreeMetareePhase(phase),
            };
            SkreeBehavior.StartRequestedSkreeAnimation(skreeSlot, skreeState);
            AssertSkreeMetareeHandoff(
                skreeSlot,
                skreeState.InstalledAnimation,
                phase,
                skreeState.Playback,
                SkreeMetareeAnimationChoices.SkreeAnimation(phase),
                $"Skree {phase}");
        }
    }

    /// <summary>Drives a live Metaree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveMetareeAnimationHandoffs()
    {
        var enemies = NewSkreeMetareeGuardedSystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        slot.Bind(EnemyDefinitionId.Metaree, slot.Definition);
        MetareeBehavior.InitializeMetaree(slot);
        MetareeEnemyState state = (enemies.MetareeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MetareeStates[0]."));
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        MetareeBehavior.RunMetareeIdle(slot, (state), samus);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledAnimation,
                SkreeMetareeAnimationPhase.PreparingAttack,
                state.Playback,
                SkreeMetareeAnimationChoices.MetareeAnimation(SkreeMetareeAnimationPhase.PreparingAttack),
            "live Metaree preparation");

        state.AttackReady = true;
        enemies.MetareeBehavior.RunMetareePreparation(slot, state);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledAnimation,
                SkreeMetareeAnimationPhase.Diving,
                state.Playback,
                SkreeMetareeAnimationChoices.MetareeAnimation(SkreeMetareeAnimationPhase.Diving),
            "live Metaree dive");
    }

    /// <summary>Drives a live Skree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveSkreeAnimationHandoffs()
    {
        var enemies = NewSkreeMetareeGuardedSystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        slot.Bind(EnemyDefinitionId.Skree, slot.Definition);
        SkreeBehavior.InitializeSkree(slot);
        SkreeEnemyState state = (enemies.SkreeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SkreeStates[0]."));
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledAnimation,
                SkreeMetareeAnimationPhase.PreparingAttack,
                state.Playback,
                SkreeMetareeAnimationChoices.SkreeAnimation(SkreeMetareeAnimationPhase.PreparingAttack),
            "live Skree preparation");

        state.AttackReady = true;
        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledAnimation,
                SkreeMetareeAnimationPhase.Diving,
                state.Playback,
                SkreeMetareeAnimationChoices.SkreeAnimation(SkreeMetareeAnimationPhase.Diving),
            "live Skree dive");
    }

    /// <summary>Creates an enemy system that rejects reads from the two migrated animation selector tables.</summary>
    private static RoomEnemySystem NewSkreeMetareeGuardedSystem(
)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>Seeds nondefault instruction and timer values so installation must visibly replace them.</summary>
    private static RoomEnemySlot NewSkreeMetareeAnimationSlot() =>
        new RoomEnemySlot(0, EnemySlotFixtures.Registry).WithAnimationClock(hold: 0x2222, repeats: 0x3333);

    /// <summary>Chooses an installed phase distinct from the requested one for each handoff fixture.</summary>
    private static SkreeMetareeAnimationPhase DifferentSkreeMetareePhase(
        SkreeMetareeAnimationPhase phase) =>
        phase == SkreeMetareeAnimationPhase.Idling
            ? SkreeMetareeAnimationPhase.PreparingAttack
            : SkreeMetareeAnimationPhase.Idling;

    /// <summary>Checks the installed phase and the freshly started animation plus both timer resets.</summary>
    private static void AssertSkreeMetareeHandoff(
        RoomEnemySlot slot,
        SkreeMetareeAnimationPhase installed,
        SkreeMetareeAnimationPhase expectedPhase,
        AnimationPlayback<SkreeMetareeSegment> playback,
        SkreeMetareeSegment expectedProgram,
        string context)
    {
        AssertEqual(expectedPhase, installed, $"{context} installed phase");
        AssertTrue(ReferenceEquals(SkreeMetareeAnimations.StartingWith(expectedProgram), playback.Animation) &&
            playback.Place == AnimationPlace.StartOfAnimation, $"{context} animation");
        AssertEqual(1, slot.AnimationClock.Hold.RawValue, $"{context} instruction timer");
        AssertEqual(0, slot.AnimationClock.Repeats.RawValue, $"{context} loop timer");
    }

}
