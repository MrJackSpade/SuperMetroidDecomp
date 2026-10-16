using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>Checks the four native animation selectors and live instruction handoffs for both Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitionsVerification
{
    /// <summary>Compares both compiled phase tables with ROM and exercises all install and attack transitions with table reads forbidden.</summary>
    internal static void VerifySkreeMetareeAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3894e + index * 2)),
                NativeEnemyProgramOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationDefinitions.MetareeProgram(phase)),
                $"Metaree animation phase {phase}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3c69c + index * 2)),
                NativeEnemyProgramOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, SkreeMetareeAnimationDefinitions.SkreeProgram(phase)),
                $"Skree animation phase {phase}");
        }

        Suite(nameof(VerifyAllSkreeMetareeInstallHandoffs), () => VerifyAllSkreeMetareeInstallHandoffs());
        Suite(nameof(VerifyLiveMetareeAnimationHandoffs), () => VerifyLiveMetareeAnimationHandoffs(rom));
        Suite(nameof(VerifyLiveSkreeAnimationHandoffs), () => VerifyLiveSkreeAnimationHandoffs(rom));

        var invalid = (SkreeMetareeAnimationPhase)4;
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationDefinitions.MetareeProgram(invalid),
            "Metaree animation phase past table");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationDefinitions.SkreeProgram(invalid),
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
            var metareeState = new MetareeEnemyState(metareeSlot)
            {
                RequestedInstructionListIndex = phase,
                InstalledInstructionListIndex = DifferentSkreeMetareePhase(phase),
            };
            MetareeBehavior.InstallRequestedMetareeInstruction(metareeSlot, metareeState);
            AssertSkreeMetareeHandoff(
                metareeSlot,
                metareeState.InstalledInstructionListIndex,
                phase,
                metareeState.Program,
                SkreeMetareeAnimationDefinitions.MetareeProgram(phase),
                $"Metaree {phase}");

            var skreeSlot = NewSkreeMetareeAnimationSlot();
            var skreeState = new SkreeEnemyState(skreeSlot)
            {
                RequestedInstructionIndex = phase,
                InstalledInstructionIndex = DifferentSkreeMetareePhase(phase),
            };
            SkreeBehavior.InstallRequestedSkreeInstruction(skreeSlot, skreeState);
            AssertSkreeMetareeHandoff(
                skreeSlot,
                skreeState.InstalledInstructionIndex,
                phase,
                skreeState.Program,
                SkreeMetareeAnimationDefinitions.SkreeProgram(phase),
                $"Skree {phase}");
        }
    }

    /// <summary>Drives a live Metaree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveMetareeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        enemies.MetareeBehavior.InitializeMetaree(slot);
        MetareeEnemyState state = (enemies.MetareeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MetareeStates[0]."));
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        MetareeBehavior.RunMetareeIdle(slot, (state ?? throw new InvalidOperationException("Verification requires state.")), samus);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledInstructionListIndex,
                SkreeMetareeAnimationPhase.PreparingAttack,
                state.Program,
                SkreeMetareeAnimationDefinitions.MetareeProgram(SkreeMetareeAnimationPhase.PreparingAttack),
            "live Metaree preparation");

        state.AttackReady = true;
        enemies.MetareeBehavior.RunMetareePreparation(slot, state);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledInstructionListIndex,
                SkreeMetareeAnimationPhase.Diving,
                state.Program,
                SkreeMetareeAnimationDefinitions.MetareeProgram(SkreeMetareeAnimationPhase.Diving),
            "live Metaree dive");
    }

    /// <summary>Drives a live Skree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveSkreeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        SkreeBehavior.InitializeSkree(slot);
        SkreeEnemyState state = (enemies.SkreeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SkreeStates[0]."));
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
                slot,
                (state ?? throw new InvalidOperationException("Verification requires state.")).InstalledInstructionIndex,
                SkreeMetareeAnimationPhase.PreparingAttack,
                state.Program,
                SkreeMetareeAnimationDefinitions.SkreeProgram(SkreeMetareeAnimationPhase.PreparingAttack),
            "live Skree preparation");

        state.AttackReady = true;
        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
                slot,
                state.InstalledInstructionIndex,
                SkreeMetareeAnimationPhase.Diving,
                state.Program,
                SkreeMetareeAnimationDefinitions.SkreeProgram(SkreeMetareeAnimationPhase.Diving),
            "live Skree dive");
    }

    /// <summary>Creates an enemy system that rejects reads from the two migrated animation selector tables.</summary>
    private static RoomEnemySystem NewSkreeMetareeGuardedSystem(
        SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSkreeMetareeAnimationReadGuard(rom));
        return enemies;
    }

    /// <summary>Seeds nondefault instruction and timer values so installation must visibly replace them.</summary>
    private static RoomEnemySlot NewSkreeMetareeAnimationSlot() => new(0, EnemySlotFixtures.Registry)
    {
        InstructionTimer = NativeCountdown.Start(0x2222),
        Timer = NativeCountdown.Start(0x3333),
    };

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
        AnimationProgramCursor<SkreeMetareeProgram> program,
        SkreeMetareeProgram expectedProgram,
        string context)
    {
        AssertEqual(expectedPhase, installed, $"{context} installed phase");
        AssertEqual(AnimationProgramCursor<SkreeMetareeProgram>.Start(expectedProgram), program, $"{context} animation");
        AssertEqual(1, slot.InstructionTimer.RawValue, $"{context} instruction timer");
        AssertEqual(0, slot.Timer.RawValue, $"{context} loop timer");
    }

    /// <summary>Forbids runtime reads of bank-$A3 Metaree and Skree animation selector words.</summary>
    private static CartridgeReadGuard NewSkreeMetareeAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3894e and < 0xa38956 or >= 0xa3c69c and < 0xa3c6a4,
            address => $"Skree/Metaree attempted migrated selector read ${address:X6}.");
}
